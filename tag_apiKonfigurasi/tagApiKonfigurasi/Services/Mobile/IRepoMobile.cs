using tagApiKonfigurasi.Model;
using tagApiKonfigurasi.Model.DTO;
using tagApiKonfigurasi.Model.DTO.Mobile;

namespace tagApiKonfigurasi.Services.Mobile
{
    public interface IRepoMobile
    {
        Task<MobileLoginResult> Login(LoginRequestDto request, string ip, string device, string modul);
        Task<LoginResponseDto?> Refresh(string refreshToken, string modul);
        Task<bool> Logout(string refreshToken, string ip, string device);

        Task<ApiResponse<object>> ChangePassword(string username, FormGantiPassword request);
        Task<ApiResponse<object>> UpdatePhoto(string username, FormUpdatePhoto request);
    }
}
