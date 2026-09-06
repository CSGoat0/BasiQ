using BasiQDAL.Entities;

namespace BasiQDAL.Repositories.Abstraction
{
    public interface IGlobalProductRepository
    {
        // ===== CRUD Operations =====
        Task<GlobalProduct?> GetGlobalProductByIdAsync(int id);
        Task<(IEnumerable<GlobalProduct> Data, int TotalCount)> GetAllGlobalProductsAsync(int pageNumber, int pageSize, bool includeDeleted = false);
        Task<GlobalProduct> AddGlobalProductAsync(GlobalProduct globalProduct);
        Task<GlobalProduct> UpdateGlobalProductAsync(GlobalProduct globalProduct);
        Task DeleteGlobalProductAsync(int id);
        Task RestoreGlobalProductAsync(int id);

        // ===== Filtering & Querying =====
        Task<(IEnumerable<GlobalProduct> Data, int TotalCount)> SearchGlobalProductsAsync(int pageNumber, int pageSize, string searchTerm);
        Task<(IEnumerable<GlobalProduct> Data, int TotalCount)> GetDeletedGlobalProductsAsync(int pageNumber, int pageSize);
        Task<GlobalProduct?> GetGlobalProductByNameAsync(string name);

        // ===== Product Management =====
        Task<bool> LinkProductToGlobalAsync(int productId, int globalProductId);
        Task<bool> UnlinkProductFromGlobalAsync(int productId);
        Task<int> GetLinkedProductsCountAsync(int globalProductId);

        // ===== Statistics =====
        Task<int> GetTotalGlobalProductsCountAsync(bool includeDeleted = false);
        Task<Dictionary<int, int>> GetGlobalProductProductCountsAsync();

        // ===== Utility =====
        Task<bool> GlobalProductExistsAsync(int id);
        Task<bool> IsProductLinkedToGlobalAsync(int productId);
    }
}
