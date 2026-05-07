using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using tagApiKonfigurasi.Data;
using tagApiKonfigurasi.Filter;
using tagApiKonfigurasi.Model;
using tagApiKonfigurasi.Model.DTO;
using tagApiKonfigurasi.Model.DTO.Mobile;
using tagApiKonfigurasi.Services.Mobile;

namespace tagApiKonfigurasi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthMobileController : ControllerBase
    {
        private readonly IRepoMobile _repo;
        private readonly ApplicationDbContext _context;
        public AuthMobileController(IRepoMobile repo, ApplicationDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(ApiResponse<object>.Error("Username dan password wajib diisi", "404"));
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var device = Request.Headers["User-Agent"].ToString();
            var modul = HttpContext.Request.Headers["X-Modul"].FirstOrDefault() ?? "MOBILE";

            var result = await _repo.Login(request, ip, device, modul);

            if (result == null)
            {
                return BadRequest(ApiResponse<object>.Error("Username atau password salah", "404"));
            }

            return Ok(ApiResponse<LoginResponseDto>.Success(result));

        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromHeader(Name = "X-Refresh-Token")] string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest(ApiResponse<object>.Error("Refresh token wajib diisi", "404"));
            }

            var modul = HttpContext.Request.Headers["X-Modul"].FirstOrDefault() ?? "MOBILE";
            var result = await _repo.Refresh(refreshToken, modul);

            if (result == null)
            {
                return Unauthorized(ApiResponse<object>.Error("Refresh token tidak valid", "401"));
            }


            return Ok(ApiResponse<LoginResponseDto>.Success(result));
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var nik = User.FindFirst("noktp")?.Value;

            if (string.IsNullOrWhiteSpace(nik))
            {
                return Unauthorized(ApiResponse<object>.Error("User tidak ditemukan", "401"));
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var device = Request.Headers["User-Agent"].ToString();

            var result = await _repo.Logout(nik, ip, device);

            if (!result)
            {
                return BadRequest(ApiResponse<object>.Error("User tidak ditemukan", "404"));
            }

            return Ok(ApiResponse<object>.SuccessNoData("Logout berhasil"));
        }

        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] FormGantiPassword request)
        {
            var username = User.FindFirst("noktp")?.Value;

            if (string.IsNullOrEmpty(username))
                return Ok(ApiResponse<object>.Error("Unauthorized", "401"));

            var result = await _repo.ChangePassword(username, request);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("UpdatePhoto")]
        public async Task<IActionResult> UpdatePhoto([FromBody] FormUpdatePhoto request)
        {
            var username = User.FindFirst("noktp")?.Value;

            if (string.IsNullOrEmpty(username))
                return Ok(ApiResponse<object>.Error("Unauthorized", "401"));

            var result = await _repo.UpdatePhoto(username, request);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("GetRiwayatLogin")]
        public async Task<ActionResult<ApiResponse<List<ViewAuditLoginDto>>>> GetRiwayatLogin()
        {
            try
            {
                var nik = User.FindFirst("noktp")?.Value;
               

                // ================= RANGE 10 HARI TERAKHIR =================
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-10);

                var items = await _context.AuditLogin
                    .AsNoTracking()
                    .Where(x =>
                        x.UserId != null &&
                        x.UserId.ToLower() == nik.ToLower() &&
                        x.LoginTime >= startDate &&
                        x.LoginTime <= endDate
                    )
                    .OrderByDescending(x => x.LoginTime)
                    .Take(50) // 🔥 LIMIT 50
                    .Select(x => new ViewAuditLoginDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        Username = x.Username,
                        IpAddress = x.IpAddress,
                        Device = x.Device,
                        Modul = x.Modul,
                        UserAgent = x.UserAgent,
                        LoginTime = x.LoginTime.AddHours(7), // WIB
                        LogoutTime = x.LogoutTime.HasValue ? x.LogoutTime.Value.AddHours(7) : null,
                        IsSuccess = x.IsSuccess
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<ViewAuditLoginDto>>.Success(items));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }


    }

}
