using BasiQDAL.Database;
using BasiQDAL.Entities;
using BasiQDAL.Enums;
using BasiQDAL.Extensions;
using BasiQDAL.Repositories.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace BasiQDAL.Repositories.Implementation
{
    public class ProductRepository : IProductRepository
    {
        private readonly BasiQDbContext _context;

        public ProductRepository(BasiQDbContext context)
        {
            _context = context;
        }

        // ===== CRUD Operations =====

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Market)
                .Include(p => p.GlobalProduct)
                .Include(p => p.ProductImages)
                .Include(p => p.Categories)
                .Include(p => p.Rejections)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<(IEnumerable<Product> Data, int TotalCount)> GetAllProductsAsync(int pageNumber, int pageSize, bool includeDeleted = false)
        {
            var query = _context.Products.AsQueryable();

            if (!includeDeleted)
                query = query.Where(p => !p.IsDeleted);

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await GetProductByIdAsync(id);
            if (product != null)
            {
                product.Delete();
                await _context.SaveChangesAsync();
            }
        }

        public async Task RestoreProductAsync(int id)
        {
            var product = await _context.Products
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                product.Restore();
                await _context.SaveChangesAsync();
            }
        }

        // ===== Filtering & Querying =====

        public async Task<(IEnumerable<Product> Data, int TotalCount)> GetProductsByMarketAsync(int pageNumber, int pageSize, int marketId)
        {
            var query = _context.Products
                .Where(p => p.MarketId == marketId && !p.IsDeleted)
                .Include(p => p.ProductImages)
                .Include(p => p.Categories)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Product> Data, int TotalCount)> GetProductsByStatusAsync(int pageNumber, int pageSize, ProductStatus status)
        {
            var query = _context.Products
                .Where(p => p.Status == status && !p.IsDeleted)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Product> Data, int TotalCount)> GetProductsByGlobalProductAsync(int pageNumber, int pageSize, int globalProductId)
        {
            var query = _context.Products
                .Where(p => p.GlobalProductId == globalProductId && !p.IsDeleted)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Product> Data, int TotalCount)> SearchProductsAsync(int pageNumber, int pageSize, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllProductsAsync(pageNumber, pageSize);

            var query = _context.Products
                .Where(p => !p.IsDeleted &&
                    (p.Name != null && p.Name.Contains(searchTerm) ||
                     p.Description != null && p.Description.Contains(searchTerm)))
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Product> Data, int TotalCount)> GetProductsByPriceRangeAsync(int pageNumber, int pageSize, double minPrice, double maxPrice)
        {
            var query = _context.Products
                .Where(p => !p.IsDeleted &&
                    p.BasePrice >= minPrice &&
                    p.BasePrice <= maxPrice)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Product> Data, int TotalCount)> GetAvailableProductsAsync(int pageNumber, int pageSize)
        {
            var query = _context.Products
                .Where(p => p.Status == ProductStatus.Available &&
                           p.Stock > 0 &&
                           !p.IsDeleted)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Product> Data, int TotalCount)> GetProductsOnSaleAsync(int pageNumber, int pageSize)
        {
            var query = _context.Products
                .Where(p => p.SalePrice != null &&
                           p.SalePrice < p.BasePrice &&
                           p.Status == ProductStatus.Available &&
                           p.Stock > 0 &&
                           !p.IsDeleted)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Product> Data, int TotalCount)> GetDeletedProductsAsync(int pageNumber, int pageSize)
        {
            var query = _context.Products
                .IgnoreQueryFilters()
                .Where(p => p.IsDeleted)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        // ===== Product Status Management =====

        public async Task<Product?> UpdateProductStatusAsync(int productId, ProductStatus status)
        {
            var product = await GetProductByIdAsync(productId);
            if (product != null)
            {
                product.UpdateStatus(status);
                await _context.SaveChangesAsync();
            }
            return product;
        }

        public async Task<Product?> ApproveProductAsync(int productId)
        {
            var product = await GetProductByIdAsync(productId);
            if (product != null)
            {
                product.Approve();
                await _context.SaveChangesAsync();
            }
            return product;
        }

        public async Task<Product?> RejectProductAsync(int productId)
        {
            var product = await GetProductByIdAsync(productId);
            if (product != null)
            {
                product.Reject();
                await _context.SaveChangesAsync();
            }
            return product;
        }

        public async Task<Product?> DiscontinueProductAsync(int productId)
        {
            var product = await GetProductByIdAsync(productId);
            if (product != null)
            {
                product.Discontinue();
                await _context.SaveChangesAsync();
            }
            return product;
        }

        public async Task<Product?> ReopenProductForReviewAsync(int productId)
        {
            var product = await GetProductByIdAsync(productId);
            if (product != null)
            {
                product.ReopenForReview();
                await _context.SaveChangesAsync();
            }
            return product;
        }

        // ===== Stock Management =====

        public async Task<Product?> UpdateProductStockAsync(int productId, int stock)
        {
            var product = await GetProductByIdAsync(productId);
            if (product != null)
            {
                product.EditStock(stock);
                await _context.SaveChangesAsync();
            }
            return product;
        }

        public async Task<Product?> IncrementProductStockAsync(int productId, int amount)
        {
            var product = await GetProductByIdAsync(productId);
            if (product != null && product.Stock.HasValue)
            {
                var newStock = product.Stock.Value + amount;
                product.EditStock(newStock);
                await _context.SaveChangesAsync();
            }
            return product;
        }

        public async Task<Product?> DecrementProductStockAsync(int productId, int amount)
        {
            var product = await GetProductByIdAsync(productId);
            if (product != null && product.Stock.HasValue)
            {
                var newStock = Math.Max(0, product.Stock.Value - amount);
                product.EditStock(newStock);
                await _context.SaveChangesAsync();
            }
            return product;
        }

        // ===== Global Product Linking =====

        public async Task<Product?> LinkToGlobalProductAsync(int productId, int globalProductId)
        {
            var product = await GetProductByIdAsync(productId);
            var globalProduct = await _context.GlobalProducts
                .FirstOrDefaultAsync(gp => gp.Id == globalProductId && !gp.IsDeleted);

            if (product != null && globalProduct != null)
            {
                product.EditGlobalProductId(globalProductId);
                await _context.SaveChangesAsync();
            }
            return product;
        }

        public async Task<Product?> UnlinkFromGlobalProductAsync(int productId)
        {
            var product = await GetProductByIdAsync(productId);
            if (product != null)
            {
                product.UnlinkFromGlobalProduct();
                await _context.SaveChangesAsync();
            }
            return product;
        }

        // ===== Statistics =====

        public async Task<int> GetTotalProductsCountAsync(bool includeDeleted = false)
        {
            if (!includeDeleted)
                return await _context.Products
                    .Where(p => !p.IsDeleted)
                    .CountAsync();

            return await _context.Products.CountAsync();
        }

        public async Task<int> GetProductsByMarketCountAsync(int marketId)
        {
            return await _context.Products
                .Where(p => p.MarketId == marketId && !p.IsDeleted)
                .CountAsync();
        }

        public async Task<int> GetProductsByStatusCountAsync(ProductStatus status)
        {
            return await _context.Products
                .Where(p => p.Status == status && !p.IsDeleted)
                .CountAsync();
        }

        public async Task<int> GetAvailableProductsCountAsync()
        {
            return await _context.Products
                .Where(p => p.Status == ProductStatus.Available &&
                           p.Stock > 0 &&
                           !p.IsDeleted)
                .CountAsync();
        }

        public async Task<double> GetAveragePriceAsync()
        {
            var result = await _context.Products
                .Where(p => p.BasePrice.HasValue && !p.IsDeleted)
                .AverageAsync(p => p.BasePrice ?? 0);

            return result;
        }

        public async Task<double> GetMinPriceAsync()
        {
            var result = await _context.Products
                .Where(p => p.BasePrice.HasValue && !p.IsDeleted)
                .MinAsync(p => p.BasePrice ?? 0);

            return result;
        }

        public async Task<double> GetMaxPriceAsync()
        {
            var result = await _context.Products
                .Where(p => p.BasePrice.HasValue && !p.IsDeleted)
                .MaxAsync(p => p.BasePrice ?? 0);

            return result;
        }

        public async Task<Dictionary<ProductStatus, int>> GetProductCountByStatusAsync()
        {
            var result = await _context.Products
                .Where(p => !p.IsDeleted)
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return result.ToDictionary(r => r.Status, r => r.Count);
        }

        // ===== Utility =====

        public async Task<bool> ProductExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<bool> IsProductAvailableAsync(int id)
        {
            var product = await GetProductByIdAsync(id);
            return product?.Status == ProductStatus.Available && product.Stock > 0;
        }

        public async Task<bool> IsProductOwnedByMarketAsync(int productId, int marketId)
        {
            var product = await GetProductByIdAsync(productId);
            return product?.MarketId == marketId;
        }

        public async Task<int> GetProductImageCountAsync(int productId)
        {
            return await _context.ProductImages
                .Where(pi => pi.ProductId == productId && !pi.IsDeleted)
                .CountAsync();
        }
    }
}
