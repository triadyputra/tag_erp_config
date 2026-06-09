using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using tagApiKonfigurasi.Data;
using tagApiKonfigurasi.Model;
using tagApiKonfigurasi.Model.DTO;
using tagApiKonfigurasi.Model.DTO.Mobile;
using tagApiKonfigurasi.Model.Konfigurasi;
using tagApiKonfigurasi.Services.EmployeeLogin;

namespace tagApiKonfigurasi.Services.Mobile
{
    public class RepoMobile : IRepoMobile
    {
        private readonly DapperSistagHrdContext _context;
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly IEmployeeLoginEligibilityService _loginEligibility;

        public RepoMobile(
            DapperSistagHrdContext context,
            ApplicationDbContext db,
            IConfiguration config,
            IEmployeeLoginEligibilityService loginEligibility)
        {
            _context = context;
            _db = db;
            _config = config;
            _loginEligibility = loginEligibility;
        }

        public async Task<MobileLoginResult> Login(LoginRequestDto request, string ip, string device, string modul)
        {
            //var sql = @"
            //SELECT 
            //    a.NOKTP       AS NoKtp,
            //    a.NIKSISTAG   AS NIKSistag,
            //    a.NMKARYAWAN  AS Nama,
            //    a.NMJABATAN   AS Jabatan,
            //    a.NMDIVISI    AS Divisi,
            //    a.TMT,
            //    a.NMCABANG    AS Cabang,
            //    b.FOTO        AS PHOTO 
            //FROM HRDTAG.dbo.v_MASTERPEGAWAI a
            //LEFT JOIN HRDTAG.dbo.MST_KTP b 
            //    ON a.NOKTP COLLATE DATABASE_DEFAULT = b.NOKTP COLLATE DATABASE_DEFAULT
            //WHERE a.NOKTP = @Username 
            //  AND COALESCE(b.Password, a.NIKSISTAG) = @Password";

            //using var conn = _context.CreateConnection();

            //var userDb = await conn.QueryFirstOrDefaultAsync<UserDbDto>(sql, new
            //{
            //    Username = request.Username,
            //    Password = request.Password
            //});

            //if (userDb == null)
            //    return null;



            var sql = @"
                SELECT 
                    a.NOKTP       AS NoKtp,
                    a.NIKSISTAG   AS NIKSistag,
                    a.NMKARYAWAN  AS Nama,
                    a.NMJABATAN   AS Jabatan,
                    a.NMDIVISI    AS Divisi,
                    a.TMT,
                    a.NMCABANG    AS Cabang,
                    b.FOTO        AS PHOTO,
                    b.Password
                FROM HRDTAG.dbo.v_MASTERPEGAWAI a
                LEFT JOIN HRDTAG.dbo.MST_KTP b 
                    ON a.NOKTP COLLATE DATABASE_DEFAULT = b.NOKTP COLLATE DATABASE_DEFAULT
                WHERE a.NOKTP = @Username";

            using var conn = _context.CreateConnection();

            var userDb = await conn.QueryFirstOrDefaultAsync<UserDbDto>(sql, new
            {
                Username = request.Username,
                Password = request.Password
            });

            if (userDb == null)
                return MobileLoginResult.CredentialFailure();

            //================= VALIDASI PASSWORD =================
            var currentPassword = (userDb.Password ?? userDb.NIKSistag)?.ToString()?.Trim();

            if (string.IsNullOrEmpty(currentPassword) ||
                !string.Equals(currentPassword, request.Password?.Trim()))
                return MobileLoginResult.CredentialFailure();

            var eligibility = await _loginEligibility.ValidateAsync(
                userDb.NoKtp,
                LoginEligibilityMode.MobileGrace);

            if (!eligibility.IsEligible)
            {
                await SaveAuditLogin(
                    userDb.NoKtp,
                    userDb.Nama,
                    ip,
                    device,
                    false,
                    eligibility.Message,
                    modul);

                return MobileLoginResult.EligibilityFailure(eligibility.Message!);
            }

            // =========================
            // SINGLE SESSION (per user + modul)
            // =========================
            var session = await _db.UserSession
                .FirstOrDefaultAsync(x => x.Username == userDb.Nama && x.Modul == modul);

            if (session == null)
            {
                session = new UserSession
                {
                    Username = userDb.Nama,
                    Modul = modul,
                    SessionVersion = 1
                };
                _db.UserSession.Add(session);
            }
            else
            {
                session.SessionVersion += 1;
            }

            await _db.SaveChangesAsync();

            var accessToken = GenerateToken(userDb, modul, session.SessionVersion);
            var refreshToken = GenerateRefreshToken(userDb.NoKtp, modul);

            _db.RefreshToken.Add(refreshToken);
            await _db.SaveChangesAsync();

            // =========================
            // AUDIT LOGIN
            // =========================
            var auditId = await SaveAuditLogin(
                userDb.NoKtp,
                userDb.Nama,
                ip,
                device,
                true,
                failReason: null,
                modul: modul,
                refreshToken: refreshToken.Token
            );

            string? photoBase64 = userDb.Photo is { Length: > 0 }
                ? PhotoCompressionHelper.ToMobileBase64(userDb.Photo)
                : null;

            var user = new UserInfoDto
            {
                NoKtp = userDb.NoKtp,
                NIKSistag = userDb.NIKSistag,
                Nama = userDb.Nama,
                Jabatan = userDb.Jabatan,
                Divisi = userDb.Divisi,
                TMT = userDb.TMT,
                Cabang = userDb.Cabang,
                Photo = photoBase64
            };

            return MobileLoginResult.Success(new LoginResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiredAt = DateTime.UtcNow.AddMinutes(10),
                User = user
            });
        }

