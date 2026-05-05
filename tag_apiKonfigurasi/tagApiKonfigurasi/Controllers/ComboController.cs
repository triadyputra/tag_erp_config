using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tagApiKonfigurasi.Data;
using tagApiKonfigurasi.Model;




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

        public ComboController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
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


    }
}
