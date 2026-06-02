namespace tagApiKonfigurasi.Services.Role
{
    public interface IRoleAccessMergeService
    {
        Task<HashSet<string>> GetControllerIdsForModulAsync(string modul);

        Task<string> MergeModulAccessAsync(
            string? existingAccessJson,
            string? newModulAccessJson,
            string modul);
    }
}
