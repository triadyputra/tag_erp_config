namespace tagApiKonfigurasi.Model.DTO
{
    public class FormMenuDto
    {
        public string? IdMenu { get; set; }
        public string NamaMenu { get; set; } = "";
        public int NoUrut { get; set; }

        public string IdModul { get; set; } = ""; // 🔥 WAJIB
        public string? Icon { get; set; }
        public string? ParentId { get; set; }

        public List<FormControllerDto> Controllers { get; set; } = new();
    }

    public class FormControllerDto
    {
        public string? IdController { get; set; }
        public string NamaController { get; set; } = "";
        public string Url { get; set; } = ""; // 🔥 WAJIB
        public int NoUrut { get; set; }
        public string? Icon { get; set; }

        public List<FormActionDto> Actions { get; set; } = new();
    }

    public class FormActionDto
    {
        public string IdAction { get; set; } = "";
        public string NamaAction { get; set; } = "";
        public int NoUrut { get; set; }
    }
}
