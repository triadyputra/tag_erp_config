using DocumentFormat.OpenXml.Bibliography;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tagApiKonfigurasi.Model.Konfigurasi
{
    public class MstController
    {
        [Key]
        public string IdController { get; set; } = "";

        public string NamaController { get; set; } = "";

        public string Url { get; set; } = ""; // 🔥 WAJIB

        public string IdMenu { get; set; } = "";

        public int NoUrut { get; set; }

        // 🔥 TAMBAHAN
        public string? Icon { get; set; } // untuk sidebar item

        [ForeignKey("IdMenu")]
        public MstMenu? Menu { get; set; }

        public ICollection<MstAction> Actions { get; set; } = new List<MstAction>();
    }
}
