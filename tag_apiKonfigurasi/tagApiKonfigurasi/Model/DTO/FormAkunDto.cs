using System.ComponentModel.DataAnnotations;

namespace tagApiKonfigurasi.Model.DTO
{
    public class FormAkunDto
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "UserName wajib diisi")]
        public string? UserName { get; set; }

        /// <summary>Nama lengkap. Wajib diisi manual jika tanpa No KTP; jika kosong saat simpan, API memakai username.</summary>
        public string? FullName { get; set; }

        public string? NoKtp { get; set; }

        [Required(ErrorMessage = "NIK Sistag wajib diisi")]
        public string? NikSistag { get; set; }

        public string? IdModul { get; set; }

        public string? Email { get; set; }
        public string? Photo { get; set; }
        public bool Active { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Cabang { get; set; }

        [Required(ErrorMessage = "Group harus diisi")]
        public string[]? Group { get; set; } // <- nullable dulu
    }
}
