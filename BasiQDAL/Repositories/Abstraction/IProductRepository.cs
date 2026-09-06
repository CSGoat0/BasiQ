using BasiQDAL.Entities;
using BasiQDAL.Enums;

namespace BasiQDAL.Repositories.Abstraction
{
    public interface IProductRepository
    {
        // ===== CRUD Operations =====
        Task<Product?> GetProductByIdAsync(int id);
        Task<(IEnumerable<Product> Data, int TotalCount)> GetAllProductsAsync(int pageNumber, int pageSize, bool includeDeleted = false);
        Task<Product> AddProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
        Task RestoreProductAsync(int id);

        // ===== Filtering & Querying =====
        Task<(IEnumerable<Product> Data, int TotalCount)> GetProductsByMarketAsync(int pageNumber, int pageSize, int marketId);
        Task<(IEnumerable<Product> Data, int TotalCount)> GetProductsByStatusAsync(int pageNumber, int pageSize, ProductStatus status);
        Task<(IEnumerable<Product> Data, int TotalCount)> GetProductsByGlobalProductAsync(int pageNumber, int pageSize, int globalProductId);
        Task<(IEnumerable<Product> Data, int TotalCount)> SearchProductsAsync(int pageNumber, int pageSize, string searchTerm);
        Task<(IEnumerable<Product> Data, int TotalCount)> GetProductsByPriceRangeAsync(int pageNumber, int pageSize, double minPrice, double maxPrice);
        Task<(IEnumerable<Product> Data, int TotalCount)> GetAvailableProductsAsync(int pageNumber, int pageSize);
        Task<(IEnumerable<Product> Data, int TotalCount)> GetProductsOnSaleAsync(int pageNumber, int pageSize);
        Task<(IEnumerable<Product> Data, int TotalCount)> GetDeletedProductsAsync(int pageNumber, int pageSize);

        // Advanced filtering - takes primitive types, not DTOs
        Task<(IEnumerable<Product> Data, int TotalCount)> FilterProductsAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm,
            int? marketId,
            ProductStatus? status,
            int? globalProductId,
            double? minPrice,
            double? maxPrice,
            bool? inStock,
            bool? onSale,
            List<int>? categoryIds,
            DateTime? createdFrom,
            DateTime? createdTo,
            bool includeDeleted);

        // ===== Product Status Management =====
        Task<Product?> UpdateProductStatusAsync(int productId, ProductStatus status);
        Task<Product?> ApproveProductAsync(int productId);
        Task<Product?> RejectProductAsync(int productId);
        Task<Product?> DiscontinueProductAsync(int productId);
        Task<Product?> ReopenProductForReviewAsync(int productId);

        // ===== Stock Management =====
        Task<Product?> UpdateProductStockAsync(int productId, int stock);
        Task<Product?> IncrementProductStockAsync(int productId, int amount);
        Task<Product?> DecrementProductStockAsync(int productId, int amount);

        // ===== Global Product Linking =====
        Task<Product?> LinkToGlobalProductAsync(int productId, int globalProductId);
        Task<Product?> UnlinkFromGlobalProductAsync(int productId);

        // ===== Product Image Management =====
        Task AddProductImageAsync(ProductImage image);
        Task<ProductImage?> GetProductImageByIdAsync(int id);
        Task UpdateProductImageAsync(ProductImage image);
        Task RemoveProductImageAsync(int imageId);
        Task SetPrimaryImageAsync(int imageId);
        Task<(IEnumerable<ProductImage> Data, int TotalCount)> GetProductImagesAsync(int pageNumber, int pageSize, int productId);
        Task<int> GetProductImageCountAsync(int productId);

        // ===== Category Management (Using ProductCategory) =====
        Task AddCategoryToProductAsync(int productId, int categoryId);
        Task RemoveCategoryFromProductAsync(int productId, int categoryId);
        Task UpdateProductCategoriesAsync(int productId, List<int> categoryIds);
        Task<IEnumerable<Category>> GetCategoriesForProductAsync(int productId);

        // ===== Rejection Management =====
        Task AddRejectionToProductAsync(ProductRejection rejection);
        Task<ProductRejection?> GetLatestRejectionAsync(int productId);

        // ===== Statistics =====
        Task<int> GetTotalProductsCountAsync(bool includeDeleted = false);
        Task<int> GetProductsByMarketCountAsync(int marketId);
        Task<int> GetProductsByStatusCountAsync(ProductStatus status);
        Task<int> GetAvailableProductsCountAsync();
        Task<double> GetAveragePriceAsync();
        Task<double> GetMinPriceAsync();
        Task<double> GetMaxPriceAsync();
        Task<Dictionary<ProductStatus, int>> GetProductCountByStatusAsync();

        // ===== Utility =====
        Task<bool> ProductExistsAsync(int id);
        Task<bool> IsProductAvailableAsync(int id);
        Task<bool> IsProductOwnedByMarketAsync(int productId, int marketId);
    }
}
