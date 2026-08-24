using BasiQDAL.Database;
using BasiQDAL.Entities;
using BasiQDAL.Enums;
using BasiQDAL.Extensions;
using BasiQDAL.Repositories.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace BasiQDAL.Repositories.Implementation
{
    public class MarketRepository : IMarketRepository
    {
        private readonly BasiQDbContext _context;

        public MarketRepository(BasiQDbContext context)
        {
            _context = context;
        }

        // ===== CRUD Operations =====

        public async Task<Market?> GetMarketByIdAsync(int id)
        {
            return await _context.Markets
                .Include(m => m.AdminUser)
                .Include(m => m.Products)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<(IEnumerable<Market> Data, int TotalCount)> GetAllMarketsAsync(int pageNumber, int pageSize, bool includeDeleted = false)
        {
            var query = _context.Markets.AsQueryable();

            if (!includeDeleted)
                query = query.Where(m => !m.IsDeleted);

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<Market> AddMarketAsync(Market market)
        {
            _context.Markets.Add(market);
            await _context.SaveChangesAsync();
            return market;
        }

        public async Task<Market> UpdateMarketAsync(Market market)
        {
            _context.Entry(market).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return market;
        }

        public async Task DeleteMarketAsync(int id)
        {
            var market = await GetMarketByIdAsync(id);
            if (market != null)
            {
                market.Delete();
                await _context.SaveChangesAsync();
            }
        }

        public async Task RestoreMarketAsync(int id)
        {
            var market = await _context.Markets
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (market != null)
            {
                market.Restore();
                await _context.SaveChangesAsync();
            }
        }

        // ===== Filtering & Querying =====

        public async Task<(IEnumerable<Market> Data, int TotalCount)> GetMarketsByAdminAsync(int pageNumber, int pageSize, string adminUserId)
        {
            var query = _context.Markets
                .Where(m => m.AdminUserId == adminUserId && !m.IsDeleted)
                .Include(m => m.AdminUser)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Market> Data, int TotalCount)> GetMarketsByStatusAsync(int pageNumber, int pageSize, MarketStatus status)
        {
            var query = _context.Markets
                .Where(m => m.Status == status && !m.IsDeleted)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Market> Data, int TotalCount)> SearchMarketsAsync(int pageNumber, int pageSize, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllMarketsAsync(pageNumber, pageSize);

            var query = _context.Markets
                .Where(m => !m.IsDeleted &&
                    (m.Name != null && m.Name.Contains(searchTerm) ||
                     m.Description != null && m.Description.Contains(searchTerm)))
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Market> Data, int TotalCount)> GetTrustedMarketsAsync(int pageNumber, int pageSize)
        {
            var query = _context.Markets
                .Where(m => m.IsTrusted && !m.IsDeleted)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Market> Data, int TotalCount)> GetDeletedMarketsAsync(int pageNumber, int pageSize)
        {
            var query = _context.Markets
                .IgnoreQueryFilters()
                .Where(m => m.IsDeleted)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        // ===== Statistics =====

        public async Task<int> GetTotalMarketsCountAsync(bool includeDeleted = false)
        {
            if (!includeDeleted)
                return await _context.Markets
                    .Where(m => !m.IsDeleted)
                    .CountAsync();

            return await _context.Markets.CountAsync();
        }

        public async Task<int> GetActiveMarketsCountAsync()
        {
            return await _context.Markets
                .Where(m => m.Status == MarketStatus.Active && !m.IsDeleted)
                .CountAsync();
        }

        public async Task<int> GetMarketsByAdminCountAsync(string adminUserId)
        {
            return await _context.Markets
                .Where(m => m.AdminUserId == adminUserId && !m.IsDeleted)
                .CountAsync();
        }

        public async Task<Dictionary<MarketStatus, int>> GetMarketCountByStatusAsync()
        {
            var result = await _context.Markets
                .Where(m => !m.IsDeleted)
                .GroupBy(m => m.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return result.ToDictionary(r => r.Status, r => r.Count);
        }

        // ===== Utility =====

        public async Task<bool> MarketExistsAsync(int id)
        {
            return await _context.Markets.AnyAsync(m => m.Id == id && !m.IsDeleted);
        }

        public async Task<bool> IsMarketActiveAsync(int id)
        {
            var market = await GetMarketByIdAsync(id);
            return market?.Status == MarketStatus.Active;
        }

        public async Task<bool> IsMarketOwnedByUserAsync(int marketId, string userId)
        {
            var market = await GetMarketByIdAsync(marketId);
            return market?.AdminUserId == userId;
        }

        public async Task<int> GetMarketProductCountAsync(int marketId)
        {
            return await _context.Products
                .Where(p => p.MarketId == marketId && !p.IsDeleted)
                .CountAsync();
        }

        public async Task<bool> CanMarketAddMoreProductsAsync(int marketId)
        {
            var market = await GetMarketByIdAsync(marketId);
            if (market == null) return false;

            var productCount = await GetMarketProductCountAsync(marketId);
            return productCount < market.MaxProducts;
        }

        public async Task<int> GetRemainingProductSlotsAsync(int marketId)
        {
            var market = await GetMarketByIdAsync(marketId);
            if (market == null) return 0;

            var productCount = await GetMarketProductCountAsync(marketId);
            return Math.Max(0, market.MaxProducts - productCount);
        }
    }
}
