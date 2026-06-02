using Microsoft.EntityFrameworkCore;
using System;
using tagApiKonfigurasi.Data;
using tagApiKonfigurasi.Model.DTO;

namespace tagApiKonfigurasi.Services.Menu
{
    public class MenuService : IMenuService
    {
        private readonly ApplicationDbContext _context;

        public MenuService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<List<MenuViewModel>> GetMenuAsync(string? modul)
        {
            var menus = await _context.Menus
                .Where(m => string.IsNullOrEmpty(modul) || m.IdModul == modul)
                .Include(m => m.Modul)
                .Include(m => m.Controllers)
                    .ThenInclude(c => c.Actions)
                .OrderBy(m => m.Modul != null ? m.Modul.NoUrut : int.MaxValue)
                .ThenBy(m => m.NoUrut)
                .ToListAsync();

            return menus.Select(menu => new MenuViewModel
            {
                IdMenu = menu.IdMenu,
                NamaMenu = menu.NamaMenu,
                NoUrut = menu.NoUrut,
                IdModul = menu.IdModul,
                NamaModul = menu.Modul?.NamaModul ?? menu.IdModul,

                // 🔥 TAMBAHAN WAJIB
                ParentId = menu.ParentId,
                Icon = menu.Icon,

                ControllerViewModel = menu.Controllers
                    .OrderBy(c => c.NoUrut)
                    .Select(ctrl => new ControllerViewModel
                    {
                        IdController = ctrl.IdController,
                        Controller = ctrl.NamaController,
                        IdMenu = ctrl.IdMenu,
                        NoUrut = ctrl.NoUrut,

                        // 🔥 WAJIB INI
                        Url = ctrl.Url ?? "",
                        Icon = ctrl.Icon,

                        ActionViewModel = ctrl.Actions
                            .OrderBy(a => a.NoUrut)
                            .Select(act => new ActionViewModel
                            {
                                IdAction = act.IdAction,
                                NamaAction = act.NamaAction,
                                IdController = act.IdController,
                                NoUrut = act.NoUrut
                            }).ToList()
                    }).ToList()
            }).ToList();
        }
        //public async Task<List<MenuViewModel>> GetMenuAsync()
        //{
        //    var menus = await _context.Menus
        //        .Include(m => m.Controllers)
        //            .ThenInclude(c => c.Actions)
        //        .OrderBy(m => m.NoUrut)
        //        .ToListAsync();

        //    return menus.Select(menu => new MenuViewModel
        //    {
        //        IdMenu = menu.IdMenu,
        //        NamaMenu = menu.NamaMenu,
        //        NoUrut = menu.NoUrut,

        //        ControllerViewModel = menu.Controllers
        //            .OrderBy(c => c.NoUrut)
        //            .Select(ctrl => new ControllerViewModel
        //            {
        //                IdController = ctrl.IdController,
        //                Controller = ctrl.NamaController,
        //                IdMenu = ctrl.IdMenu,
        //                NoUrut = ctrl.NoUrut,

        //                ActionViewModel = ctrl.Actions
        //                    .OrderBy(a => a.NoUrut)
        //                    .Select(act => new ActionViewModel
        //                    {
        //                        IdAction = act.IdAction,
        //                        NamaAction = act.NamaAction,
        //                        IdController = act.IdController,
        //                        NoUrut = act.NoUrut
        //                    }).ToList()
        //            }).ToList()
        //    }).ToList();
        //}


    }
}
