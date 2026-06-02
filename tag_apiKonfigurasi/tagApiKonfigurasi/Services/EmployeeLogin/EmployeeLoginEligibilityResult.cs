namespace tagApiKonfigurasi.Services.EmployeeLogin
{
    public sealed class EmployeeLoginEligibilityResult
    {
        public bool IsEligible { get; init; }
        public string? Message { get; init; }

        public static EmployeeLoginEligibilityResult Ok() =>
            new() { IsEligible = true };

        public static EmployeeLoginEligibilityResult Fail(string message) =>
            new() { IsEligible = false, Message = message };
    }

    public static class EmployeeLoginEligibilityMessages
    {
        public const string NoKtpNotLinked = "Akun belum dikaitkan dengan No KTP";
        public const string KtpNotFound = "No KTP tidak ditemukan di Master KTP";
        public const string Resigned = "Status karyawan tidak aktif (sudah resign)";
        public const string ContractInactive = "Kontrak karyawan tidak aktif";
        public const string ContractExpiredGrace = "Kontrak karyawan telah berakhir lebih dari 30 hari";
    }
}
