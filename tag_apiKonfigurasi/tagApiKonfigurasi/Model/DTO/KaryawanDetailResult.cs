namespace tagApiKonfigurasi.Model.DTO
{
    public class KaryawanProfile
    {
        public string? NOKTP { get; set; }
        public string? NAMALENGKAP { get; set; }
        public string? KELAMIN { get; set; }
        public DateTime? TGLLAHIR { get; set; }
        public string? ALAMAT { get; set; }
        public DateTime? TGLMASUK { get; set; }
        //public string? FOTO { get; set; }
        public byte[]? FOTO { get; set; }

        public string? FOTO_BASE64 { get; set; } // 🔥 untuk frontend
        public DateTime? ResignDate { get; set; }
        public string? NMVENDOR { get; set; }
        public string? NIKSISTAG { get; set; }
    }
}
