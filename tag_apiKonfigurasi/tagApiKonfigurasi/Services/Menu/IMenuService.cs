

using tagApiKonfigurasi.Model.DTO;

namespace tagApiKonfigurasi.Services.Menu
{
    public interface IMenuService
    {
        Task<List<MenuViewModel>> GetMenuAsync(string? modul);
    }
}
