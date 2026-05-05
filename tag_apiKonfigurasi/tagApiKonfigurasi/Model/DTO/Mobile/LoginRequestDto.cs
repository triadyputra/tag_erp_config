namespace tagApiKonfigurasi.Model.DTO.Mobile
{
    public class LoginRequestDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponseDto
    {
        public required string Token { get; set; }
        public DateTime ExpiredAt { get; set; }
        public string RefreshToken { get; set; }
        public UserInfoDto User { get; set; }
    }

    public class UserInfoDto
    {
        public required string NoKtp { get; set; }
        public required string NIKSistag { get; set; }
        public required string Nama { get; set; }
        public required string Jabatan { get; set; }
        public required string Divisi { get; set; }
        public DateTime TMT { get; set; }
        public required string Cabang { get; set; }
        public string? Photo { get; set; } // <-- ubah dari byte[] ke string

    }

    public class UserDbDto
    {
        public string NoKtp { get; set; }
        public string NIKSistag { get; set; }
        public string Nama { get; set; }
        public string Jabatan { get; set; }
        public string Divisi { get; set; }
        public DateTime TMT { get; set; }
        public string Cabang { get; set; }

        public byte[]? Photo { get; set; }
    }
}
