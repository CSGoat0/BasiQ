using BasiQBLL.DTOs;
using BasiQBLL.DTOs.PaginationDTOs;
using BasiQBLL.DTOs.ProductDTOs;
using BasiQBLL.DTOs.ProductImageDTOs;
using BasiQBLL.DTOs.RejectionDTOs;
using BasiQBLL.Services.Abstraction;
using BasiQDAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BasiQPLL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
            ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // ==============================
        // Helper Methods
        // ==============================

        private string? GetCurrentUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier);

        private IActionResult HandleResponse<T>(
            ServiceResponse<T> response,
            bool notFoundOnFailure = false)
        {
            if (!response.Success)
            {
                if (notFoundOnFailure)
                    return NotFound(response);

                return BadRequest(response);
            }
            return Ok(response);
        }

        // ==============================
        // CRUD Operations
        // ==============================

        /// <summary>
        /// Get all products with pagination
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParametersDTO parametersDTO, [FromQuery] bool includeDeleted = false)
        {
            var result = await _productService.GetAllProductsAsync(parametersDTO, includeDeleted);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Get product details with images, categories, and rejection info
        /// </summary>
        [HttpGet("{id:int}/details")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDetails(int id)
        {
            var result = await _productService.GetProductDetailsByIdAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin, MarketAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateProductDTO dto)
        {
            var result = await _productService.CreateProductAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data }, result);
        }

        /// <summary>
        /// Update product
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdmin, MarketAdmin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDTO dto)
        {
            dto.Id = id;
            var result = await _productService.UpdateProductAsync(dto);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Delete product (soft delete)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperAdmin, MarketAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Restore deleted product (SuperAdmin only)
        /// </summary>
        [HttpPatch("{id:int}/restore")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _productService.RestoreProductAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        // ==============================
        // Filtering & Querying
        // ==============================

        /// <summary>
        /// Get products by market
        /// </summary>
        [HttpGet("by-market/{marketId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByMarket([FromQuery] PaginationParametersDTO parametersDTO, int marketId)
        {
            var result = await _productService.GetProductsByMarketAsync(parametersDTO, marketId);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get products by status
        /// </summary>
        [HttpGet("by-status")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByStatus([FromQuery] PaginationParametersDTO parametersDTO, [FromQuery] ProductStatus status)
        {
            var result = await _productService.GetProductsByStatusAsync(parametersDTO, status);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get products by global product
        /// </summary>
        [HttpGet("by-global-product/{globalProductId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByGlobalProduct([FromQuery] PaginationParametersDTO parametersDTO, int globalProductId)
        {
            var result = await _productService.GetProductsByGlobalProductAsync(parametersDTO, globalProductId);
            return HandleResponse(result);
        }

        /// <summary>
        /// Search products by name or description
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] PaginationParametersDTO parametersDTO, [FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(new { message = "Search term is required." });

            var result = await _productService.SearchProductsAsync(parametersDTO, term);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get products by price range
        /// </summary>
        [HttpGet("by-price-range")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByPriceRange(
            [FromQuery] PaginationParametersDTO parametersDTO,
            [FromQuery] double minPrice,
            [FromQuery] double maxPrice)
        {
            var result = await _productService.GetProductsByPriceRangeAsync(parametersDTO, minPrice, maxPrice);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get available products (in stock)
        /// </summary>
        [HttpGet("available")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailable([FromQuery] PaginationParametersDTO parametersDTO)
        {
            var result = await _productService.GetAvailableProductsAsync(parametersDTO);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get products on sale
        /// </summary>
        [HttpGet("on-sale")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOnSale([FromQuery] PaginationParametersDTO parametersDTO)
        {
            var result = await _productService.GetProductsOnSaleAsync(parametersDTO);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get deleted products (SuperAdmin only)
        /// </summary>
        [HttpGet("deleted")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetDeleted([FromQuery] PaginationParametersDTO parametersDTO)
        {
            var result = await _productService.GetDeletedProductsAsync(parametersDTO);
            return HandleResponse(result);
        }

        /// <summary>
        /// Filter products with advanced criteria
        /// </summary>
        [HttpPost("filter")]
        [AllowAnonymous]
        public async Task<IActionResult> Filter([FromQuery] PaginationParametersDTO parametersDTO, [FromBody] ProductFilterDTO filterDTO)
        {
            var result = await _productService.FilterProductsAsync(parametersDTO, filterDTO);
            return HandleResponse(result);
        }

        // ==============================
        // Product Status Management
        // ==============================

        /// <summary>
        /// Update product status (SuperAdmin only)
        /// </summary>
        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] ProductStatus status)
        {
            var result = await _productService.UpdateProductStatusAsync(id, status);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Approve product (SuperAdmin only)
        /// </summary>
        [HttpPatch("{id:int}/approve")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _productService.ApproveProductAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Reject product with reason (SuperAdmin only)
        /// </summary>
        [HttpPost("{id:int}/reject")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectProductDTO dto)
        {
            dto.ProductId = id;
            var result = await _productService.RejectProductAsync(dto);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Discontinue product (MarketAdmin or SuperAdmin)
        /// </summary>
        [HttpPatch("{id:int}/discontinue")]
        [Authorize(Roles = "SuperAdmin, MarketAdmin")]
        public async Task<IActionResult> Discontinue(int id)
        {
            var result = await _productService.DiscontinueProductAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Reopen product for review (SuperAdmin only)
        /// </summary>
        [HttpPatch("{id:int}/reopen")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ReopenForReview(int id)
        {
            var result = await _productService.ReopenProductForReviewAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        // ==============================
        // Stock Management
        // ==============================

        /// <summary>
        /// Update product stock
        /// </summary>
        [HttpPatch("{id:int}/stock")]
        [Authorize(Roles = "SuperAdmin, MarketAdmin")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] int stock)
        {
            var result = await _productService.UpdateProductStockAsync(id, stock);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Increment product stock
        /// </summary>
        [HttpPatch("{id:int}/stock/increment")]
        [Authorize(Roles = "SuperAdmin, MarketAdmin")]
        public async Task<IActionResult> IncrementStock(int id, [FromQuery] int amount)
        {
            var result = await _productService.IncrementProductStockAsync(id, amount);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Decrement product stock
        /// </summary>
        [HttpPatch("{id:int}/stock/decrement")]
        [Authorize(Roles = "SuperAdmin, MarketAdmin")]
        public async Task<IActionResult> DecrementStock(int id, [FromQuery] int amount)
        {
            var result = await _productService.DecrementProductStockAsync(id, amount);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        // ==============================
        // Product Image Management
        // ==============================

        /// <summary>
        /// Add image to product
        /// </summary>
        [HttpPost("{id:int}/images")]
        [Authorize(Roles = "SuperAdmin, MarketAdmin")]
        public async Task<IActionResult> AddImage(int id, [FromBody] AddProductImageDTO dto)
        {
            dto.ProductId = id;
            var result = await _productService.AddProductImageAsync(dto);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Update product image
        /// </summary>
        [HttpPut("images/{imageId:int}")]
        [Authorize(Roles = "SuperAdmin, MarketAdmin")]
        public async Task<IActionResult> UpdateImage(int imageId, [FromBody] UpdateProductImageDTO dto)
        {
            dto.Id = imageId;
            var result = await _productService.UpdateProductImageAsync(dto);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Remove product image
        /// </summary>
        [HttpDelete("images/{imageId:int}")]
        [Authorize(Roles = "SuperAdmin, MarketAdmin")]
        public async Task<IActionResult> RemoveImage(int imageId)
        {
            var result = await _productService.RemoveProductImageAsync(imageId);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Set primary image
        /// </summary>
        [HttpPatch("images/{imageId:int}/set-primary")]
        [Authorize(Roles = "SuperAdmin, MarketAdmin")]
        public async Task<IActionResult> SetPrimaryImage(int imageId)
        {
            var result = await _productService.SetPrimaryImageAsync(imageId);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Get product images
        /// </summary>
        [HttpGet("{id:int}/images")]
        [AllowAnonymous]
        public async Task<IActionResult> GetImages([FromQuery] PaginationParametersDTO parametersDTO, int id)
        {
            var result = await _productService.GetProductImagesAsync(parametersDTO, id);
            return HandleResponse(result);
        }

        // ==============================
        // Category Management
        // ==============================

        /// <summary>
        /// Add category to product
        /// </summary>
        [HttpPost("{id:int}/categories/{categoryId:int}")]
        [Authorize(Roles = "SuperAdmin, MarketAdmin")]
        public async Task<IActionResult> AddCategory(int id, int categoryId)
        {
            var result = await _productService.AddCategoryToProductAsync(id, categoryId);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Remove category from product
        /// </summary>
        [HttpDelete("{id:int}/categories/{categoryId:int}")]
        [Authorize(Roles = "SuperAdmin, MarketAdmin")]
        public async Task<IActionResult> RemoveCategory(int id, int categoryId)
        {
            var result = await _productService.RemoveCategoryFromProductAsync(id, categoryId);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Update product categories
        /// </summary>
        [HttpPut("{id:int}/categories")]
        [Authorize(Roles = "SuperAdmin, MarketAdmin")]
        public async Task<IActionResult> UpdateCategories(int id, [FromBody] List<int> categoryIds)
        {
            var result = await _productService.UpdateProductCategoriesAsync(id, categoryIds);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        // ==============================
        // Global Product Management
        // ==============================

        /// <summary>
        /// Link product to global product (SuperAdmin only)
        /// </summary>
        [HttpPatch("{id:int}/link-to-global/{globalProductId:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> LinkToGlobal(int id, int globalProductId)
        {
            var result = await _productService.LinkProductToGlobalAsync(id, globalProductId);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Unlink product from global product (SuperAdmin only)
        /// </summary>
        [HttpPatch("{id:int}/unlink-from-global")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UnlinkFromGlobal(int id)
        {
            var result = await _productService.UnlinkProductFromGlobalAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        // ==============================
        // Statistics
        // ==============================

        /// <summary>
        /// Get total products count
        /// </summary>
        [HttpGet("statistics/total-count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTotalCount([FromQuery] bool includeDeleted = false)
        {
            var result = await _productService.GetTotalProductsCountAsync(includeDeleted);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get products count by market
        /// </summary>
        [HttpGet("statistics/by-market/{marketId:int}/count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByMarketCount(int marketId)
        {
            var result = await _productService.GetProductsByMarketCountAsync(marketId);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get products count by status
        /// </summary>
        [HttpGet("statistics/by-status/{status}/count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByStatusCount(ProductStatus status)
        {
            var result = await _productService.GetProductsByStatusCountAsync(status);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get available products count
        /// </summary>
        [HttpGet("statistics/available-count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableCount()
        {
            var result = await _productService.GetAvailableProductsCountAsync();
            return HandleResponse(result);
        }

        /// <summary>
        /// Get average price
        /// </summary>
        [HttpGet("statistics/average-price")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAveragePrice()
        {
            var result = await _productService.GetAveragePriceAsync();
            return HandleResponse(result);
        }

        /// <summary>
        /// Get min price
        /// </summary>
        [HttpGet("statistics/min-price")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMinPrice()
        {
            var result = await _productService.GetMinPriceAsync();
            return HandleResponse(result);
        }

        /// <summary>
        /// Get max price
        /// </summary>
        [HttpGet("statistics/max-price")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMaxPrice()
        {
            var result = await _productService.GetMaxPriceAsync();
            return HandleResponse(result);
        }

        /// <summary>
        /// Get product count by status
        /// </summary>
        [HttpGet("statistics/count-by-status")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCountByStatus()
        {
            var result = await _productService.GetProductCountByStatusAsync();
            return HandleResponse(result);
        }

        // ==============================
        // Utility
        // ==============================

        /// <summary>
        /// Check if product exists
        /// </summary>
        [HttpGet("{id:int}/exists")]
        [AllowAnonymous]
        public async Task<IActionResult> Exists(int id)
        {
            var result = await _productService.ProductExistsAsync(id);
            return HandleResponse(result);
        }

        /// <summary>
        /// Check if product is available
        /// </summary>
        [HttpGet("{id:int}/is-available")]
        [AllowAnonymous]
        public async Task<IActionResult> IsAvailable(int id)
        {
            var result = await _productService.IsProductAvailableAsync(id);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get latest product rejection (SuperAdmin only)
        /// </summary>
        [HttpGet("{id:int}/rejection")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetLatestRejection(int id)
        {
            var result = await _productService.GetLatestProductRejectionAsync(id);
            return HandleResponse(result);
        }

        // ==============================
        // Options
        // ==============================

        /// <summary>
        /// Get available product status options
        /// </summary>
        [HttpGet("options/statuses")]
        [AllowAnonymous]
        public IActionResult GetStatusOptions()
        {
            var values = Enum.GetValues(typeof(ProductStatus))
                             .Cast<ProductStatus>()
                             .Select(e => new
                             {
                                 Id = (int)e,
                                 Name = e.ToString()
                             });

            return Ok(values);
        }
    }
}
