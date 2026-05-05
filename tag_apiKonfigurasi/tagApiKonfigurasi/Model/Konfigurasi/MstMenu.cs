using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Controller;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tagApiKonfigurasi.Model.Konfigurasi
{
    public class MstMenu
    {
        [Key]
        public string IdMenu { get; set; } = "";

        public string NamaMenu { get; set; } = "";

        public int NoUrut { get; set; }

        // 🔥 RELASI MODUL
        public string IdModul { get; set; } = "";

        [ForeignKey("IdModul")]
        public MstModul? Modul { get; set; }

        // 🔥 TAMBAHAN
        public string? Icon { get; set; } // untuk parent menu
        public string? ParentId { get; set; } // nested menu (opsional)

        public ICollection<MstController> Controllers { get; set; } = new List<MstController>();
    }
}
