namespace tagApiKonfigurasi.Model
{
    public class UserSession
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;
        public string Modul { get; set; } = null!;

        public int SessionVersion { get; set; } = 0;
    }
}
