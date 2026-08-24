using BasiQDAL.Database;
using BasiQDAL.Entities;
using BasiQDAL.Extensions;
using BasiQDAL.Repositories.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace BasiQDAL.Repositories.Implementation
{
    public class GlobalProductRepository : IGlobalProductRepository
    {
        private readonly BasiQDbContext _context;

        public GlobalProductRepository(BasiQDbContext context)
        {
            _context = context;
        }

        // ===== CRUD Operations =====

        public async Task<GlobalProduct?> GetGlobalProductByIdAsync(int id)
        {
            return await _context.GlobalProducts
                .Include(gp => gp.Products)
                .ThenInclude(p => p.Market)
                .FirstOrDefaultAsync(gp => gp.Id == id);
        }

        public async Task<(IEnumerable<GlobalProduct> Data, int TotalCount)> GetAllGlobalProductsAsync(int pageNumber, int pageSize, bool includeDeleted = false)
        {
            var query = _context.GlobalProducts.AsQueryable();

            if (!includeDeleted)
                query = query.Where(gp => !gp.IsDeleted);

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<GlobalProduct> AddGlobalProductAsync(GlobalProduct globalProduct)
        {
            _context.GlobalProducts.Add(globalProduct);
            await _context.SaveChangesAsync();
            return globalProduct;
        }

        public async Task<GlobalProduct> UpdateGlobalProductAsync(GlobalProduct globalProduct)
        {
            _context.Entry(globalProduct).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return globalProduct;
        }

        public async Task DeleteGlobalProductAsync(int id)
        {
            var globalProduct = await GetGlobalProductByIdAsync(id);
            if (globalProduct != null)
            {
                globalProduct.Delete();
                await _context.SaveChangesAsync();
            }
        }

        public async Task RestoreGlobalProductAsync(int id)
        {
            var globalProduct = await _context.GlobalProducts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(gp => gp.Id == id);

            if (globalProduct != null)
            {
                globalProduct.Restore();
                await _context.SaveChangesAsync();
            }
        }

        // ===== Filtering & Querying =====

        public async Task<(IEnumerable<GlobalProduct> Data, int TotalCount)> SearchGlobalProductsAsync(int pageNumber, int pageSize, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllGlobalProductsAsync(pageNumber, pageSize);

            var query = _context.GlobalProducts
                .Where(gp => !gp.IsDeleted &&
                    (gp.Name != null && gp.Name.Contains(searchTerm) ||
                     gp.Description != null && gp.Description.Contains(searchTerm)))
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<GlobalProduct> Data, int TotalCount)> GetDeletedGlobalProductsAsync(int pageNumber, int pageSize)
        {
            var query = _context.GlobalProducts
                .IgnoreQueryFilters()
                .Where(gp => gp.IsDeleted)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<GlobalProduct?> GetGlobalProductByNameAsync(string name)
        {
            return await _context.GlobalProducts
                .FirstOrDefaultAsync(gp => gp.Name == name && !gp.IsDeleted);
        }

        // ===== Product Management =====

        public async Task<bool> LinkProductToGlobalAsync(int productId, int globalProductId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);

            var globalProduct = await GetGlobalProductByIdAsync(globalProductId);

            if (product == null || globalProduct == null)
                return false;

            product.EditGlobalProductId(globalProductId);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlinkProductFromGlobalAsync(int productId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);

            if (product == null)
                return false;

            product.UnlinkFromGlobalProduct();
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetLinkedProductsCountAsync(int globalProductId)
        {
            return await _context.Products
                .Where(p => p.GlobalProductId == globalProductId && !p.IsDeleted)
                .CountAsync();
        }

        // ===== Statistics =====

        public async Task<int> GetTotalGlobalProductsCountAsync(bool includeDeleted = false)
        {
            if (!includeDeleted)
                return await _context.GlobalProducts
                    .Where(gp => !gp.IsDeleted)
                    .CountAsync();

            return await _context.GlobalProducts.CountAsync();
        }

        public async Task<Dictionary<int, int>> GetGlobalProductProductCountsAsync()
        {
            var result = await _context.Products
                .Where(p => p.GlobalProductId.HasValue && !p.IsDeleted)
                .GroupBy(p => p.GlobalProductId.Value)
                .Select(g => new { GlobalProductId = g.Key, Count = g.Count() })
                .ToListAsync();

            return result.ToDictionary(r => r.GlobalProductId, r => r.Count);
        }

        // ===== Utility =====

        public async Task<bool> GlobalProductExistsAsync(int id)
        {
            return await _context.GlobalProducts.AnyAsync(gp => gp.Id == id && !gp.IsDeleted);
        }

        public async Task<bool> IsProductLinkedToGlobalAsync(int productId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);

            return product?.GlobalProductId.HasValue ?? false;
        }
    }
}
