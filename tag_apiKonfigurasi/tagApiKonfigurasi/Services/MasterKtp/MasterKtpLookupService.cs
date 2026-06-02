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
                    NOKTP, 
                    NAMALENGKAP, 
                    KDCABANG 
                FROM HRDTAG.dbo.MST_KTP
                WHERE NOKTP = @noktp";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<MasterKtpLookupDto>(
                sql,
                new { noktp = noktp.Trim() });
        }
    }
}
