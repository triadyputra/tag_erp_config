using System.ComponentModel.DataAnnotations;

namespace tagApiKonfigurasi.Model.DTO
{
    public class FormLoginDto
    {
        [Required(ErrorMessage = "Username Harus diisi")]
        public string username { get; set; }

        [Required(ErrorMessage = "Password Harus diisi")]
        public string password { get; set; }
    }
}
