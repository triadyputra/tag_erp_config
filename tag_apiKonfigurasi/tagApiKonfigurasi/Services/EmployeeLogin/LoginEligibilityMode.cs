namespace tagApiKonfigurasi.Services.EmployeeLogin
{
    public enum LoginEligibilityMode
    {
        /// <summary>Kontrak harus aktif hari ini (PAWAL &lt;= today &lt;= PAKHIR).</summary>
        WebStrict,

        /// <summary>Kontrak boleh habis hingga N hari lalu (grace, default 30).</summary>
        MobileGrace,
    }
}
