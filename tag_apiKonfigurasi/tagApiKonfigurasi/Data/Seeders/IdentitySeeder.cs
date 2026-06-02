using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using tagApiKonfigurasi.Model;
using tagApiKonfigurasi.Model.Konfigurasi;

namespace tagApiKonfigurasi.Data.Seeders
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            const string ROLE_NAME = "SuperAdmin";
            const string ADMIN_EMAIL = "deri.triadi.putra@tag.com";
            const string ADMIN_USERNAME = "triady";
            const string CABANG_ID = "TAG01";

            var accessList = new[]
            {
                new { IdController = "Beranda", IdAction = "Read" },
                new { IdController = "Beranda", IdAction = "ProgresPemutahiran" },
                new { IdController = "Role", IdAction = "GetListRole" },
                new { IdController = "Role", IdAction = "PostRole" },
                new { IdController = "Role", IdAction = "PutRole" },
                new { IdController = "Role", IdAction = "DeleteRole" },
                new { IdController = "Akun", IdAction = "GetListAkun" },
                new { IdController = "Akun", IdAction = "PostAkun" },
                new { IdController = "Akun", IdAction = "PutAkun" },
                new { IdController = "Akun", IdAction = "DeleteAkun" },
                new { IdController = "AuditLogin", IdAction = "GetListAuditLogin" },
                new { IdController = "HrdGroupAkun", IdAction = "GetListRole" },
                new { IdController = "HrdGroupAkun", IdAction = "PostRole" },
                new { IdController = "HrdGroupAkun", IdAction = "PutRole" },
                new { IdController = "HrdGroupAkun", IdAction = "DeleteRole" }
            };

            var accessJson = JsonConvert.SerializeObject(accessList);

            // ================= ROLE =================
            var role = await roleManager.FindByNameAsync(ROLE_NAME);
            if (role == null)
            {
                role = new ApplicationRole
                {
                    Name = ROLE_NAME,
                    Access = accessJson
                };

                await roleManager.CreateAsync(role);
            }

            // ================= USER =================
            var user = await userManager.FindByNameAsync(ADMIN_USERNAME);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = ADMIN_USERNAME,
                    Email = ADMIN_EMAIL,
                    EmailConfirmed = true,
                    FullName = "Deri Triadi Putra",
                    Active = true,
                    Cabang = CABANG_ID
                };

                await userManager.CreateAsync(user, "P@ssw0rd");
            }

            if (!await userManager.IsInRoleAsync(user, ROLE_NAME))
                await userManager.AddToRoleAsync(user, ROLE_NAME);

            // 🔥 MENU SYSTEM
            await SeedMenuSystem(context);
        }

        // =====================================================
        // MENU + CONTROLLER + ACTION
        // =====================================================
        private static async Task SeedMenuSystem(ApplicationDbContext context)
        {
            // ================= MODUL =================
            if (!await context.Set<MstModul>().AnyAsync(x => x.IdModul == "CONFIG"))
            {
                context.Add(
                    new MstModul { IdModul = "CONFIG", KodeModul = "CONFIG", NamaModul = "Konfigurasi SIstem", NoUrut = 1 }
                );
                await context.SaveChangesAsync();
            }

            if (!await context.Set<MstModul>().AnyAsync(x => x.IdModul == "HRD"))
            {
                context.Add(
                    new MstModul { IdModul = "HRD", KodeModul = "HRD", NamaModul = "Human Resource", NoUrut = 2 }
                );
                await context.SaveChangesAsync();
            }

            // ================= MENU =================
            var menus = new List<MstMenu>
            {
                new() { IdMenu = "Home", NamaMenu = "Home", IdModul = "CONFIG", Icon = "IconHome", NoUrut = 1 },

                new() { IdMenu = "Konfigurasi", NamaMenu = "Konfigurasi", IdModul = "CONFIG", Icon = "IconSettings", NoUrut = 2 },

                new() { IdMenu = "MasterData", NamaMenu = "Master Data", IdModul = "CONFIG", Icon = "IconDatabase", NoUrut = 3 },

                new() { IdMenu = "PengaturanHrd", NamaMenu = "Pengaturan", IdModul = "HRD", Icon = "IconSettings", NoUrut = 99 },

            };

            foreach (var menu in menus)
            {
                if (!await context.Menus.AnyAsync(x => x.IdMenu == menu.IdMenu))
                    context.Menus.Add(menu);
            }

            await context.SaveChangesAsync();

            // ================= CONTROLLER =================
            var controllers = new List<MstController>
            {
                new() { IdController = "Beranda", NamaController = "Dashboard", IdMenu = "Home", Url = "/", Icon = "IconChartHistogram", NoUrut = 1 },

                new() { IdController = "Role", NamaController = "Group Akun", IdMenu = "Konfigurasi", Url = "/konfigurasi/group", Icon = "IconLockAccess", NoUrut = 1 },

                new() { IdController = "Akun", NamaController = "User Akun", IdMenu = "Konfigurasi", Url = "/konfigurasi/akun", Icon = "IconUsers", NoUrut = 2 },

                new() { IdController = "MenuManagement", NamaController = "Manajemen Menu", IdMenu = "Konfigurasi", Url = "/konfigurasi/menu", Icon = "IconHierarchy", NoUrut = 3 },
                
                new() { IdController = "AuditLogin", NamaController = "Log Login", IdMenu = "Konfigurasi", Url = "/konfigurasi/audit-login", Icon = "IconHistory", NoUrut = 4 },

                new() { IdController = "MstCabang", NamaController = "Master Cabang", IdMenu = "MasterData", Url = "/master-data/cabang", Icon = "IconHome", NoUrut = 1 },

                new() { IdController = "HrdGroupAkun", NamaController = "Group Akun", IdMenu = "PengaturanHrd", Url = "/hrd/group-akun", Icon = "IconLockAccess", NoUrut = 1 },
            };

            foreach (var ctrl in controllers)
            {
                if (!await context.Controllers.AnyAsync(x => x.IdController == ctrl.IdController))
                    context.Controllers.Add(ctrl);
            }

            await context.SaveChangesAsync();

            // ================= ACTION =================
            var actions = new List<MstAction>
            {
                new() { IdAction = "Read", NamaAction = "Lihat", IdController = "Beranda", NoUrut = 1 },

                new() { IdAction = "GetListRole", NamaAction = "Lihat", IdController = "Role", NoUrut = 1 },
                new() { IdAction = "PostRole", NamaAction = "Tambah", IdController = "Role", NoUrut = 2 },
                new() { IdAction = "PutRole", NamaAction = "Edit", IdController = "Role", NoUrut = 3 },
                new() { IdAction = "DeleteRole", NamaAction = "Hapus", IdController = "Role", NoUrut = 4 },

                new() { IdAction = "GetListAkun", NamaAction = "Lihat", IdController = "Akun", NoUrut = 1 },
                new() { IdAction = "PostAkun", NamaAction = "Tambah", IdController = "Akun", NoUrut = 2 },
                new() { IdAction = "PutAkun", NamaAction = "Edit", IdController = "Akun", NoUrut = 3 },
                new() { IdAction = "DeleteAkun", NamaAction = "Hapus", IdController = "Akun", NoUrut = 4 },

                new() { IdAction = "GetListMenu", NamaAction = "Lihat", IdController = "MenuManagement", NoUrut = 1 },
                new() { IdAction = "PostMenu", NamaAction = "Tambah", IdController = "MenuManagement", NoUrut = 2 },
                new() { IdAction = "PutMenu", NamaAction = "Edit", IdController = "MenuManagement", NoUrut = 3 },
                new() { IdAction = "DeleteMenu", NamaAction = "Hapus", IdController = "MenuManagement", NoUrut = 4 },

                new() { IdAction = "GetListAuditLogin", NamaAction = "Lihat", IdController = "AuditLogin", NoUrut = 1 },

                new() { IdAction = "GetListCabang", NamaAction = "Lihat", IdController = "MstCabang", NoUrut = 1 },
                new() { IdAction = "PostCabang", NamaAction = "Tambah", IdController = "MstCabang", NoUrut = 2 },
                new() { IdAction = "PutCabang", NamaAction = "Edit", IdController = "MstCabang", NoUrut = 3 },
                new() { IdAction = "DeleteCabang", NamaAction = "Hapus", IdController = "MstCabang", NoUrut = 4 },

                new() { IdAction = "GetListRole", NamaAction = "Lihat", IdController = "HrdGroupAkun", NoUrut = 1 },
                new() { IdAction = "PostRole", NamaAction = "Tambah", IdController = "HrdGroupAkun", NoUrut = 2 },
                new() { IdAction = "PutRole", NamaAction = "Edit", IdController = "HrdGroupAkun", NoUrut = 3 },
                new() { IdAction = "DeleteRole", NamaAction = "Hapus", IdController = "HrdGroupAkun", NoUrut = 4 },
            };

            foreach (var act in actions)
            {
                if (!await context.Actions.AnyAsync(x =>
                    x.IdAction == act.IdAction &&
                    x.IdController == act.IdController))
                {
                    context.Actions.Add(act);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
