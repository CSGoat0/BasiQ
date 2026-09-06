using BasiQBLL.DTOs;
using BasiQBLL.DTOs.GlobalProductDTOs;
using BasiQBLL.DTOs.PaginationDTOs;
using BasiQBLL.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasiQPLL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GlobalProductController : ControllerBase
    {
        private readonly IGlobalProductService _globalProductService;
        private readonly ILogger<GlobalProductController> _logger;

        public GlobalProductController(
            IGlobalProductService globalProductService,
            ILogger<GlobalProductController> logger)
        {
            _globalProductService = globalProductService;
            _logger = logger;
        }

        // ==============================
        // Helper Methods
        // ==============================

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
        /// Get all global products with pagination
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParametersDTO parametersDTO, [FromQuery] bool includeDeleted = false)
        {
            var result = await _globalProductService.GetAllGlobalProductsAsync(parametersDTO, includeDeleted);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get global product by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _globalProductService.GetGlobalProductByIdAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Create a new global product (SuperAdmin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateGlobalProductDTO dto)
        {
            var result = await _globalProductService.CreateGlobalProductAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data }, result);
        }

        /// <summary>
        /// Update global product (SuperAdmin only)
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateGlobalProductDTO dto)
        {
            dto.Id = id;
            var result = await _globalProductService.UpdateGlobalProductAsync(dto);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Delete global product (SuperAdmin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _globalProductService.DeleteGlobalProductAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Restore deleted global product (SuperAdmin only)
        /// </summary>
        [HttpPatch("{id:int}/restore")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _globalProductService.RestoreGlobalProductAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        // ==============================
        // Filtering & Querying
        // ==============================

        /// <summary>
        /// Search global products by name or description
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] PaginationParametersDTO parametersDTO, [FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(new { message = "Search term is required." });

            var result = await _globalProductService.SearchGlobalProductsAsync(parametersDTO, term);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get global product by name
        /// </summary>
        [HttpGet("by-name/{name}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByName(string name)
        {
            var result = await _globalProductService.GetGlobalProductByNameAsync(name);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Get deleted global products (SuperAdmin only)
        /// </summary>
        [HttpGet("deleted")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetDeleted([FromQuery] PaginationParametersDTO parametersDTO)
        {
            var result = await _globalProductService.GetDeletedGlobalProductsAsync(parametersDTO);
            return HandleResponse(result);
        }

        // ==============================
        // Product Management
        // ==============================

        /// <summary>
        /// Link product to global product (SuperAdmin only)
        /// </summary>
        [HttpPost("link-product")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> LinkProduct([FromBody] LinkProductToGlobalDTO dto)
        {
            var result = await _globalProductService.LinkProductToGlobalAsync(dto.ProductId, dto.GlobalProductId);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Unlink product from global product (SuperAdmin only)
        /// </summary>
        [HttpDelete("unlink-product/{productId:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UnlinkProduct(int productId)
        {
            var result = await _globalProductService.UnlinkProductFromGlobalAsync(productId);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Get linked products count for a global product
        /// </summary>
        [HttpGet("{id:int}/linked-count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLinkedCount(int id)
        {
            var result = await _globalProductService.GetLinkedProductsCountAsync(id);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get linked products for a global product
        /// </summary>
        [HttpGet("{id:int}/linked-products")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLinkedProducts([FromQuery] PaginationParametersDTO parametersDTO, int id)
        {
            var result = await _globalProductService.GetLinkedProductsAsync(parametersDTO, id);
            return HandleResponse(result);
        }

        // ==============================
        // Statistics
        // ==============================

        /// <summary>
        /// Get total global products count
        /// </summary>
        [HttpGet("statistics/total-count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTotalCount([FromQuery] bool includeDeleted = false)
        {
            var result = await _globalProductService.GetTotalGlobalProductsCountAsync(includeDeleted);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get complete global product statistics dashboard (SuperAdmin only)
        /// </summary>
        [HttpGet("statistics/dashboard")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetStatistics()
        {
            var result = await _globalProductService.GetGlobalProductStatisticsAsync();
            return HandleResponse(result);
        }

        // ==============================
        // Utility
        // ==============================

        /// <summary>
        /// Check if global product exists
        /// </summary>
        [HttpGet("{id:int}/exists")]
        [AllowAnonymous]
        public async Task<IActionResult> Exists(int id)
        {
            var result = await _globalProductService.GlobalProductExistsAsync(id);
            return HandleResponse(result);
        }

        /// <summary>
        /// Check if product is linked to a global product
        /// </summary>
        [HttpGet("is-product-linked/{productId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> IsProductLinked(int productId)
        {
            var result = await _globalProductService.IsProductLinkedToGlobalAsync(productId);
            return HandleResponse(result);
        }
    }
}
