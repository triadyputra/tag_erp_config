using tagApiKonfigurasi.Model.DTO;

namespace tagApiKonfigurasi.Services.MasterKtp
{
    public interface IMasterKtpLookupService
    {
        Task<List<MasterKtpLookupDto>> SearchAsync(string? nama, string? cabang);
        Task<MasterKtpLookupDto?> GetByNoKtpAsync(string noktp);

        //Task<KaryawanProfile?> GetDetailKaryawan(string noktp);
    }
}
