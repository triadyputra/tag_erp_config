using Dapper;
using tagApiKonfigurasi.Data;
using tagApiKonfigurasi.Model.DTO;

namespace tagApiKonfigurasi.Services.MasterKtp
{
    public class MasterKtpLookupService : IMasterKtpLookupService
    {
        private readonly DapperSistagHrdContext _context;

        public MasterKtpLookupService(DapperSistagHrdContext context)
        {
            _context = context;
        }

        public async Task<List<MasterKtpLookupDto>> SearchAsync(string? nama, string? cabang)
        {
            const string sql = @"
                SELECT 
                    NOKTP, 
                    NAMALENGKAP, 
                    KELAMIN, 
                    KDCABANG 
                FROM HRDTAG.dbo.MST_KTP
                WHERE 
                    (@ckdcabang IS NULL OR KDCABANG = @ckdcabang)
                    AND (@cnmlengkap IS NULL OR NAMALENGKAP LIKE '%' + @cnmlengkap + '%')
                ORDER BY NAMALENGKAP";

            using var connection = _context.CreateConnection();
            var data = await connection.QueryAsync<MasterKtpLookupDto>(
                sql,
                new
                {
                    ckdcabang = string.IsNullOrWhiteSpace(cabang) ? null : cabang.Trim(),
                    cnmlengkap = string.IsNullOrWhiteSpace(nama) ? null : nama.Trim(),
                });

            return data.ToList();
        }

        public async Task<MasterKtpLookupDto?> GetByNoKtpAsync(string noktp)
        {
            if (string.IsNullOrWhiteSpace(noktp))
                return null;

            const string sql = @"
                SELECT 
                    a.NOKTP, 
                    a.NAMALENGKAP, 
                    a.KDCABANG,
                    RIGHT('00000000' + CAST(CAST(mp.NIKSISTAG AS INT) AS VARCHAR(8)), 8) AS NIKSISTAG
                FROM HRDTAG.dbo.MST_KTP a
                LEFT JOIN HRDTAG.dbo.v_MASTERPEGAWAI mp
                    ON a.NOKTP COLLATE DATABASE_DEFAULT = mp.NOKTP COLLATE DATABASE_DEFAULT
                WHERE a.NOKTP = @noktp";

            //const string sql = @"
            //    SELECT 
            //        a.NOKTP, 
            //        a.NAMALENGKAP, 
            //        a.KDCABANG,
            //        mp.NIKSISTAG
            //    FROM HRDTAG.dbo.MST_KTP a
            //    LEFT JOIN HRDTAG.dbo.v_MASTERPEGAWAI mp
            //        ON a.NOKTP COLLATE DATABASE_DEFAULT = mp.NOKTP COLLATE DATABASE_DEFAULT
            //    WHERE a.NOKTP = @noktp";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<MasterKtpLookupDto>(
                sql,
                new { noktp = noktp.Trim() });
        }

        /*
        public async Task<KaryawanProfile?> GetDetailKaryawan(string noktp)
        {
            if (string.IsNullOrWhiteSpace(noktp))
                return null;

            const string sql = @"
                SELECT 
                    a.NOKTP, 
                    a.NAMALENGKAP, 
                    a.KELAMIN, 
                    a.TGLLAHIR, 
                    a.ALAMAT, 
                    a.TGLMASUK, 
                    a.FOTO, 
                    CASE
                        WHEN pk.TGLRESIGN IS NOT NULL THEN pk.TGLRESIGN
                        WHEN hk.PAKHIRFREE IS NOT NULL THEN hk.PAKHIRFREE
                        ELSE NULL
                    END AS ResignDate,
                    a.NMVENDOR,
                    mp.NIKSISTAG
                FROM HRDTAG.dbo.MST_KTP a
                LEFT JOIN SISTAGHRD.dbo.TRX_KARYAWANPK pk 
                    ON a.NOKTP COLLATE DATABASE_DEFAULT = pk.NOKTP COLLATE DATABASE_DEFAULT
                LEFT JOIN SISTAGHRD.dbo.TRX_KARYAWANHK hk 
                    ON a.NOKTP COLLATE DATABASE_DEFAULT = hk.NIKKTP COLLATE DATABASE_DEFAULT
                LEFT JOIN HRDTAG.dbo.v_MASTERPEGAWAI mp
                    ON a.NOKTP COLLATE DATABASE_DEFAULT = mp.NOKTP COLLATE DATABASE_DEFAULT
                WHERE a.NOKTP = @noktp";

            using var connection = _context.CreateConnection();
            var profile = await connection.QueryFirstOrDefaultAsync<KaryawanProfile>(
                sql,
                new { noktp = noktp.Trim() });

            if (profile?.FOTO is { Length: > 0 })
            {
                profile.FOTO_BASE64 = $"data:image/jpeg;base64,{Convert.ToBase64String(profile.FOTO)}";
            }

            return profile;
        }
        */
    }
}
