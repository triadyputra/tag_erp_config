using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tagApiKonfigurasi.Data;
using tagApiKonfigurasi.Filter;
using tagApiKonfigurasi.Model;
using tagApiKonfigurasi.Model.MasterData;

namespace tagApiKonfigurasi.Controllers.master_data
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MstCabangController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MstCabangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET LIST (PAGINATION)
        // ==========================================
        [ApiKeyAuthorize]
        [HttpGet("GetListCabang")]
        public async Task<ActionResult<PaginatedResponse<TblCabang>>> GetListCabang(
            [FromQuery] string? filter = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var query = _context.TblCabang.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    query = query.Where(x =>
                            x.KdCabang.ToLower().Contains(filter.ToLower()) ||
                            x.NmCabang.ToLower().Contains(filter.ToLower()));
                }

                var totalCount = await query.CountAsync();

                var data = await query
                    .OrderBy(x => x.KdCabang)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new PaginatedResponse<TblCabang>
                {
                    Data = data,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

        // ==========================================
        // GET BY ID
        // ==========================================
        [HttpGet("{kode}")]
        public async Task<ActionResult<ApiResponse<TblCabang>>> Get(string kode)
        {
            var data = await _context.TblCabang.FindAsync(kode);

            if (data == null)
                return NotFound(ApiResponse<object>.Error("Data not found", "404"));

            return Ok(ApiResponse<TblCabang>.Success(data));
        }

        // ==========================================
        // CREATE
        // ==========================================
        [ApiKeyAuthorize]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> PostCabang([FromBody] TblCabang dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.KdCabang))
                    return BadRequest(ApiResponse<object>.Error("Kode Cabang wajib", "400"));

                if (await _context.TblCabang.AnyAsync(x => x.KdCabang == dto.KdCabang))
                    return BadRequest(ApiResponse<object>.Error("Kode sudah ada", "400"));

                _context.TblCabang.Add(dto);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessNoData());
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

        // ==========================================
        // UPDATE
        // ==========================================
        [ApiKeyAuthorize]
        [HttpPut]
        public async Task<ActionResult<ApiResponse<object>>> PutCabang([FromBody] TblCabang dto)
        {
            var entity = await _context.TblCabang.FindAsync(dto.KdCabang);

            if (entity == null)
                return NotFound(ApiResponse<object>.Error("Data not found", "404"));

            entity.NmCabang = dto.NmCabang;
            entity.Alamat = dto.Alamat;
            entity.Telepon = dto.Telepon;
            entity.Fax = dto.Fax;
            entity.KdPos = dto.KdPos;
            entity.Kota = dto.Kota;
            entity.KaCab = dto.KaCab;
            entity.Grup = dto.Grup;
            entity.NoCab = dto.NoCab;
            entity.KodeCab = dto.KodeCab;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessNoData());
        }

        // ==========================================
        // DELETE
        // ==========================================
        [ApiKeyAuthorize]
        [HttpDelete("{kode}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteCabang(string kode)
        {
            var entity = await _context.TblCabang.FindAsync(kode);

            if (entity == null)
                return NotFound(ApiResponse<object>.Error("Data not found", "404"));

            _context.TblCabang.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessNoData("Deleted", "200"));
        }

        // ==========================================
        // DOWNLOAD TEMPLATE
        // ==========================================
        [ApiKeyAuthorize]
        [HttpGet("DownloadTemplate")]
        public IActionResult DownloadTemplateCabang()
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("MasterCabang");

            ws.Cell("A1").Value = "KdCabang";
            ws.Cell("B1").Value = "NmCabang";
            ws.Cell("C1").Value = "Alamat";
            ws.Cell("D1").Value = "Telepon";
            ws.Cell("E1").Value = "Fax";
            ws.Cell("F1").Value = "KdPos";
            ws.Cell("G1").Value = "Kota";
            ws.Cell("H1").Value = "KaCab";
            ws.Cell("I1").Value = "Grup";
            ws.Cell("J1").Value = "NoCab";
            ws.Cell("K1").Value = "KodeCab";

            ws.Range("A1:K1").Style.Font.Bold = true;
            ws.Range("A1:K1").Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.SheetView.FreezeRows(1);

            // Sample
            ws.Cell("A2").Value = "CB001";
            ws.Cell("B2").Value = "Cabang Jakarta";
            ws.Cell("C2").Value = "Jl. Sudirman No 1";
            ws.Cell("D2").Value = "021123456";
            ws.Cell("E2").Value = "-";
            ws.Cell("F2").Value = "10110";
            ws.Cell("G2").Value = "Jakarta";
            ws.Cell("H2").Value = "Budi";
            ws.Cell("I2").Value = "GRUP1";
            ws.Cell("J2").Value = "01";
            ws.Cell("K2").Value = "JKT01";

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Template_MasterCabang.xlsx"
            );
        }

        private byte[] GenerateErrorExcelCabang(List<(int Row, string Message)> errors)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Upload_Error");

            ws.Cell(1, 1).Value = "Row Excel";
            ws.Cell(1, 2).Value = "Error Message";

            ws.Range("A1:B1").Style.Font.Bold = true;
            ws.Range("A1:B1").Style.Fill.BackgroundColor = XLColor.LightPink;

            int row = 2;
            foreach (var err in errors)
            {
                ws.Cell(row, 1).Value = err.Row;
                ws.Cell(row, 2).Value = err.Message;
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream.ToArray();
        }

        // ==========================================
        // UPLOAD EXCEL
        // ==========================================
        [ApiKeyAuthorize]
        [HttpPost("Upload")]
        public async Task<IActionResult> UploadCabang(IFormFile file)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync<IActionResult>(async () =>
            {
                await using var tx = await _context.Database.BeginTransactionAsync();

                try
                {
                    if (file == null || file.Length == 0)
                        return Ok(ApiResponse<object>.Error("File tidak ditemukan", "400"));

                    if (!file.FileName.EndsWith(".xlsx"))
                        return Ok(ApiResponse<object>.Error("Format file harus .xlsx", "400"));

                    var errors = new List<(int Row, string Message)>();
                    var cabangList = new List<TblCabang>();

                    using var stream = file.OpenReadStream();
                    using var wb = new XLWorkbook(stream);

                    if (!wb.Worksheets.TryGetWorksheet("MasterCabang", out IXLWorksheet? ws))
                        return Ok(ApiResponse<object>.Error("Worksheet tidak ditemukan.", "400"));

                    var rows = ws.RangeUsed()?.RowsUsed().Skip(1).ToList();
                    if (rows == null || !rows.Any())
                        return Ok(ApiResponse<object>.Error("File tidak memiliki data.", "400"));

                    foreach (var row in rows)
                    {
                        var rowNo = row.RowNumber();

                        try
                        {
                            if (row.IsEmpty())
                                continue;

                            var kode = row.Cell(1).GetString()?.Trim().ToUpper();
                            var nama = row.Cell(2).GetString()?.Trim();

                            if (string.IsNullOrWhiteSpace(kode))
                                throw new Exception("KdCabang kosong");

                            if (string.IsNullOrWhiteSpace(nama))
                                throw new Exception("NmCabang kosong");

                            // 🔥 CEK DUPLIKAT DB
                            if (await _context.TblCabang.AnyAsync(x => x.KdCabang == kode))
                                throw new Exception($"Kode {kode} sudah ada");

                            cabangList.Add(new TblCabang
                            {
                                KdCabang = kode,
                                NmCabang = nama,
                                Alamat = row.Cell(3).GetString(),
                                Telepon = row.Cell(4).GetString(),
                                Fax = row.Cell(5).GetString(),
                                KdPos = row.Cell(6).GetString(),
                                Kota = row.Cell(7).GetString(),
                                KaCab = row.Cell(8).GetString(),
                                Grup = row.Cell(9).GetString(),
                                NoCab = row.Cell(10).GetString(),
                                KodeCab = row.Cell(11).GetString()
                            });
                        }
                        catch (Exception ex)
                        {
                            errors.Add((rowNo, ex.Message));
                        }
                    }

                    // ================= ERROR FILE =================
                    if (errors.Any())
                    {
                        var errorFile = GenerateErrorExcelCabang(errors);

                        return File(
                            errorFile,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"Upload_MasterCabang_Error_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                        );
                    }

                    // ================= SAVE =================
                    _context.TblCabang.AddRange(cabangList);
                    await _context.SaveChangesAsync();

                    await tx.CommitAsync();

                    return Ok(ApiResponse<object>.SuccessNoData("Upload berhasil"));
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();

                    var realError =
                        ex.InnerException?.InnerException?.Message
                        ?? ex.InnerException?.Message
                        ?? ex.Message;

                    var errorFile = GenerateErrorExcelCabang(new List<(int, string)>
            {
                (0, $"Database Error: {realError}")
            });

                    return File(
                        errorFile,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Upload_MasterCabang_Error_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                    );
                }
            });
        }
    }
}
