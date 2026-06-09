using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using tagApiKonfigurasi.Model;
using tagApiKonfigurasi.Model.Konfigurasi;

namespace tagApiKonfigurasi.Data.Seeders
{
    public class AccesSeedItem
    {
        public string IdController { get; set; } = "";
        public string IdAction { get; set; } = "";
    }

    public static class IdentitySeeder
    {
        /// <summary>
        /// Menambah menu/aksi baru ke DB yang sudah ada + merge akses role (jalan setiap startup).
        /// </summary>
        public static async Task EnsureIncrementalSeedAsync(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            await EnsureHrdUserAkunMenuAsync(context);
            await MergeSuperAdminAccessAsync(roleManager, GetDefaultAccessList());
        }

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

            var accessList = GetDefaultAccessList();
            var accessJson = JsonConvert.SerializeObject(accessList);

            // ================= ROLE =================
            var role = await roleManager.FindByNameAsync(ROLE_NAME);
            if (role == null)
            {
                role = new ApplicationRole
                {
                    Name = ROLE_NAME,
                    Access = accessJson,
                    IdModul = "CONFIG",
                };

                await roleManager.CreateAsync(role);
            }
            else
            {
                if (string.IsNullOrEmpty(role.IdModul))
                    role.IdModul = "CONFIG";

                var merged = MergeAccessJson(role.Access, accessList);
                if (merged != role.Access)
                {
                    role.Access = merged;
                    await roleManager.UpdateAsync(role);
                }
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
                    NikSistag = ADMIN_USERNAME,
                    Active = true,
                    Cabang = CABANG_ID
                };

                await userManager.CreateAsync(user, "P@ssw0rd");
            }
            else if (string.IsNullOrWhiteSpace(user.NikSistag))
            {
                user.NikSistag = ADMIN_USERNAME;
                await userManager.UpdateAsync(user);
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

                new() { IdController = "HrdUserAkun", NamaController = "User Akun", IdMenu = "PengaturanHrd", Url = "/hrd/user-akun", Icon = "IconUsers", NoUrut = 2 },
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

                new() { IdAction = "GetListAkun", NamaAction = "Lihat", IdController = "HrdUserAkun", NoUrut = 1 },
                new() { IdAction = "PostAkun", NamaAction = "Tambah", IdController = "HrdUserAkun", NoUrut = 2 },
                new() { IdAction = "PutAkun", NamaAction = "Edit", IdController = "HrdUserAkun", NoUrut = 3 },
                new() { IdAction = "DeleteAkun", NamaAction = "Hapus", IdController = "HrdUserAkun", NoUrut = 4 },
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

            await EnsureHrdUserAkunMenuAsync(context);
        }

        private static AccesSeedItem[] GetDefaultAccessList() =>
        [
            new AccesSeedItem { IdController = "Beranda", IdAction = "Read" },
            new AccesSeedItem { IdController = "Beranda", IdAction = "ProgresPemutahiran" },
            new AccesSeedItem { IdController = "Role", IdAction = "GetListRole" },
            new AccesSeedItem { IdController = "Role", IdAction = "PostRole" },
            new AccesSeedItem { IdController = "Role", IdAction = "PutRole" },
            new AccesSeedItem { IdController = "Role", IdAction = "DeleteRole" },
            new AccesSeedItem { IdController = "Akun", IdAction = "GetListAkun" },
            new AccesSeedItem { IdController = "Akun", IdAction = "PostAkun" },
            new AccesSeedItem { IdController = "Akun", IdAction = "PutAkun" },
            new AccesSeedItem { IdController = "Akun", IdAction = "DeleteAkun" },
            new AccesSeedItem { IdController = "AuditLogin", IdAction = "GetListAuditLogin" },
            new AccesSeedItem { IdController = "HrdGroupAkun", IdAction = "GetListRole" },
            new AccesSeedItem { IdController = "HrdGroupAkun", IdAction = "PostRole" },
            new AccesSeedItem { IdController = "HrdGroupAkun", IdAction = "PutRole" },
            new AccesSeedItem { IdController = "HrdGroupAkun", IdAction = "DeleteRole" },
            new AccesSeedItem { IdController = "HrdUserAkun", IdAction = "GetListAkun" },
            new AccesSeedItem { IdController = "HrdUserAkun", IdAction = "PostAkun" },
            new AccesSeedItem { IdController = "HrdUserAkun", IdAction = "PutAkun" },
            new AccesSeedItem { IdController = "HrdUserAkun", IdAction = "DeleteAkun" },
        ];

        private static AccesSeedItem[] GetHrdUserAkunAccessList() =>
        [
            new AccesSeedItem { IdController = "HrdUserAkun", IdAction = "GetListAkun" },
            new AccesSeedItem { IdController = "HrdUserAkun", IdAction = "PostAkun" },
            new AccesSeedItem { IdController = "HrdUserAkun", IdAction = "PutAkun" },
            new AccesSeedItem { IdController = "HrdUserAkun", IdAction = "DeleteAkun" },
        ];

