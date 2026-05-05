using System.ComponentModel.DataAnnotations;

namespace tagApiKonfigurasi.Model.Konfigurasi
{
    public class MstModul
    {
        [Key]
        public string IdModul { get; set; } = "";

        public string KodeModul { get; set; } = ""; // HRD, SIMRS
        public string NamaModul { get; set; } = "";

        public string? Icon { get; set; } // 🔥 untuk navlabel (optional)

        public int NoUrut { get; set; }

        public ICollection<MstMenu> Menus { get; set; } = new List<MstMenu>();
    }
}
