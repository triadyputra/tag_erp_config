using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using tagApiKonfigurasi.Model.Konfigurasi;

namespace tagApiKonfigurasi.Model
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(256, ErrorMessage = "Panjang karakter {0} tidak boleh lebih dari {2}")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? NoKtp { get; set; }

        [Required]
        [StringLength(50)]
        public string NikSistag { get; set; } = string.Empty;

        public string? IdModul { get; set; }
        public MstModul? Modul { get; set; }

        public string? Photo { set; get; }
        public bool Active { get; set; }
        public string? Cabang { set; get; }
        public int SessionVersion { get; set; } = 0;
    }
}
