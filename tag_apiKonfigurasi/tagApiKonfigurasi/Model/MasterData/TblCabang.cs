using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tagApiKonfigurasi.Model.MasterData
{
    public class TblCabang
    {
        [Key]
        [Column("KDCABANG")]
        [MaxLength(10)]
        public string KdCabang { get; set; } = "";

        [Column("NMCABANG")]
        [MaxLength(50)]
        public string? NmCabang { get; set; }

        [Column("ALAMAT")]
        [MaxLength(100)]
        public string? Alamat { get; set; }

        [Column("TELEPON")]
        [MaxLength(30)]
        public string? Telepon { get; set; }

        [Column("FAX")]
        [MaxLength(30)]
        public string? Fax { get; set; }

        [Column("KDPOS")]
        [MaxLength(10)]
        public string? KdPos { get; set; }

        [Column("KOTA")]
        [MaxLength(50)]
        public string? Kota { get; set; }

        [Column("KACAB")]
        [MaxLength(50)]
        public string? KaCab { get; set; }

        [Column("GRUP")]
        [MaxLength(20)]
        public string? Grup { get; set; }

        [Column("NOCAB")]
        [MaxLength(2)]
        public string? NoCab { get; set; }

        [Column("KODECAB")]
        [MaxLength(20)]
        public string? KodeCab { get; set; }

        [Column("KDGROUP")]
        [MaxLength(20)]
        public string? KdGroup { get; set; }
        public bool Status { get; set; }
    }
}
