using Microsoft.AspNetCore.Identity;
using tagApiKonfigurasi.Model.Konfigurasi;

namespace tagApiKonfigurasi.Model
{
    public class ApplicationRole : IdentityRole
    {
        public string? Access { get; set; }
        public string? Keterangan { get; set; }
        public string? Photo { get; set; }
        public string? IdModul { get; set; }
        public MstModul? Modul { get; set; }
    }
}
