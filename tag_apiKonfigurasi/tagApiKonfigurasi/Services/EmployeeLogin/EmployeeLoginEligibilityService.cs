using Dapper;
using tagApiKonfigurasi.Data;

namespace tagApiKonfigurasi.Services.EmployeeLogin
{
    public class EmployeeLoginEligibilityService : IEmployeeLoginEligibilityService
    {
        private readonly DapperSistagHrdContext _context;
        private readonly IConfiguration _configuration;

        public EmployeeLoginEligibilityService(
            DapperSistagHrdContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<EmployeeLoginEligibilityResult> ValidateAsync(
            string? noKtp,
            LoginEligibilityMode mode,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(noKtp))
                return EmployeeLoginEligibilityResult.Fail(
                    EmployeeLoginEligibilityMessages.NoKtpNotLinked);

            var noktp = noKtp.Trim();
            var row = await LoadRowAsync(noktp);

            if (row == null)
                return EmployeeLoginEligibilityResult.Fail(
                    EmployeeLoginEligibilityMessages.KtpNotFound);

            var today = DateTime.Today;

            if (row.ResignDate.HasValue && row.ResignDate.Value.Date <= today)
                return EmployeeLoginEligibilityResult.Fail(
                    EmployeeLoginEligibilityMessages.Resigned);

            if (row.IsPermanent)
                return EmployeeLoginEligibilityResult.Ok();

            if (mode == LoginEligibilityMode.WebStrict)
            {
                if (row.HasActiveContractToday)
                    return EmployeeLoginEligibilityResult.Ok();

                return EmployeeLoginEligibilityResult.Fail(
                    EmployeeLoginEligibilityMessages.ContractInactive);
            }

            var graceDays = _configuration.GetValue("Login:MobileContractGraceDays", 30);
            if (graceDays < 0)
                graceDays = 0;

            var cutoff = today.AddDays(-graceDays);

            if (row.LatestPakhir.HasValue && row.LatestPakhir.Value.Date >= cutoff)
                return EmployeeLoginEligibilityResult.Ok();

            var graceMessage = graceDays == 30
                ? EmployeeLoginEligibilityMessages.ContractExpiredGrace
                : $"Kontrak karyawan telah berakhir lebih dari {graceDays} hari";

            return EmployeeLoginEligibilityResult.Fail(graceMessage);
        }

        private async Task<EmployeeLoginEligibilityRow?> LoadRowAsync(string noktp)
        {
            //const string sql = @"
            //    SELECT
            //        ktp.NOKTP AS Noktp,
            //        CASE
            //            WHEN pk.TGLRESIGN IS NOT NULL THEN pk.TGLRESIGN
            //            WHEN hk.PAKHIRFREE IS NOT NULL THEN hk.PAKHIRFREE
            //            ELSE NULL
            //        END AS ResignDate,
            //        CAST(NULL AS DATETIME) AS ResignDate,
            //        CASE WHEN tetap.NOKTP IS NOT NULL THEN 1 ELSE 0 END AS IsPermanent,
            //        kontrak.LatestPakhir,
            //        ISNULL(kontrak.HasActiveContractToday, 0) AS HasActiveContractToday
            //    FROM HRDTAG.dbo.MST_KTP ktp
            //    LEFT JOIN SISTAGHRD.dbo.TRX_KARYAWANPK pk
            //        ON ktp.NOKTP COLLATE DATABASE_DEFAULT = pk.NOKTP COLLATE DATABASE_DEFAULT
            //    LEFT JOIN SISTAGHRD.dbo.TRX_KARYAWANHK hk
            //        ON ktp.NOKTP COLLATE DATABASE_DEFAULT = hk.NIKKTP COLLATE DATABASE_DEFAULT
            //    LEFT JOIN HRDTAG.dbo.MST_KARYAWANTETAP tetap
            //        ON ktp.NOKTP COLLATE DATABASE_DEFAULT = tetap.NOKTP COLLATE DATABASE_DEFAULT
            //    OUTER APPLY (
            //        SELECT
            //            MAX(k.PAKHIR) AS LatestPakhir,
            //            MAX(CASE
            //                WHEN CAST(GETDATE() AS date) >= CAST(k.PAWAL AS date)
            //                 AND CAST(GETDATE() AS date) <= CAST(k.PAKHIR AS date)
            //                THEN 1 ELSE 0 END) AS HasActiveContractToday
            //        FROM SISTAGHRD.dbo.TRX_KONTRAKKARYAWAN k
            //        WHERE ktp.NOKTP COLLATE DATABASE_DEFAULT = k.NOKTP COLLATE DATABASE_DEFAULT
            //    ) kontrak
            //    WHERE ktp.NOKTP = @noktp";

            const string sql = @"
                SELECT
                    ktp.NOKTP AS Noktp,
                    CAST(NULL AS DATETIME) AS ResignDate,
                    CASE WHEN tetap.NOKTP IS NOT NULL THEN 1 ELSE 0 END AS IsPermanent,
                    kontrak.LatestPakhir,
                    ISNULL(kontrak.HasActiveContractToday, 0) AS HasActiveContractToday
                FROM HRDTAG.dbo.MST_KTP ktp
                LEFT JOIN SISTAGHRD.dbo.TRX_KARYAWANPK pk
                    ON ktp.NOKTP COLLATE DATABASE_DEFAULT = pk.NOKTP COLLATE DATABASE_DEFAULT
                LEFT JOIN SISTAGHRD.dbo.TRX_KARYAWANHK hk
                    ON ktp.NOKTP COLLATE DATABASE_DEFAULT = hk.NIKKTP COLLATE DATABASE_DEFAULT
                LEFT JOIN HRDTAG.dbo.MST_KARYAWANTETAP tetap
                    ON ktp.NOKTP COLLATE DATABASE_DEFAULT = tetap.NOKTP COLLATE DATABASE_DEFAULT
                OUTER APPLY (
                    SELECT
                        MAX(k.PAKHIR) AS LatestPakhir,
                        MAX(CASE
                            WHEN CAST(GETDATE() AS date) >= CAST(k.PAWAL AS date)
                             AND CAST(GETDATE() AS date) <= CAST(k.PAKHIR AS date)
                            THEN 1 ELSE 0 END) AS HasActiveContractToday
                    FROM SISTAGHRD.dbo.TRX_KONTRAKKARYAWAN k
                    WHERE ktp.NOKTP COLLATE DATABASE_DEFAULT = k.NOKTP COLLATE DATABASE_DEFAULT
                ) kontrak
                WHERE ktp.NOKTP = @noktp";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<EmployeeLoginEligibilityRow>(
                sql,
                new { noktp });
        }
    }
}
