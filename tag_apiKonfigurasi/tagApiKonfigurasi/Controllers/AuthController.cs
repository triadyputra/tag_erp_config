using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using tagApiKonfigurasi.Data;
using tagApiKonfigurasi.Helper;
using tagApiKonfigurasi.Model;
using tagApiKonfigurasi.Model.DTO;
using tagApiKonfigurasi.Model.Konfigurasi;
using tagApiKonfigurasi.Services.EmployeeLogin;
using tagApiKonfigurasi.Services.Menu;

namespace tagApiKonfigurasi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMenuService _menuService;
        private readonly IEmployeeLoginEligibilityService _loginEligibility;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ApplicationDbContext context,
            IMenuService menuService,
            IEmployeeLoginEligibilityService loginEligibility)
        {
            this.userManager = userManager;
            _configuration = configuration;
            _context = context;
            _menuService = menuService;
            _loginEligibility = loginEligibility;
        }

        // =====================================================
        // LOGIN (Single Session + Audit IP/Device)
        // =====================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] FormLoginDto model)
        {

            var ip = GetIpAddress();
            var device = GetDeviceInfo();

            var modul = HttpContext.Request.Headers["X-Modul"].FirstOrDefault() ?? "UNKNOWN";

            // 🔥 ambil lokasi dari IP
            var (city, country) = await GetGeoFromIp(ip);

            var user = await userManager.FindByNameAsync(model.username);

            if (user == null)
            {
                await SaveAuditLogin(
                     model.username,
                     model.username,
                     ip,
                     device,
                     false,
                     "Username tidak ditemukan",
                     city,
                     country,
                     modul
                 );

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    metadata = new { code = "201", message = "Username tidak ditemukan" },
                    response = ""
                });
            }


            if (!user.Active)
            {
                await SaveAuditLogin(
                    user.Id,
                    user.UserName,
                    ip,
                    device,
                    false,
                    "User tidak aktif",
                    city,
                    country,
                    modul
                );

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    metadata = new { code = "201", message = "User tidak aktif" },
                    response = ""
                });
            }

            if (!await userManager.CheckPasswordAsync(user, model.password))
            {
                await SaveAuditLogin(
                     user.Id,
                     user.UserName,
                     ip,
                     device,
                     false,
                     "Password salah",
                     city,
                     country,
                     modul
                 );

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    metadata = new { code = "201", message = "User password tidak sesuai" },
                    response = ""
                });
            }

            var eligibility = await _loginEligibility.ValidateAsync(
                user.NoKtp,
                LoginEligibilityMode.WebStrict);

            if (!eligibility.IsEligible)
            {
                await SaveAuditLogin(
                    user.Id,
                    user.UserName,
                    ip,
                    device,
                    false,
                    eligibility.Message,
                    city,
                    country,
                    modul);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    metadata = new { code = "201", message = eligibility.Message },
                    response = ""
                });
            }

            // 🔥 Single session: naikkan session version (biar device lama langsung 401)
            //user.SessionVersion += 1;
            //await userManager.UpdateAsync(user);
            var session = await _context.UserSession
            .FirstOrDefaultAsync(x => x.Username == user.UserName && x.Modul == modul);

            if (session == null)
            {
                session = new UserSession
                {
                    Username = user.UserName,
                    Modul = modul,
                    SessionVersion = 1
                };

                _context.UserSession.Add(session);
            }
            else
            {
                session.SessionVersion += 1;
            }

            await _context.SaveChangesAsync();

            // 🔥 Revoke semua refresh token lama
            var oldTokens = _context.RefreshToken
                .Where(x => x.UserId == user.Id && !x.IsRevoked);

            foreach (var oldToken in oldTokens)
                oldToken.IsRevoked = true;

            // ✅ Audit login sukses
            var auditId = await SaveAuditLogin(
                 user.Id,
                 user.UserName,
                 ip,
                 device,
                 true,
                 null,
                 city,
                 country,
                 modul
             );

            // 🔥 SAVE SEKALI SAJA
            await _context.SaveChangesAsync();


            // Build access token (WAJIB include SessionVersion)
            //var accessToken = BuildAccessToken(user.UserName, user.SessionVersion, modul);
            var accessToken = BuildAccessToken(user.UserName, session.SessionVersion, modul);

            // Create refresh token baru (kalau mau, simpan auditId juga di refresh token)
            var refreshToken = GenerateRefreshToken(user.Id, modul);

            _context.RefreshToken.Add(refreshToken);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                metadata = new { code = "200", message = "ok" },
                response = new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(accessToken),
                    refreshToken = refreshToken.Token,
                    expiresIn = 600,
                    auditLoginId = auditId // optional buat tracking
                }
            });
        }

        // =====================================================
        // REFRESH (WAJIB include SessionVersion)
        // =====================================================
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromHeader(Name = "X-Refresh-Token")] string refreshToken)
        {
            var modul = HttpContext.Request.Headers["X-Modul"].FirstOrDefault() ?? "UNKNOWN";

            var storedToken = await _context.RefreshToken
                .FirstOrDefaultAsync(x => x.Token == refreshToken && x.Modul == modul);

            if (storedToken == null ||
                storedToken.ExpiredAt < DateTime.UtcNow ||
                storedToken.IsUsed ||
                storedToken.IsRevoked)
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }

            storedToken.IsUsed = true;

            var user = await userManager.FindByIdAsync(storedToken.UserId);
            if (user == null || !user.Active)
                return Unauthorized(new { message = "User tidak valid / tidak aktif" });

            // ✅ Access token baru harus bawa SessionVersion terbaru dari DB
            //var newAccessToken = BuildAccessToken(user.UserName, user.SessionVersion, modul);
            var session = await _context.UserSession.FirstOrDefaultAsync(x => x.Username == user.UserName && x.Modul == modul);
            if (session == null)
                return Unauthorized(new { message = "Session tidak ditemukan" });
            var newAccessToken = BuildAccessToken(user.UserName, session.SessionVersion, modul);

            // Refresh token baru
            var newRefreshToken = GenerateRefreshToken(user.Id, modul);
            _context.RefreshToken.Add(newRefreshToken);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                metadata = new { code = "200", message = "ok" },
                response = new
                {
                    accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                    refreshToken = newRefreshToken.Token,
                    expiresIn = 600
                }
            });
        }

        // =====================================================
        // LOGOUT (revoke token + set logout time audit terakhir)
        // =====================================================
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized();

            var user = await userManager.FindByNameAsync(username);
            if (user == null) return Unauthorized();

            // revoke refresh token user
            var tokens = _context.RefreshToken
                .Where(x => x.UserId == user.Id && !x.IsRevoked);

            foreach (var t in tokens)
                t.IsRevoked = true;

            // set logout time pada audit login terakhir (yang belum logout)
            var lastAudit = await _context.AuditLogin
                .Where(x => x.UserId == user.Id && x.IsSuccess && x.LogoutTime == null)
                .OrderByDescending(x => x.LoginTime)
                .FirstOrDefaultAsync();

            if (lastAudit != null)
                lastAudit.LogoutTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                metadata = new { code = "200", message = "Logged out" }
            });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            var modul = HttpContext.Request.Headers["X-Modul"].FirstOrDefault() ?? "UNKNOWN";

            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            var user = await userManager.FindByNameAsync(username);
            var userRoles = await userManager.GetRolesAsync(user);

            // =====================================================
            // 🔥 ROLE + ACCESS
            // =====================================================
            var roles = await (
                from usr in _context.Users
                join userRole in _context.UserRoles.AsNoTracking() on usr.Id equals userRole.UserId
                join role in _context.Roles on userRole.RoleId equals role.Id
                where usr.UserName == username
                select role
            ).ToListAsync();

            var accesList = new List<AccesModel>();

            foreach (var role in roles)
            {
                if (!string.IsNullOrEmpty(role.Access))
                {
                    var access = JsonConvert.DeserializeObject<IEnumerable<AccesModel>>(role.Access);
                    accesList.AddRange(access);
                }
            }

            var distinctAccess = accesList
                .GroupBy(x => new { x.IdController, x.IdAction })
                .Select(g => g.First())
                .ToList();

            // =====================================================
            // 🔥 AMBIL DATA MENU DARI DB
            // =====================================================
            var menuData = await _menuService.GetMenuAsync(modul);

            var allControllers = menuData
                .SelectMany(m => m.ControllerViewModel ?? new List<ControllerViewModel>())
                .ToList();

            // =====================================================
            // 🔥 FILTER CONTROLLER BERDASARKAN ACCESS
            // =====================================================
            var allowedControllers = allControllers
                .Where(ctrl =>
                    ctrl.ActionViewModel.Any(action =>
                        distinctAccess.Any(a =>
                            a.IdController == ctrl.IdController &&
                            a.IdAction == action.IdAction)))
                .Select(c => c.IdController)
                .ToHashSet();

            // =====================================================
            // 🔥 BUILD MENU TREE (DARI DB)
            // =====================================================
            List<object> BuildDynamicMenu(
                List<MenuViewModel> menus,
                HashSet<string> allowedControllers
            )
            {
                var result = new List<object>();

                var parentMenus = menus
                    .Where(m => string.IsNullOrEmpty(m.ParentId))
                    .OrderBy(m => m.NoUrut)
                    .ToList();

                foreach (var parent in parentMenus)
                {
                    var childrenMenu = menus
                        .Where(m => m.ParentId == parent.IdMenu)
                        .OrderBy(m => m.NoUrut)
                        .ToList();

                    var sectionItems = new List<object>();

                    // =========================================
                    // 🔥 CONTROLLER LANGSUNG DI PARENT
                    // =========================================
                    foreach (var ctrl in parent.ControllerViewModel ?? new())
                    {
                        if (!allowedControllers.Contains(ctrl.IdController))
                            continue;

                        sectionItems.Add(new
                        {
                            id = ctrl.IdController,
                            title = ctrl.Controller,
                            icon = ctrl.Icon ?? "IconPoint",
                            href = ctrl.Url
                        });
                    }

                    // =========================================
                    // 🔥 CHILD MENU (NESTED)
                    // =========================================
                    foreach (var child in childrenMenu)
                    {
                        var childControllers = child.ControllerViewModel ?? new();

                        var validChildren = childControllers
                            .Where(c => allowedControllers.Contains(c.IdController))
                            .Select(c => new
                            {
                                id = c.IdController,
                                title = c.Controller,
                                icon = c.Icon ?? "IconPoint",
                                href = c.Url
                            })
                            .ToList();

                        if (!validChildren.Any())
                            continue;

                        sectionItems.Add(new
                        {
                            id = child.IdMenu,
                            title = child.NamaMenu,
                            icon = child.Icon ?? "IconFolder",
                            href = "#",
                            children = validChildren
                        });
                    }

                    // =========================================
                    // 🔥 JIKA ADA ISI → TAMBAH NAVLABEL
                    // =========================================
                    if (sectionItems.Any())
                    {
                        result.Add(new
                        {
                            navlabel = true,
                            subheader = parent.NamaMenu
                        });

                        result.AddRange(sectionItems);
                    }
                }

                return result;
            }

            var dynamicMenu = BuildDynamicMenu(menuData, allowedControllers);

            // =====================================================
            // 🔥 FORMAT ACCESS UNTUK FRONTEND
            // =====================================================
            var sendAccess = allControllers
                .SelectMany(ctrl => ctrl.ActionViewModel
                    .Where(a => distinctAccess.Any(x =>
                        x.IdController == ctrl.IdController &&
                        x.IdAction == a.IdAction))
                    .Select(a => new FilterMenuWeb
                    {
                        action = a.IdAction,
                        subject = ctrl.IdController
                    }))
                .ToList();

            // =====================================================
            // 🔥 RESPONSE FINAL
            // =====================================================
            return Ok(new
            {
                metadata = new { code = "200", message = "ok" },
                response = new
                {
                    user = new
                    {
                        fullName = user.FullName,
                        username = user.UserName,
                        avatar = user.Photo,
                        cabang = user.Cabang,
                        group = userRoles,
                        role = userRoles.FirstOrDefault()
                    },
                    acces = sendAccess,
                    Menu = dynamicMenu // 🔥 FULL DYNAMIC
                }
            });
        }

        //[HttpGet("me")]
        //[Authorize]
        //public async Task<IActionResult> GetUserProfile()
        //{
        //    var username = User.FindFirst(ClaimTypes.Name)?.Value;

        //    var user = await userManager.FindByNameAsync(username);
        //    var userRoles = await userManager.GetRolesAsync(user);

        //    // Ambil roles beserta akses
        //    var roles = await (
        //        from usr in _context.Users
        //        join userRole in _context.UserRoles.AsNoTracking() on usr.Id equals userRole.UserId
        //        join role in _context.Roles on userRole.RoleId equals role.Id
        //        where usr.UserName == username
        //        select role
        //    ).ToListAsync();

        //    var _acces = new List<AccesModel>();

        //    foreach (var role in roles)
        //    {
        //        if (!string.IsNullOrEmpty(role.Access))
        //        {
        //            var accessList = JsonConvert.DeserializeObject<IEnumerable<AccesModel>>(role.Access);
        //            _acces.AddRange(accessList.Select(a => new AccesModel
        //            {
        //                IdController = a.IdController,
        //                IdAction = a.IdAction
        //            }));
        //        }
        //    }

        //    var distinctList = _acces.GroupBy(s => s.IdAction).Select(s => s.First()).ToList();

        //    //ModulClass xData = new ModulClass();

        //    /* new */
        //    //var allControllers = xData.GetListMenu()
        //    //    .SelectMany(m => m.ControllerViewModel ?? new List<ControllerViewModel>())
        //    //    .ToList();

        //    var menuData = await _menuService.GetMenuAsync();

        //    var allControllers = menuData
        //        .SelectMany(m => m.ControllerViewModel ?? new List<ControllerViewModel>())
        //        .ToList();

        //    var userAccessibleControllers = allControllers
        //        .Select(controller => new ControllerViewModel
        //        {
        //            IdController = controller.IdController,
        //            NoUrut = controller.NoUrut,
        //            Controller = controller.Controller,
        //            IdMenu = controller.IdMenu,
        //            ActionViewModel = (controller.ActionViewModel ?? new List<ActionViewModel>())
        //                .Where(action =>
        //                    distinctList.Any(a =>
        //                        a.IdController == controller.IdController &&
        //                        a.IdAction == action.IdAction)
        //                )
        //                .ToList()
        //        })
        //        .Where(ctrl => ctrl.ActionViewModel.Any())
        //        .ToList();

        //    var sendRole = userAccessibleControllers
        //    .SelectMany(ctrl => ctrl.ActionViewModel
        //        .Select(a => new FilterMenuWeb
        //        {
        //            action = a.IdAction,
        //            subject = a.IdController
        //        }))
        //    .ToList();

        //    // =====================================================
        //    // CONTROLLER YANG BOLEH MUNCUL DI MENU (HANYA "LIHAT")
        //    // =====================================================
        //    var allowedControllers = userAccessibleControllers
        //        .Where(c => c.ActionViewModel.Any(a =>
        //            a.NamaAction.Equals("Lihat", StringComparison.OrdinalIgnoreCase)))
        //        .Select(c => c.IdController)
        //        .ToHashSet();

        //    // =====================================================
        //    // MAPPING MENU → CONTROLLER
        //    // =====================================================
        //    var menuControllerMap = new Dictionary<string, string>
        //    {
        //        ["/konfigurasi/akun"] = "Akun",
        //        ["/konfigurasi/group"] = "Role",
        //        ["/konfigurasi/menu"] = "MenuManagement",
        //        ["/konfigurasi/audit-login"] = "AuditLogin",

        //        ["/master-data/cabang"] = "MstCabang",
        //    };


        //    // Buat sendMenu ala front-end
        //    var sendMenu = new List<object>
        //    {
        //        // ======================
        //        // HOME
        //        // ======================
        //        new { navlabel = true, subheader = "Home" },
        //        new {
        //            id = Guid.NewGuid(),
        //            title = "Dashboard",
        //            icon = "IconChartHistogram",
        //            href = "/"
        //        },

        //        // ======================
        //        // KONFIGURASI
        //        // ======================
        //        new { navlabel = true, subheader = "Konfigurasi" },
        //        new {
        //            id = Guid.NewGuid(),
        //            title = "User Akun",
        //            icon = "IconUsers",
        //            href = "/konfigurasi/akun"
        //        },
        //        new {
        //            id = Guid.NewGuid(),
        //            title = "Group Akun",
        //            icon = "IconLockAccess",
        //            href = "/konfigurasi/group"
        //        },
        //        new {
        //            id = Guid.NewGuid(),
        //            title = "Manajemen Menu",
        //            icon = "IconHierarchy",
        //            href = "/konfigurasi/menu"
        //        },
        //        new {
        //            id = Guid.NewGuid(),
        //            title = "Log Login",
        //            icon = "IconHistory",
        //            href = "/konfigurasi/audit-login"
        //        },


        //        // ======================
        //        // MASTER DATA
        //        // ======================
        //        new { navlabel = true, subheader = "Master Data" },
        //        new {
        //            id = Guid.NewGuid(),
        //            title = "Master Cabang",
        //            icon = "IconBuilding",
        //            href = "/master-data/cabang"
        //        },


        //    };

        //    // =====================================================
        //    // FILTER MENU BERDASARKAN HAK LIHAT
        //    // =====================================================
        //    List<object> FilterMenu(List<object> menus)
        //    {
        //        var result = new List<object>();
        //        object? pendingNavLabel = null;

        //        foreach (var item in menus)
        //        {
        //            // =====================================
        //            // NAVLABEL → TAHAN DULU
        //            // =====================================
        //            if (item.GetType().GetProperty("navlabel") != null)
        //            {
        //                pendingNavLabel = item;
        //                continue;
        //            }

        //            var hrefProp = item.GetType().GetProperty("href");
        //            var childrenProp = item.GetType().GetProperty("children");

        //            var href = hrefProp?.GetValue(item)?.ToString();

        //            // =====================================
        //            // DASHBOARD (GLOBAL)
        //            // =====================================
        //            if (href == "/")
        //            {
        //                if (pendingNavLabel != null)
        //                {
        //                    result.Add(pendingNavLabel);
        //                    pendingNavLabel = null;
        //                }

        //                result.Add(item);
        //                continue;
        //            }

        //            // =====================================
        //            // MENU DENGAN CHILD (PARENT)
        //            // =====================================
        //            if (childrenProp != null)
        //            {
        //                var children = childrenProp.GetValue(item) as IEnumerable<object>;
        //                if (children == null) continue;

        //                var validChildren = new List<object>();

        //                foreach (var child in children)
        //                {
        //                    var childHref = child.GetType()
        //                        .GetProperty("href")
        //                        ?.GetValue(child)
        //                        ?.ToString();

        //                    if (string.IsNullOrWhiteSpace(childHref) || childHref == "#")
        //                        continue;

        //                    if (!menuControllerMap.TryGetValue(childHref, out var controller))
        //                        continue;

        //                    if (!allowedControllers.Contains(controller))
        //                        continue;

        //                    // ✅ child valid
        //                    validChildren.Add(child);
        //                }

        //                // ❌ TIDAK ADA CHILD VALID → SKIP PARENT
        //                if (!validChildren.Any())
        //                    continue;

        //                // ✅ ADA CHILD VALID → TAMBAHKAN
        //                if (pendingNavLabel != null)
        //                {
        //                    result.Add(pendingNavLabel);
        //                    pendingNavLabel = null;
        //                }

        //                // rebuild parent dengan child hasil filter
        //                result.Add(new
        //                {
        //                    id = item.GetType().GetProperty("id")?.GetValue(item),
        //                    title = item.GetType().GetProperty("title")?.GetValue(item),
        //                    icon = item.GetType().GetProperty("icon")?.GetValue(item),
        //                    href = "#",
        //                    children = validChildren
        //                });

        //                continue;
        //            }

        //            // =====================================
        //            // MENU BIASA
        //            // =====================================
        //            if (!string.IsNullOrEmpty(href) &&
        //                menuControllerMap.TryGetValue(href, out var ctrl) &&
        //                allowedControllers.Contains(ctrl))
        //            {
        //                if (pendingNavLabel != null)
        //                {
        //                    result.Add(pendingNavLabel);
        //                    pendingNavLabel = null;
        //                }

        //                result.Add(item);
        //            }
        //        }

        //        return result;
        //    }

        //    var filteredMenu = FilterMenu(sendMenu);

        //    return Ok(new
        //    {
        //        metadata = new { code = "200", message = "ok" },
        //        response = new
        //        {
        //            user = new
        //            {
        //                fullName = user.FullName,
        //                username = user.UserName,
        //                avatar = user.Photo,
        //                cabang = user.Cabang,
        //                group = userRoles,
        //                role = userRoles.FirstOrDefault()
        //            },
        //            acces = sendRole,
        //            Menu = filteredMenu,
        //            XMenu = sendMenu
        //        }
        //    });
        //}


        [HttpGet]
        [Route("validate-session")]
        [Authorize]
        public async Task<IActionResult> ValidateSession()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var tokenVersion = User.FindFirst("SessionVersion")?.Value;
            var modul = User.FindFirst("modul")?.Value;

            if (string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(tokenVersion) ||
                string.IsNullOrEmpty(modul))
                return Unauthorized();

            var session = await _context.UserSession
                .FirstOrDefaultAsync(x => x.Username == username && x.Modul == modul);

            if (session == null || session.SessionVersion.ToString() != tokenVersion)
                return Unauthorized();

            return Ok();
        }


        [HttpPost]
        [Route("Validate")]
        public async Task<IActionResult> Validate([FromBody] AccessValidationRequest request)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(request.Token.Replace("Bearer ", ""));
                var claims = token.Claims.ToList();

                var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(username))
                    return Unauthorized(new { message = "Token tidak memiliki username (ClaimTypes.Name)" });

                var roles = await (
                    from user in _context.Users
                    join userRole in _context.UserRoles on user.Id equals userRole.UserId
                    join role in _context.Roles on userRole.RoleId equals role.Id
                    where user.UserName == username
                    select role
                ).ToListAsync();

                var accessList = new List<AccesModel>();

                foreach (var role in roles)
                {
                    if (!string.IsNullOrEmpty(role.Access))
                    {
                        var parsedAccess = JsonConvert.DeserializeObject<IEnumerable<AccesModel>>(role.Access);
                        accessList.AddRange(parsedAccess);
                    }
                }

                var hasAccess = accessList.Any(x =>
                    string.Equals(x.IdController, request.Controller, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.IdAction, request.Action, StringComparison.OrdinalIgnoreCase));

                if (!hasAccess)
                    return Forbid();

                return Ok(new { valid = true });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }



        [HttpGet("GetCabang")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> GetCabang(
            [FromQuery] string? cabang
        )
        {
            try
            {
                var finalCabang = await User.GetCabangAsync(userManager, cabang);

                return Ok(ApiResponse<object>.Success(finalCabang));

            }
            catch (Exception ex)
            {

                return Ok(ApiResponse<object>.Error(ex.InnerException?.Message ?? "", "500"));
            }
        }

        // =====================================================
        // Helpers
        // =====================================================
        private JwtSecurityToken BuildAccessToken(string username, int sessionVersion, string modul)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim("SessionVersion", sessionVersion.ToString()),
                new Claim("modul", modul), // 🔥 TAMBAHAN
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])
            );

            return new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.UtcNow.AddMinutes(10),
                claims: claims,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
        }

        private RefreshToken GenerateRefreshToken(string userId, string modul)
        {
            return new RefreshToken
            {
                UserId = userId,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiredAt = DateTime.UtcNow.AddDays(14),
                CreatedAt = DateTime.UtcNow,
                IsUsed = false,
                IsRevoked = false,
                Modul = modul
            };
        }

        private async Task<long?> SaveAuditLogin(
            string? userId,
            string? username,
            string ip,
            string device,
            bool success,
            string? failReason = null,
            string? city = null,
            string? country = null,
            string? modul = null,
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
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),

                // 📍 LOKASI DI SINI
                City = city,
                Country = country,
                Latitude = lat,
                Longitude = lng,

                LoginTime = DateTime.UtcNow,
                IsSuccess = success,

                FailReason = success ? null : failReason,
                SessionId = Guid.NewGuid().ToString(),

                Modul = modul
            };

            _context.AuditLogin.Add(audit);

            return audit.Id;
        }

        //private string GetIpAddress()
        //{
        //    // kalau ada proxy/IIS reverse proxy, X-Forwarded-For bisa berisi "clientIp, proxyIp"
        //    var xff = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        //    if (!string.IsNullOrWhiteSpace(xff))
        //        return xff.Split(',')[0].Trim();

        //    return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "-";
        //}

        private string GetIpAddress()
        {
            var ip = HttpContext.Connection.RemoteIpAddress;

            if (ip == null)
                return "-";

            if (ip.IsIPv4MappedToIPv6)
                ip = ip.MapToIPv4();

            if (!IPAddress.TryParse(ip.ToString(), out var parsedIp))
                return "-";

            return parsedIp.ToString();
        }

        private string GetDeviceInfo()
        {
            return HttpContext.Request.Headers["User-Agent"].ToString();
        }

        private async Task<(string city, string country)> GetGeoFromIp(string ip)
        {
            try
            {
                using var client = new HttpClient();
                var res = await client.GetStringAsync($"http://ip-api.com/json/{ip}");
                var geo = JsonConvert.DeserializeObject<dynamic>(res);

                return (geo.city?.ToString(), geo.country?.ToString());
            }
            catch
            {
                return ("Unknown", "Unknown");
            }
        }

    }
}
