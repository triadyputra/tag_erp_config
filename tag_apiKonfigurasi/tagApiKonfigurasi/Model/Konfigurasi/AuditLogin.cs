namespace tagApiKonfigurasi.Model.Konfigurasi
{
    public class AuditLogin
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public string Username { get; set; }

        public string IpAddress { get; set; }

        public string Device { get; set; }       // contoh: Chrome - Windows
        public string UserAgent { get; set; }

        // 🔥 LOKASI
        public string? City { get; set; }
        public string? Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // 🔐 SESSION TRACKING
        public string? SessionId { get; set; }    // unik per login
        public string? RefreshToken { get; set; } // optional

        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }

        public bool IsSuccess { get; set; }

        // 🔥 tambahan penting
        public string? FailReason { get; set; }   // "Password salah", "User tidak aktif"
        
        public string? Modul { get; set; }   // "Password salah", "User tidak aktif"
    }
}