        private static async Task EnsureHrdUserAkunMenuAsync(ApplicationDbContext context)
        {
            if (!await context.Set<MstModul>().AnyAsync(x => x.IdModul == "HRD"))
            {
                context.Add(
                    new MstModul { IdModul = "HRD", KodeModul = "HRD", NamaModul = "Human Resource", NoUrut = 2 }
                );
                await context.SaveChangesAsync();
            }

            if (!await context.Menus.AnyAsync(x => x.IdMenu == "PengaturanHrd"))
            {
                context.Menus.Add(new MstMenu
                {
                    IdMenu = "PengaturanHrd",
                    NamaMenu = "Pengaturan",
                    IdModul = "HRD",
                    Icon = "IconSettings",
                    NoUrut = 99,
                });
                await context.SaveChangesAsync();
            }

            if (!await context.Controllers.AnyAsync(x => x.IdController == "HrdUserAkun"))
            {
                context.Controllers.Add(new MstController
                {
                    IdController = "HrdUserAkun",
                    NamaController = "User Akun",
                    IdMenu = "PengaturanHrd",
                    Url = "/hrd/user-akun",
                    Icon = "IconUsers",
                    NoUrut = 2,
                });
                await context.SaveChangesAsync();
            }
            else
            {
                var hrdUserAkun = await context.Controllers
                    .FirstAsync(x => x.IdController == "HrdUserAkun");
                if (hrdUserAkun.Url != "/hrd/user-akun")
                {
                    hrdUserAkun.Url = "/hrd/user-akun";
                    hrdUserAkun.IdMenu = "PengaturanHrd";
                    hrdUserAkun.NamaController = "User Akun";
                    await context.SaveChangesAsync();
                }
            }

            var hrdUserAkunActions = new List<MstAction>
            {
                new() { IdAction = "GetListAkun", NamaAction = "Lihat", IdController = "HrdUserAkun", NoUrut = 1 },
                new() { IdAction = "PostAkun", NamaAction = "Tambah", IdController = "HrdUserAkun", NoUrut = 2 },
                new() { IdAction = "PutAkun", NamaAction = "Edit", IdController = "HrdUserAkun", NoUrut = 3 },
                new() { IdAction = "DeleteAkun", NamaAction = "Hapus", IdController = "HrdUserAkun", NoUrut = 4 },
            };

            foreach (var act in hrdUserAkunActions)
            {
                if (!await context.Actions.AnyAsync(x =>
                    x.IdAction == act.IdAction && x.IdController == act.IdController))
                {
                    context.Actions.Add(act);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task MergeSuperAdminAccessAsync(
            RoleManager<ApplicationRole> roleManager,
            AccesSeedItem[] requiredAccess)
        {
            const string roleName = "SuperAdmin";
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null) return;

            var merged = MergeAccessJson(role.Access, requiredAccess);
            if (merged == role.Access) return;

            role.Access = merged;
            await roleManager.UpdateAsync(role);
        }

        /// <summary>
        /// Role yang sudah punya HrdGroupAkun otomatis dapat HrdUserAkun (satu paket Pengaturan HRD).
        /// </summary>
        public static async Task MergeHrdUserAkunAccessForHrdRolesAsync(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var hrdUserAkunAccess = GetHrdUserAkunAccessList();

            foreach (var role in roleManager.Roles.ToList())
            {
                if (string.IsNullOrEmpty(role.Access)) continue;

                List<AccesSeedItem>? current;
                try
                {
                    current = JsonConvert.DeserializeObject<List<AccesSeedItem>>(role.Access);
                }
                catch
                {
                    continue;
                }

                if (current == null) continue;

                var hasHrdGroup = current.Any(a =>
                    string.Equals(a.IdController, "HrdGroupAkun", StringComparison.OrdinalIgnoreCase));

                if (!hasHrdGroup) continue;

                var merged = MergeAccessJson(role.Access, hrdUserAkunAccess);
                if (merged == role.Access) continue;

                role.Access = merged;
                await roleManager.UpdateAsync(role);
            }
        }

        private static string MergeAccessJson(string? existingJson, AccesSeedItem[] toAdd)
        {
            var list = new List<AccesSeedItem>();

            if (!string.IsNullOrWhiteSpace(existingJson))
            {
                try
                {
                    var parsed = JsonConvert.DeserializeObject<List<AccesSeedItem>>(existingJson);
                    if (parsed != null)
                        list.AddRange(parsed);
                }
                catch
                {
                    // ignore corrupt json, rebuild from additions only below
                }
            }

            foreach (var item in toAdd)
            {
                if (!list.Any(x =>
                    string.Equals(x.IdController, item.IdController, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.IdAction, item.IdAction, StringComparison.OrdinalIgnoreCase)))
                {
                    list.Add(item);
                }
            }

            return JsonConvert.SerializeObject(list);
        }
    }
}
