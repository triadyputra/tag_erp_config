using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using tagApiKonfigurasi.Data;
using tagApiKonfigurasi.Filter;
using tagApiKonfigurasi.Helper;
using tagApiKonfigurasi.Model;
using Microsoft.EntityFrameworkCore;
using tagApiKonfigurasi.Model.DTO;
using tagApiKonfigurasi.Services.MasterKtp;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;

namespace tagApiKonfigurasi.Controllers.konfigurasi
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AkunController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IMasterKtpLookupService _masterKtpLookup;

        public AkunController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IMasterKtpLookupService masterKtpLookup)
        {
            _context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
            _masterKtpLookup = masterKtpLookup;
        }

        private async Task<(bool Ok, string? Error, MasterKtpLookupDto? Ktp)> ResolveKtpAsync(string? noKtp)
        {
            if (string.IsNullOrWhiteSpace(noKtp))
                return (false, "No KTP wajib diisi", null);

            var ktp = await _masterKtpLookup.GetByNoKtpAsync(noKtp);
            if (ktp == null)
                return (false, "No KTP tidak ditemukan di Master KTP", null);

            return (true, null, ktp);
        }

        private async Task<string?> CheckDuplicateNoKtpAsync(string noKtp, string? excludeUserId = null)
        {
            var exists = await userManager.Users.AnyAsync(u =>
                u.NoKtp == noKtp &&
                (excludeUserId == null || u.Id != excludeUserId));
            return exists ? "No KTP sudah digunakan akun lain" : null;
        }

        private static (bool Ok, string? Error) ValidateNikSistag(string? nikSistag)
        {
            if (string.IsNullOrWhiteSpace(nikSistag))
                return (false, "NIK Sistag wajib diisi");

            return (true, null);
        }

        private async Task<(bool Ok, string? Error)> ValidateIdModulAsync(string? idModul)
        {
            if (string.IsNullOrWhiteSpace(idModul))
                return (true, null);

            var exists = await _context.Moduls.AnyAsync(m => m.IdModul == idModul);
            if (!exists)
                return (false, "Modul tidak ditemukan");

            return (true, null);
        }

        private async Task<string?> CheckDuplicateNikSistagAsync(string nikSistag, string? excludeUserId = null)
        {
            var exists = await userManager.Users.AnyAsync(u =>
                u.NikSistag == nikSistag &&
                (excludeUserId == null || u.Id != excludeUserId));
            return exists ? "NIK Sistag sudah digunakan akun lain" : null;
        }

        private string? GetRequestModul() =>
            HttpContext.Request.Headers["X-Modul"].FirstOrDefault();

        private static bool ShouldFilterHrdModul(string? idModul, string? requestModul) =>
            string.Equals(idModul, "HRD", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(requestModul, "HRD", StringComparison.OrdinalIgnoreCase);

        private bool IsHrdRequest() => ShouldFilterHrdModul(null, GetRequestModul());

        private static bool HasGroupAssignment(string[]? group) =>
            group != null && group.Length > 0;

        [ApiKeyAuthorize]
        [HttpGet]
        [Route("GetListAkun")]
        public async Task<ActionResult<PaginatedResponse<ViewAkunDto>>> GetListAkun(
        [FromQuery] string? filter = null,
        [FromQuery] string? idModul = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var query = this.userManager.Users
                    .Include(x => x.Modul)
                    .AsQueryable();

                if (ShouldFilterHrdModul(idModul, GetRequestModul()))
                    query = query.Where(x => x.IdModul == "HRD");

                if (!string.IsNullOrEmpty(filter))
                {
                    var f = filter.ToUpper();
                    query = query.Where(x =>
                        (x.FullName != null && x.FullName.ToUpper().Contains(f)) ||
                        (x.NoKtp != null && x.NoKtp.ToUpper().Contains(f)) ||
                        (x.NikSistag != null && x.NikSistag.ToUpper().Contains(f)) ||
                        (x.UserName != null && x.UserName.ToUpper().Contains(f)));
                }

                var count = await query.CountAsync();

                // Ambil user dulu (tanpa role)
                var users = await query
                    .OrderBy(x => x.FullName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var items = new List<ViewAkunDto>();

                foreach (var user in users)
                {
                    // Ambil roles
                    var roles = await userManager.GetRolesAsync(user);

                    items.Add(new ViewAkunDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FullName = user.FullName,
                        NoKtp = user.NoKtp,
                        NikSistag = user.NikSistag,
                        IdModul = user.IdModul,
                        NamaModul = user.Modul?.NamaModul ?? user.IdModul,
                        Photo = user.Photo,
                        PhoneNumber = user.PhoneNumber,
                        Cabang = user.Cabang,
                        Active = user.Active,
                        Group = roles.ToList() ,    // <= TAMBAHKAN PROPERTI INI
                    });
                }

                return Ok(new PaginatedResponse<ViewAkunDto>
                {
                    Data = items,
                    TotalCount = count,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(count / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message.ToString(), "500"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> GetAkun(string id)
        {
            try
            {
                var user = await userManager.FindByIdAsync(id);

                if (user == null)
                {
                    return Ok(ApiResponse<object>.Error("Data not found", "404"));
                }

                var roles = await userManager.GetRolesAsync(user);

                return Ok(ApiResponse<FormAkunDto>.Success(
                    new FormAkunDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        FullName = user.FullName,
                        NoKtp = user.NoKtp,
                        NikSistag = user.NikSistag,
                        IdModul = user.IdModul,
                        Email = user.Email,
                        Photo = user.Photo,
                        Active = user.Active,
                        PhoneNumber = user.PhoneNumber,
                        Cabang = user.Cabang,
                        Group = roles.ToArray(),
                    }));
            }
            catch (Exception ex)
            {
                //return StatusCode(500, ApiResponse<object>.Error(ex.Message, "500"));
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

        [ApiKeyAuthorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> PutAkun(string id, [FromBody] FormAkunDto item)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return Ok(ApiResponse<object>.Error("Validation failed", "400", errors));
                }

                if (string.IsNullOrWhiteSpace(item.UserName))
                {
                    return Ok(ApiResponse<object>.Error("Username tidak boleh kosong", "404"));
                }

                if (!IsHrdRequest() && !HasGroupAssignment(item.Group))
                {
                    return Ok(ApiResponse<object>.Error("Group tidak boleh kosong", "404"));
                }

                var (nikOk, nikError) = ValidateNikSistag(item.NikSistag);
                if (!nikOk)
                    return Ok(ApiResponse<object>.Error(nikError!, "400"));

                var nikSistag = item.NikSistag!.Trim();

                if (IsHrdRequest())
                    item.IdModul = "HRD";

                var modulValidation = await ValidateIdModulAsync(item.IdModul);
                if (!modulValidation.Ok)
                    return Ok(ApiResponse<object>.Error(modulValidation.Error!, "400"));

                if (id != item.Id)
                {
                    return Ok(ApiResponse<object>.Error("ID mismatch", "400"));
                }

                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return Ok(ApiResponse<object>.Error("Data not found", "404"));
                }

                var dupNikError = await CheckDuplicateNikSistagAsync(nikSistag, id);
                if (dupNikError != null)
                    return Ok(ApiResponse<object>.Error(dupNikError, "400"));

                string fullName;
                string? noKtp;

                if (!string.IsNullOrWhiteSpace(item.NoKtp))
                {
                    var (ktpOk, ktpError, ktp) = await ResolveKtpAsync(item.NoKtp);
                    if (!ktpOk)
                        return Ok(ApiResponse<object>.Error(ktpError!, "400"));

                    var dupError = await CheckDuplicateNoKtpAsync(ktp!.NOKTP, id);
                    if (dupError != null)
                        return Ok(ApiResponse<object>.Error(dupError, "400"));

                    noKtp = ktp.NOKTP;
                    fullName = ktp.NAMALENGKAP;
                }
                else
                {
                    noKtp = null;
                    fullName = !string.IsNullOrWhiteSpace(item.FullName)
                        ? item.FullName.Trim()
                        : item.UserName!.Trim();
                }

                // Update properties
                user.UserName = item.UserName;
                user.Email = item.Email;
                user.NoKtp = noKtp;
                user.FullName = fullName;
                user.NikSistag = nikSistag;
                user.IdModul = string.IsNullOrWhiteSpace(item.IdModul) ? null : item.IdModul.Trim();
                user.PhoneNumber = item.PhoneNumber;
                user.Photo = item.Photo;
                user.Active = item.Active;
                user.Cabang = item.Cabang;

                var result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    if (HasGroupAssignment(item.Group))
                    {
                        var existingRoles = await userManager.GetRolesAsync(user);
                        var resultRole = await userManager.RemoveFromRolesAsync(user, existingRoles);
                        if (resultRole.Succeeded)
                        {
                            await userManager.AddToRolesAsync(user, item.Group);
                        }
                    }
                    return Ok(ApiResponse<object>.SuccessNoData());
                }
                else
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return Ok(ApiResponse<object>.Error("Failed to update data", "400", errors.ToList()));
                }

            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

        [ApiKeyAuthorize]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> PostAkun([FromBody] FormAkunDto item)
        {
            try
            {
                var (nikOk, nikError) = ValidateNikSistag(item.NikSistag);
                if (!nikOk)
                    return Ok(ApiResponse<object>.Error(nikError!, "400"));

                var nikSistag = item.NikSistag!.Trim();

                if (IsHrdRequest())
                    item.IdModul = "HRD";

                if (!IsHrdRequest() && !HasGroupAssignment(item.Group))
                {
                    return Ok(ApiResponse<object>.Error("Group tidak boleh kosong", "404"));
                }

                var modulValidation = await ValidateIdModulAsync(item.IdModul);
                if (!modulValidation.Ok)
                    return Ok(ApiResponse<object>.Error(modulValidation.Error!, "400"));

                var dupNikError = await CheckDuplicateNikSistagAsync(nikSistag);
                if (dupNikError != null)
                    return Ok(ApiResponse<object>.Error(dupNikError, "400"));

                var (ktpOk, ktpError, ktp) = await ResolveKtpAsync(item.NoKtp);
                if (!ktpOk)
                    return Ok(ApiResponse<object>.Error(ktpError!, "400"));

                var dupError = await CheckDuplicateNoKtpAsync(ktp!.NOKTP);
                if (dupError != null)
                    return Ok(ApiResponse<object>.Error(dupError, "400"));

                var user = new ApplicationUser
                {
                    UserName = item.UserName,
                    Email = item.Email,
                    NoKtp = ktp.NOKTP,
                    FullName = ktp.NAMALENGKAP,
                    NikSistag = nikSistag,
                    IdModul = string.IsNullOrWhiteSpace(item.IdModul) ? null : item.IdModul.Trim(),
                    PhoneNumber = item.PhoneNumber,
                    Photo = item.Photo,
                    Active = item.Active,
                    EmailConfirmed = true,
                    Cabang = item.Cabang,
                    //Peran = akun.Peran,
                };
                var result = await userManager.CreateAsync(user, "123456");

                if (result.Succeeded)
                {
                    if (HasGroupAssignment(item.Group))
                    {
                        await userManager.AddToRolesAsync(user, item.Group);
                    }
                    return Ok(ApiResponse<object>.SuccessNoData());
                }
                else
                {
                    var errors = result.Errors.Select(e => e.Description);

                    return Ok(ApiResponse<object>.Error("Failed to create data", "400", errors.ToList()));
                }

            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

        [ApiKeyAuthorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteAkun(string id)
        {
            try
            {
                var item = await userManager.FindByIdAsync(id);
                if (item == null)
                {
                    return Ok(ApiResponse<object>.Error("Data not found", "404"));
                }

                var result = await userManager.DeleteAsync(item);
                if (!result.Succeeded)
                {
                    return Ok(ApiResponse<object>.Error(
                        "Failed to delete data",
                        "400",
                        result.Errors.Select(e => e.Description).ToList()
                    ));
                }

                return Ok(ApiResponse<object>.SuccessNoData("Data deleted successfully", "200"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

    }
}
