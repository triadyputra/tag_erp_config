namespace tagApiKonfigurasi.Model.DTO
{
    public class ViewRoleDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Access { get; set; }
        public string Keterangan { get; set; }
        public string Photo { get; set; }
        public string? IdModul { get; set; }
        public string? NamaModul { get; set; }
        //public ICollection<MenuViewModel> AccesDefault { get; set; } = new List<MenuViewModel>();
    }
}
