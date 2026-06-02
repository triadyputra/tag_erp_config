using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using tagApiKonfigurasi.Data;
using tagApiKonfigurasi.Model.DTO;

namespace tagApiKonfigurasi.Services.Role
{
    public class RoleAccessMergeService : IRoleAccessMergeService
    {
        private readonly ApplicationDbContext _context;

        public RoleAccessMergeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HashSet<string>> GetControllerIdsForModulAsync(string modul)
        {
            var ids = await _context.Controllers
                .Where(c => c.Menu != null && c.Menu.IdModul == modul)
                .Select(c => c.IdController)
                .ToListAsync();

            return ids.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public async Task<string> MergeModulAccessAsync(
            string? existingAccessJson,
            string? newModulAccessJson,
            string modul)
        {
            var modulControllerIds = await GetControllerIdsForModulAsync(modul);

            var existing = ParseAccess(existingAccessJson);
            var incoming = ParseAccess(newModulAccessJson);

            var nonModul = existing
                .Where(a => !modulControllerIds.Contains(a.IdController))
                .ToList();

            var modulIncoming = incoming
                .Where(a => modulControllerIds.Contains(a.IdController))
                .ToList();

            var merged = nonModul
                .Concat(modulIncoming)
                .GroupBy(a => new { a.IdController, a.IdAction })
                .Select(g => g.First())
                .ToList();

            return JsonConvert.SerializeObject(merged);
        }

        private static List<AccesModel> ParseAccess(string? accessJson)
        {
            if (string.IsNullOrWhiteSpace(accessJson))
                return new List<AccesModel>();

            try
            {
                return JsonConvert.DeserializeObject<List<AccesModel>>(accessJson)
                    ?? new List<AccesModel>();
            }
            catch
            {
                return new List<AccesModel>();
            }
        }
    }
}
