using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using tagApiKonfigurasi.Model;
using tagApiKonfigurasi.Model.DTO.Mobile;
using tagApiKonfigurasi.Services.Mobile;

namespace tagApiKonfigurasi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthMobileController : ControllerBase
    {
        private readonly IRepoMobile _repo;

        public AuthMobileController(IRepoMobile repo)
        {
            _repo = repo;
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
    }
}
