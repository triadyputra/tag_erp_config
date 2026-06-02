using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tagApiKonfigurasi.Model.Konfigurasi
{
    public class MstAction
    {
        public string IdAction { get; set; } = "";

        public string NamaAction { get; set; } = "";

        public string IdController { get; set; } = "";

        public int NoUrut { get; set; }

        [ForeignKey("IdController")]
        public MstController? Controller { get; set; }
    }
}
