using BasiQDAL.Entities;
using BasiQDAL.Enums;

namespace BasiQDAL.Repositories.Abstraction
{
    public interface IMarketRepository
    {
        // ===== CRUD Operations =====
        Task<Market?> GetMarketByIdAsync(int id);
        Task<(IEnumerable<Market> Data, int TotalCount)> GetAllMarketsAsync(int pageNumber, int pageSize, bool includeDeleted = false);
        Task<Market> AddMarketAsync(Market market);
        Task<Market> UpdateMarketAsync(Market market);
        Task DeleteMarketAsync(int id);
        Task RestoreMarketAsync(int id);

        // ===== Filtering & Querying =====
        Task<(IEnumerable<Market> Data, int TotalCount)> GetMarketsByAdminAsync(int pageNumber, int pageSize, string adminUserId);
        Task<(IEnumerable<Market> Data, int TotalCount)> GetMarketsByStatusAsync(int pageNumber, int pageSize, MarketStatus status);
        Task<(IEnumerable<Market> Data, int TotalCount)> SearchMarketsAsync(int pageNumber, int pageSize, string searchTerm);
        Task<(IEnumerable<Market> Data, int TotalCount)> GetTrustedMarketsAsync(int pageNumber, int pageSize);
        Task<(IEnumerable<Market> Data, int TotalCount)> GetDeletedMarketsAsync(int pageNumber, int pageSize);

        // ===== Statistics =====
        Task<int> GetTotalMarketsCountAsync(bool includeDeleted = false);
        Task<int> GetActiveMarketsCountAsync();
        Task<int> GetMarketsByAdminCountAsync(string adminUserId);
        Task<Dictionary<MarketStatus, int>> GetMarketCountByStatusAsync();

        // ===== Utility =====
        Task<bool> MarketExistsAsync(int id);
        Task<bool> IsMarketActiveAsync(int id);
        Task<bool> IsMarketOwnedByUserAsync(int marketId, string userId);
        Task<int> GetMarketProductCountAsync(int marketId);
        Task<bool> CanMarketAddMoreProductsAsync(int marketId);
        Task<int> GetRemainingProductSlotsAsync(int marketId);
    }
}
