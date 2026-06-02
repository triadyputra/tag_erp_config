using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tagApiKonfigurasi.Data;
using tagApiKonfigurasi.Model;
using tagApiKonfigurasi.Model.DTO;
using tagApiKonfigurasi.Services.MasterKtp;




namespace tagApiKonfigurasi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ComboController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IMasterKtpLookupService _masterKtpLookup;

        public ComboController(
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

        [HttpGet]
        [Route("ComboGroup")]
        public async Task<IActionResult> ComboGroup()
        {
            var Group = await roleManager.Roles.ToListAsync();
            var listGroup = Group.Select(p => new { value = p.Name, title = p.Name });

            return Ok(listGroup);
        }

        [HttpGet]
        [Route("ComboCabangTag")]
        public async Task<IActionResult> ComboCabangTag()
        {
            var opt = await _context.TblCabang.Where(x => x.Status && x.KdGroup=="TAG").ToListAsync();
            var listOpt = opt.Select(p => new { value = p.KdCabang, title = p.NmCabang });
            return Ok(listOpt);
        }

        //[HttpGet]
        //[Route("ComboCabang")]
        //public async Task<IActionResult> ComboCabang()
        //{
        //    var cabang = await _comboRepository.ComboCabang();

        //    return Ok(cabang);
        //}

        //[HttpGet]
        //[Route("ComboCabangWithPusat")]
        //public async Task<IActionResult> ComboCabangWithPusat()
        //{
        //    var cabang = await _comboRepository.ComboCabangWithPusat();

        //    return Ok(cabang);
        //}

        [HttpGet("GetFilterMasterKtp")]
        public async Task<IActionResult> GetFilterMasterKtp(
            [FromQuery] string? nama,
            [FromQuery] string? cabang)
        {
            try
            {
                var data = await _masterKtpLookup.SearchAsync(nama, cabang);
                return Ok(ApiResponse<List<MasterKtpLookupDto>>.Success(data));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

        [HttpGet("GetDetailMasterKtp")]
        public async Task<IActionResult> GetDetailMasterKtp([FromQuery] string noktp)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(noktp))
                    return Ok(ApiResponse<object>.Error("No KTP wajib diisi", "400"));

                var data = await _masterKtpLookup.GetByNoKtpAsync(noktp);
                if (data == null)
                    return Ok(ApiResponse<object>.Error("Data KTP tidak ditemukan", "404"));

                return Ok(ApiResponse<MasterKtpLookupDto>.Success(data));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }
    }
}
