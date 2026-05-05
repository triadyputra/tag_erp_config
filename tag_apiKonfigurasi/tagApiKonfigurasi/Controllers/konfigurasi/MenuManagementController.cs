using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tagApiKonfigurasi.Data;
using tagApiKonfigurasi.Model;
using tagApiKonfigurasi.Model.DTO;
using tagApiKonfigurasi.Model.Konfigurasi;

namespace tagApiKonfigurasi.Controllers.konfigurasi
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MenuManagementController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MenuManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET LIST (PAGINATION)
        // =========================
        [HttpGet("GetListMenu")]
        public async Task<ActionResult<PaginatedResponse<MstMenu>>> GetListMenu(
        [FromQuery] string? filter = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var query = _context.Menus
                    .Include(x => x.Modul) // 🔥 TAMBAH
                    .Include(x => x.Controllers)
                    .ThenInclude(x => x.Actions)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(filter))
                {
                    query = query.Where(x =>
                        x.NamaMenu.ToUpper().Contains(filter.ToUpper()));
                }

                var count = await query.CountAsync();

                var data = await query
                    .OrderBy(x => x.NoUrut)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new PaginatedResponse<MstMenu>
                {
                    Data = data,
                    TotalCount = count,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(count / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

        // =========================
        // GET DETAIL
        // =========================
        // =========================
        // GET DETAIL
        // =========================
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<MstMenu>>> GetMenu(string id)
        {
            try
            {
                var data = await _context.Menus
                    .Include(x => x.Modul) // 🔥 TAMBAH
                    .Include(x => x.Controllers)
                    .ThenInclude(x => x.Actions)
                    .FirstOrDefaultAsync(x => x.IdMenu == id);

                if (data == null)
                    return Ok(ApiResponse<object>.Error("Data not found", "404"));

                return Ok(ApiResponse<MstMenu>.Success(data));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

        // =========================
        // CREATE
        // =========================
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> PostMenu([FromBody] FormMenuDto dto)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var trx = await _context.Database.BeginTransactionAsync();

                    try
                    {
                        // ================= VALIDASI =================
                        if (string.IsNullOrWhiteSpace(dto.NamaMenu))
                            return Ok(ApiResponse<object>.Error("NamaMenu wajib diisi", "400"));

                        if (string.IsNullOrWhiteSpace(dto.IdModul))
                            return Ok(ApiResponse<object>.Error("IdModul wajib diisi", "400"));

                        if (dto.Controllers == null || dto.Controllers.Count == 0)
                            return Ok(ApiResponse<object>.Error("Minimal 1 controller", "400"));

                        // ================= AUTO CREATE MODUL =================
                        var modul = await _context.Moduls
                            .FirstOrDefaultAsync(x => x.IdModul == dto.IdModul);

                        if (modul == null)
                        {
                            modul = new MstModul
                            {
                                IdModul = dto.IdModul,
                                KodeModul = dto.IdModul,
                                NamaModul = dto.IdModul, // bisa kamu ganti nanti
                                NoUrut = 0
                            };

                            _context.Moduls.Add(modul);
                            await _context.SaveChangesAsync(); // 🔥 penting supaya FK valid
                        }

                        // ================= MENU =================
                        var menu = new MstMenu
                        {
                            IdMenu = string.IsNullOrEmpty(dto.IdMenu)
                                ? Guid.NewGuid().ToString()
                                : dto.IdMenu,

                            NamaMenu = dto.NamaMenu,
                            NoUrut = dto.NoUrut,
                            IdModul = modul.IdModul,
                            Icon = dto.Icon,
                            ParentId = dto.ParentId
                        };

                        // ================= CONTROLLER =================
                        foreach (var c in dto.Controllers)
                        {
                            if (string.IsNullOrWhiteSpace(c.NamaController))
                                continue;

                            if (string.IsNullOrWhiteSpace(c.Url))
                                return Ok(ApiResponse<object>.Error($"URL wajib diisi untuk controller {c.NamaController}", "400"));

                            var controller = new MstController
                            {
                                IdController = string.IsNullOrEmpty(c.IdController)
                                    ? Guid.NewGuid().ToString()
                                    : c.IdController,

                                NamaController = c.NamaController,
                                Url = c.Url,
                                NoUrut = c.NoUrut,
                                IdMenu = menu.IdMenu,
                                Icon = c.Icon
                            };

                            // ================= ACTION =================
                            if (c.Actions != null)
                            {
                                foreach (var a in c.Actions)
                                {
                                    if (string.IsNullOrWhiteSpace(a.NamaAction))
                                        continue;

                                    controller.Actions.Add(new MstAction
                                    {
                                        IdAction = string.IsNullOrEmpty(a.IdAction)
                                            ? Guid.NewGuid().ToString()
                                            : a.IdAction,

                                        NamaAction = a.NamaAction,
                                        NoUrut = a.NoUrut
                                    });
                                }
                            }

                            menu.Controllers.Add(controller);
                        }

                        // ================= SAVE =================
                        _context.Menus.Add(menu);
                        await _context.SaveChangesAsync();

                        await trx.CommitAsync();

                        return Ok(ApiResponse<object>.SuccessNoData("Data berhasil disimpan"));
                    }
                    catch
                    {
                        await trx.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

        // =========================
        // UPDATE
        // =========================
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> PutMenu(string id, [FromBody] FormMenuDto dto)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var trx = await _context.Database.BeginTransactionAsync();

                    try
                    {
                        var menu = await _context.Menus
                            .Include(x => x.Controllers)
                            .ThenInclude(x => x.Actions)
                            .FirstOrDefaultAsync(x => x.IdMenu == id);

                        if (menu == null)
                            return Ok(ApiResponse<object>.Error("Data not found", "404"));

                        if (string.IsNullOrWhiteSpace(dto.NamaMenu))
                            return Ok(ApiResponse<object>.Error("NamaMenu wajib diisi", "400"));

                        // UPDATE MENU
                        menu.NamaMenu = dto.NamaMenu;
                        menu.NoUrut = dto.NoUrut;
                        menu.IdModul = dto.IdModul; // 🔥 TAMBAH
                        menu.Icon = dto.Icon;
                        menu.ParentId = dto.ParentId;

                        // DELETE LAMA
                        if (menu.Controllers != null && menu.Controllers.Count > 0)
                        {
                            var oldActions = menu.Controllers.SelectMany(x => x.Actions).ToList();

                            if (oldActions.Any())
                                _context.Actions.RemoveRange(oldActions);

                            _context.Controllers.RemoveRange(menu.Controllers);
                        }

                        // INSERT BARU
                        menu.Controllers = new List<MstController>();

                        foreach (var c in dto.Controllers)
                        {
                            if (string.IsNullOrWhiteSpace(c.NamaController))
                                continue;

                            var controller = new MstController
                            {
                                IdController = string.IsNullOrEmpty(c.IdController)
                                    ? Guid.NewGuid().ToString()
                                    : c.IdController,

                                NamaController = c.NamaController,
                                Url = c.Url, // 🔥 TAMBAH
                                NoUrut = c.NoUrut,
                                IdMenu = menu.IdMenu,
                                Icon = c.Icon
                            };

                            if (c.Actions != null)
                            {
                                foreach (var a in c.Actions)
                                {
                                    if (string.IsNullOrWhiteSpace(a.NamaAction))
                                        continue;

                                    controller.Actions.Add(new MstAction
                                    {
                                        IdAction = string.IsNullOrEmpty(a.IdAction)
                                            ? Guid.NewGuid().ToString()
                                            : a.IdAction,

                                        NamaAction = a.NamaAction,
                                        NoUrut = a.NoUrut
                                    });
                                }
                            }

                            menu.Controllers.Add(controller);
                        }

                        await _context.SaveChangesAsync();
                        await trx.CommitAsync();

                        return Ok(ApiResponse<object>.SuccessNoData("Data berhasil diupdate"));
                    }
                    catch
                    {
                        await trx.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

        // =========================
        // DELETE
        // =========================
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteMenu(string id)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var trx = await _context.Database.BeginTransactionAsync();

                    try
                    {
                        var menu = await _context.Menus
                            .Include(x => x.Controllers)
                            .ThenInclude(x => x.Actions)
                            .FirstOrDefaultAsync(x => x.IdMenu == id);

                        if (menu == null)
                            return Ok(ApiResponse<object>.Error("Data not found", "404"));

                        // 🔥 SIMPAN DULU MODUL ID
                        var modulId = menu.IdModul;

                        // ================= DELETE =================
                        var actions = menu.Controllers.SelectMany(x => x.Actions).ToList();

                        if (actions.Any())
                            _context.Actions.RemoveRange(actions);

                        if (menu.Controllers.Any())
                            _context.Controllers.RemoveRange(menu.Controllers);

                        _context.Menus.Remove(menu);

                        // 🔥 SAVE DULU AGAR MENU SUDAH HILANG DI DB
                        await _context.SaveChangesAsync();

                        // ================= CEK MODUL =================
                        var masihDipakai = await _context.Menus
                            .AnyAsync(x => x.IdModul == modulId);

                        if (!masihDipakai)
                        {
                            var modul = await _context.Moduls
                                .FirstOrDefaultAsync(x => x.IdModul == modulId);

                            if (modul != null)
                                _context.Moduls.Remove(modul);
                        }

                        // 🔥 SAVE LAGI UNTUK MODUL
                        await _context.SaveChangesAsync();

                        await trx.CommitAsync();

                        return Ok(ApiResponse<object>.SuccessNoData("Data berhasil dihapus"));
                    }
                    catch
                    {
                        await trx.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }
    }
}
