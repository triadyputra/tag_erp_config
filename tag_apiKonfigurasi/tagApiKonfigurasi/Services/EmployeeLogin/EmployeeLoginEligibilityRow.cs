namespace tagApiKonfigurasi.Services.EmployeeLogin
{
    internal sealed class EmployeeLoginEligibilityRow
    {
        public string? Noktp { get; set; }
        public DateTime? ResignDate { get; set; }
        public bool IsPermanent { get; set; }
        public DateTime? LatestPakhir { get; set; }
        public bool HasActiveContractToday { get; set; }
    }
}
