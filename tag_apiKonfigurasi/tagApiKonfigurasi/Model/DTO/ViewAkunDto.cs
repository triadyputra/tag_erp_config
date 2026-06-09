namespace tagApiKonfigurasi.Model.DTO
{
    public class ViewAkunDto
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? NoKtp { get; set; }
        public string? NikSistag { get; set; }
        public string? IdModul { get; set; }
        public string? NamaModul { get; set; }
        public string? Photo { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Cabang { set; get; }
        public List<string>? Group { get; set; }
        public bool Active { get; set; }
    }
}
