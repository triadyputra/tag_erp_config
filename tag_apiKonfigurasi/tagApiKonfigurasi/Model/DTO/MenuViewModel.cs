namespace tagApiKonfigurasi.Model.DTO
{
    public class MenuViewModel
    {
        public string IdMenu { get; set; } = "";
        public string NamaMenu { get; set; } = "";
        public int NoUrut { get; set; }

        // 🔥 RELASI MODUL (INI JADI NAVLABEL)
        public string IdModul { get; set; } = "";
        public string NamaModul { get; set; } = "";

        // 🔥 TAMBAHAN
        public string? ParentId { get; set; } // nested menu
        public string? Icon { get; set; }     // icon parent menu

        public List<ControllerViewModel> ControllerViewModel { get; set; } = new();
    }
}