        public async Task<LoginResponseDto?> Refresh(string refreshToken, string modul)
        {

            var cleanToken = refreshToken.Trim();

            var storedToken = await _db.RefreshToken
                .FirstOrDefaultAsync(x => x.Token == cleanToken && x.Modul == modul);

            if (storedToken == null ||
                storedToken.ExpiredAt < DateTime.UtcNow ||
                storedToken.IsUsed ||
                storedToken.IsRevoked)
            {
                return null;
            }

            // tandai token lama sudah dipakai (mirip AuthController)
            storedToken.IsUsed = true;

            var sql = @"SELECT 
                a.NOKTP       AS NoKtp,
                a.NIKSISTAG   AS NIKSistag,
                a.NMKARYAWAN  AS Nama,
                a.NMJABATAN   AS Jabatan,
                a.NMDIVISI    AS Divisi,
                a.TMT,
                a.NMCABANG    AS Cabang,
                b.FOTO        AS PHOTO 
            FROM HRDTAG.dbo.v_MASTERPEGAWAI a
            LEFT JOIN HRDTAG.dbo.MST_KTP b 
            ON a.NOKTP COLLATE DATABASE_DEFAULT = b.NOKTP COLLATE DATABASE_DEFAULT
            WHERE a.NOKTP = @NIK";

            using var conn = _context.CreateConnection();

            var userDb = await conn.QueryFirstOrDefaultAsync<UserDbDto>(sql, new
            {
                Nik = storedToken.UserId
            });

            if (userDb == null)
                return null;

            // Access token baru harus membawa SessionVersion terbaru dari DB (per modul)
            var session = await _db.UserSession
                .FirstOrDefaultAsync(x => x.Username == userDb.Nama && x.Modul == modul);

            if (session == null)
                return null;

            var newAccessToken = GenerateToken(userDb, modul, session.SessionVersion);
            var newRefreshToken = GenerateRefreshToken(userDb.NoKtp, modul);

            _db.RefreshToken.Add(newRefreshToken);

            await _db.SaveChangesAsync();

            return new LoginResponseDto
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiredAt = DateTime.UtcNow.AddMinutes(10)
            };
        }

        public async Task<bool> Logout(string nik, string ip, string device)
        {
            // ambil semua refresh token user
            var tokens = await _db.RefreshToken
                .Where(x => x.UserId == nik && !x.IsRevoked)
                .ToListAsync();

            if (!tokens.Any())
                return false;

            // revoke refresh token user (mirip AuthController)
            foreach (var t in tokens)
                t.IsRevoked = true;

            // set logout time pada audit login terakhir(yang belum logout)
            var lastAudit = await _db.AuditLogin
                .Where(x => x.UserId == nik && x.IsSuccess && x.LogoutTime == null)
                .OrderByDescending(x => x.LoginTime)
                .FirstOrDefaultAsync();

            if (lastAudit != null)
                lastAudit.LogoutTime = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return true;
        }

        private string GenerateToken(UserDbDto user, string modul, int sessionVersion)
        {
            //var jwtKey = _config["Jwt:Key"];
            //var jwtIssuer = _config["Jwt:Issuer"];
            //var jwtAudience = _config["Jwt:Audience"];

            var jwtKey = _config["JWT:Secret"];
            var jwtIssuer = _config["JWT:ValidIssuer"];
            var jwtAudience = _config["JWT:ValidAudience"];

            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new Exception("JWT Key belum dikonfigurasi di appsettings.json");

            var claims = new[]
            {
                new Claim("noktp", user.NoKtp),
                new Claim("nik", user.NIKSistag),
                new Claim(ClaimTypes.Name, user.Nama),
                new Claim("jabatan", user.Jabatan),
                new Claim("divisi", user.Divisi),
                new Claim("modul", modul),
                new Claim("SessionVersion", sessionVersion.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                //expires: DateTime.UtcNow.AddMinutes(10),
                expires: DateTime.UtcNow.AddMinutes(15), // 🔥 FIX
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken(string userId, string modul)
        {
            return new RefreshToken
            {
                UserId = userId,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                CreatedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddDays(14),
                IsUsed = false,
                IsRevoked = false,
                Modul = modul
            };
        }


        public async Task<ApiResponse<object>> ChangePassword(string username, FormGantiPassword request)
        {
            try
            {
                // ================= VALIDASI INPUT =================
                if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                    string.IsNullOrWhiteSpace(request.NewPassword) ||
                    string.IsNullOrWhiteSpace(request.ConfimrNewPassword))
                {
                    return ApiResponse<object>.Error("Semua field wajib diisi", "400");
                }

                if (request.NewPassword != request.ConfimrNewPassword)
                {
                    return ApiResponse<object>.Error("Konfirmasi password tidak sama", "400");
                }

                if (request.NewPassword.Length < 6)
                {
                    return ApiResponse<object>.Error("Password minimal 6 karakter", "400");
                }

                using var conn = _context.CreateConnection();

                // ================= AMBIL USER =================
                var sql = @"
                SELECT 
                    a.NOKTP,
                    a.NIKSISTAG,
                    b.Password
                FROM HRDTAG.dbo.v_MASTERPEGAWAI a
                LEFT JOIN HRDTAG.dbo.MST_KTP b 
                    ON a.NOKTP COLLATE DATABASE_DEFAULT = b.NOKTP COLLATE DATABASE_DEFAULT
                WHERE a.NOKTP = @Username
                ";

                var user = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, new { Username = username });

                if (user == null)
                    return ApiResponse<object>.Error("User tidak ditemukan", "404");

                // ================= VALIDASI PASSWORD LAMA =================
                var currentPassword = (user.Password ?? user.NIKSISTAG)?.ToString()?.Trim();
                var inputPassword = request.CurrentPassword?.Trim();

                // ❗ hanya blok kalau dua-duanya NULL
                if (string.IsNullOrEmpty(currentPassword))
                {
                    return ApiResponse<object>.Error("User tidak memiliki password / NIK", "400");
                }

                // validasi password lama
                if (!string.Equals(currentPassword, inputPassword))
                {
                    return ApiResponse<object>.Error("Password lama salah", "400");
                }

                // ================= SIMPAN PASSWORD BARU =================
                var sqlUpdate = @"
                IF EXISTS (SELECT 1 FROM HRDTAG.dbo.MST_KTP WHERE NOKTP = @Username)
                BEGIN
                    UPDATE HRDTAG.dbo.MST_KTP
                    SET Password = @NewPassword
                    WHERE NOKTP = @Username
                END
                ELSE
                BEGIN
                    INSERT INTO HRDTAG.dbo.MST_KTP (NOKTP, Password)
                    VALUES (@Username, @NewPassword)
                END
                ";


                await conn.ExecuteAsync(sqlUpdate, new
                {
                    Username = username,
                    NewPassword = request.NewPassword.Trim()
                });

                // ================= FORCE LOGOUT =================
                var sessions = await _db.UserSession
                    .Where(x => x.Username == username)
                    .ToListAsync();

                foreach (var s in sessions)
                {
                    s.SessionVersion += 1;
                }

                await _db.SaveChangesAsync();

                return ApiResponse<object>.Success(null, "Password berhasil diubah");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Error(ex.Message, "500");
            }
        }

        public async Task<ApiResponse<object>> UpdatePhoto(string username, FormUpdatePhoto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.PhotoBase64))
                {
                    return ApiResponse<object>.Error("Foto wajib diisi", "400");
                }

                // ================= CONVERT BASE64 =================
                string base64 = request.PhotoBase64;

                // handle jika ada prefix: data:image/jpeg;base64,...
                if (base64.Contains(","))
                    base64 = base64.Split(',')[1];

                byte[] photoBytes;

                try
                {
                    photoBytes = Convert.FromBase64String(base64);
                }
                catch
                {
                    return ApiResponse<object>.Error("Format base64 tidak valid", "400");
                }

                // ================= VALIDASI UKURAN =================
                if (photoBytes.Length > 2_000_000) // 2MB
                {
                    return ApiResponse<object>.Error("Ukuran foto maksimal 2MB", "400");
                }

                using var conn = _context.CreateConnection();

                // ================= CEK USER =================
                var userExists = await conn.QueryFirstOrDefaultAsync<int>(@"
                    SELECT COUNT(1)
                    FROM HRDTAG.dbo.v_MASTERPEGAWAI
                    WHERE NOKTP = @Username
                ", new { Username = username });

                if (userExists == 0)
                {
                    return ApiResponse<object>.Error("User tidak ditemukan", "404");
                }

                // ================= UPDATE / INSERT FOTO =================
                var sql = @"
                IF EXISTS (SELECT 1 FROM HRDTAG.dbo.MST_KTP WHERE NOKTP = @Username)
                BEGIN
                    UPDATE HRDTAG.dbo.MST_KTP
                    SET FOTO = @Photo
                    WHERE NOKTP = @Username
                END
                ELSE
                BEGIN
                    INSERT INTO HRDTAG.dbo.MST_KTP (NOKTP, FOTO)
                    VALUES (@Username, @Photo)
                END
                ";

                await conn.ExecuteAsync(sql, new
                {
                    Username = username,
                    Photo = photoBytes
                });

                return ApiResponse<object>.Success(null, "Foto berhasil diupdate");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Error(ex.Message, "500");
            }
        }
        private async Task<long?> SaveAuditLogin(
            string? userId,
            string? username,
            string ip,
            string device,
            bool success,
            string? failReason = null,
            string? modul = null,
            string? refreshToken = null,
            string? city = null,
            string? country = null,
            double? lat = null,
            double? lng = null
        )
        {
            var audit = new AuditLogin
            {
                UserId = userId,
                Username = username,
                IpAddress = ip,
                Device = device,
                UserAgent = device,
                City = city,
                Country = country,
                Latitude = lat,
                Longitude = lng,
                LoginTime = DateTime.UtcNow,
                IsSuccess = success,
                FailReason = success ? null : failReason,
                SessionId = Guid.NewGuid().ToString(),
                RefreshToken = refreshToken,
                Modul = modul
            };

            _db.AuditLogin.Add(audit);
            await _db.SaveChangesAsync();

            return audit.Id;
        }


        private string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }
    }
}
