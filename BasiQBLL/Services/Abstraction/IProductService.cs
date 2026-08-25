using BasiQBLL.DTOs;
using BasiQBLL.DTOs.PaginationDTOs;
using BasiQBLL.DTOs.ProductDTOs;
using BasiQBLL.DTOs.ProductImageDTOs;
using BasiQBLL.DTOs.RejectionDTOs;
using BasiQDAL.Enums;

namespace BasiQBLL.Services.Abstraction
{
    public interface IProductService
    {
        // ===== CRUD Operations =====
        Task<ServiceResponse<ProductResponseDTO>> GetProductByIdAsync(int id);
        Task<ServiceResponse<ProductDetailsDTO>> GetProductDetailsByIdAsync(int id);
        Task<ServiceResponse<PagedResultDTO<ProductResponseDTO>>> GetAllProductsAsync(PaginationParametersDTO parametersDTO, bool includeDeleted = false);
        Task<ServiceResponse<int>> CreateProductAsync(CreateProductDTO createDTO);
        Task<ServiceResponse<bool>> UpdateProductAsync(UpdateProductDTO updateDTO);
        Task<ServiceResponse<bool>> DeleteProductAsync(int id);
        Task<ServiceResponse<bool>> RestoreProductAsync(int id);

        // ===== Filtering & Querying =====
        Task<ServiceResponse<PagedResultDTO<ProductResponseDTO>>> GetProductsByMarketAsync(PaginationParametersDTO parametersDTO, int marketId);
        Task<ServiceResponse<PagedResultDTO<ProductResponseDTO>>> GetProductsByStatusAsync(PaginationParametersDTO parametersDTO, ProductStatus status);
        Task<ServiceResponse<PagedResultDTO<ProductResponseDTO>>> GetProductsByGlobalProductAsync(PaginationParametersDTO parametersDTO, int globalProductId);
        Task<ServiceResponse<PagedResultDTO<ProductResponseDTO>>> SearchProductsAsync(PaginationParametersDTO parametersDTO, string searchTerm);
        Task<ServiceResponse<PagedResultDTO<ProductResponseDTO>>> GetProductsByPriceRangeAsync(PaginationParametersDTO parametersDTO, double minPrice, double maxPrice);
        Task<ServiceResponse<PagedResultDTO<ProductResponseDTO>>> GetAvailableProductsAsync(PaginationParametersDTO parametersDTO);
        Task<ServiceResponse<PagedResultDTO<ProductResponseDTO>>> GetProductsOnSaleAsync(PaginationParametersDTO parametersDTO);
        Task<ServiceResponse<PagedResultDTO<ProductResponseDTO>>> GetDeletedProductsAsync(PaginationParametersDTO parametersDTO);
        Task<ServiceResponse<PagedResultDTO<ProductResponseDTO>>> FilterProductsAsync(PaginationParametersDTO parametersDTO, ProductFilterDTO filterDTO);

        // ===== Product Status Management =====
        Task<ServiceResponse<bool>> UpdateProductStatusAsync(int productId, ProductStatus status);
        Task<ServiceResponse<bool>> ApproveProductAsync(int productId);
        Task<ServiceResponse<bool>> RejectProductAsync(RejectProductDTO rejectDTO);
        Task<ServiceResponse<bool>> DiscontinueProductAsync(int productId);
        Task<ServiceResponse<bool>> ReopenProductForReviewAsync(int productId);

        // ===== Stock Management =====
        Task<ServiceResponse<bool>> UpdateProductStockAsync(int productId, int stock);
        Task<ServiceResponse<bool>> IncrementProductStockAsync(int productId, int amount);
        Task<ServiceResponse<bool>> DecrementProductStockAsync(int productId, int amount);

        // ===== Product Image Management =====
        Task<ServiceResponse<bool>> AddProductImageAsync(AddProductImageDTO addImageDTO);
        Task<ServiceResponse<bool>> UpdateProductImageAsync(UpdateProductImageDTO updateImageDTO);
        Task<ServiceResponse<bool>> RemoveProductImageAsync(int imageId);
        Task<ServiceResponse<bool>> SetPrimaryImageAsync(int imageId);
        Task<ServiceResponse<PagedResultDTO<ProductImageResponseDTO>>> GetProductImagesAsync(PaginationParametersDTO parametersDTO, int productId);

        // ===== Category Management =====
        Task<ServiceResponse<bool>> AddCategoryToProductAsync(int productId, int categoryId);
        Task<ServiceResponse<bool>> RemoveCategoryFromProductAsync(int productId, int categoryId);
        Task<ServiceResponse<bool>> UpdateProductCategoriesAsync(int productId, List<int> categoryIds);

        // ===== Global Product Management =====
        Task<ServiceResponse<bool>> LinkProductToGlobalAsync(int productId, int globalProductId);
        Task<ServiceResponse<bool>> UnlinkProductFromGlobalAsync(int productId);

        // ===== Statistics =====
        Task<ServiceResponse<int>> GetTotalProductsCountAsync(bool includeDeleted = false);
        Task<ServiceResponse<int>> GetProductsByMarketCountAsync(int marketId);
        Task<ServiceResponse<int>> GetProductsByStatusCountAsync(ProductStatus status);
        Task<ServiceResponse<int>> GetAvailableProductsCountAsync();
        Task<ServiceResponse<double>> GetAveragePriceAsync();
        Task<ServiceResponse<double>> GetMinPriceAsync();
        Task<ServiceResponse<double>> GetMaxPriceAsync();
        Task<ServiceResponse<Dictionary<ProductStatus, int>>> GetProductCountByStatusAsync();

        // ===== Utility =====
        Task<ServiceResponse<bool>> ProductExistsAsync(int id);
        Task<ServiceResponse<bool>> IsProductAvailableAsync(int id);
        Task<ServiceResponse<bool>> IsProductOwnedByMarketAsync(int productId, int marketId);
        Task<ServiceResponse<int>> GetProductImageCountAsync(int productId);
        Task<ServiceResponse<ProductRejectionResponseDTO>> GetLatestProductRejectionAsync(int productId);
    }
}
