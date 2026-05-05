using tagApiKonfigurasi.Model.DTO.Mobile;

namespace tagApiKonfigurasi.Services.Mobile
{
    public interface IRepoMobile
    {
        Task<LoginResponseDto?> Login(LoginRequestDto request, string ip, string device, string modul);
        Task<LoginResponseDto?> Refresh(string refreshToken, string modul);
        Task<bool> Logout(string refreshToken, string ip, string device);
    }
}
