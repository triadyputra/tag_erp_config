namespace tagApiKonfigurasi.Model.DTO
{
    public class ControllerViewModel
    {
        public string IdController { get; set; } = "";
        public string Controller { get; set; } = "";
        public string IdMenu { get; set; } = "";
        public int NoUrut { get; set; }

        // 🔥 WAJIB
        public string Url { get; set; } = "";

        // 🔥 TAMBAHAN (biar bisa seperti contoh kamu)
        public string? Icon { get; set; } = "IconPoint";

        // 🔥 OPTIONAL (kalau mau nested controller, jarang dipakai tapi powerful)
        public string? ParentControllerId { get; set; }

        public List<ActionViewModel> ActionViewModel { get; set; } = new();
    }
}
