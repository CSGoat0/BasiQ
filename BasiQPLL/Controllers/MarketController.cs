using BasiQBLL.DTOs;
using BasiQBLL.DTOs.MarketDTOs;
using BasiQBLL.DTOs.PaginationDTOs;
using BasiQBLL.Services.Abstraction;
using BasiQDAL.Entities;
using BasiQDAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BasiQPLL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MarketController : ControllerBase
    {
        private readonly IMarketService _marketService;
        private readonly ILogger<MarketController> _logger;

        public MarketController(
            IMarketService marketService,
            ILogger<MarketController> logger)
        {
            _marketService = marketService;
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
        /// Get all markets with pagination
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParametersDTO parametersDTO, [FromQuery] bool includeDeleted = false)
        {
            var result = await _marketService.GetAllMarketsAsync(parametersDTO, includeDeleted);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get market by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _marketService.GetMarketByIdAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Create a new market (SuperAdmin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateMarketDTO dto)
        {
            var result = await _marketService.CreateMarketAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data }, result);
        }

        /// <summary>
        /// Update market
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMarketDTO dto)
        {
            dto.Id = id;
            var result = await _marketService.UpdateMarketAsync(dto);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Update market status (SuperAdmin only)
        /// </summary>
        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] MarketStatus status)
        {
            var result = await _marketService.UpdateMarketStatusAsync(id, status);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Delete market (soft delete - SuperAdmin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _marketService.DeleteMarketAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Restore deleted market (SuperAdmin only)
        /// </summary>
        [HttpPatch("{id:int}/restore")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _marketService.RestoreMarketAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        // ==============================
        // Filtering & Querying
        // ==============================

        /// <summary>
        /// Get markets by admin user ID
        /// </summary>
        [HttpGet("by-admin/{adminUserId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetByAdmin([FromQuery] PaginationParametersDTO parametersDTO, string adminUserId)
        {
            var result = await _marketService.GetMarketsByAdminAsync(parametersDTO, adminUserId);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get markets by status
        /// </summary>
        [HttpGet("by-status")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByStatus([FromQuery] PaginationParametersDTO parametersDTO, [FromQuery] MarketStatus status)
        {
            var result = await _marketService.GetMarketsByStatusAsync(parametersDTO, status);
            return HandleResponse(result);
        }

        /// <summary>
        /// Search markets by name or description
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] PaginationParametersDTO parametersDTO, [FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(new { message = "Search term is required." });

            var result = await _marketService.SearchMarketsAsync(parametersDTO, term);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get trusted markets
        /// </summary>
        [HttpGet("trusted")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTrusted([FromQuery] PaginationParametersDTO parametersDTO)
        {
            var result = await _marketService.GetTrustedMarketsAsync(parametersDTO);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get deleted markets (SuperAdmin only)
        /// </summary>
        [HttpGet("deleted")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetDeleted([FromQuery] PaginationParametersDTO parametersDTO)
        {
            var result = await _marketService.GetDeletedMarketsAsync(parametersDTO);
            return HandleResponse(result);
        }

        // ==============================
        // Trust Management
        // ==============================

        /// <summary>
        /// Set market trusted status (SuperAdmin only)
        /// </summary>
        [HttpPatch("{id:int}/trust")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> SetTrusted(int id, [FromQuery] bool isTrusted)
        {
            var result = await _marketService.SetMarketTrustedAsync(id, isTrusted);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        // ==============================
        // Statistics
        // ==============================

        /// <summary>
        /// Get total markets count
        /// </summary>
        [HttpGet("statistics/total-count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTotalCount([FromQuery] bool includeDeleted = false)
        {
            var result = await _marketService.GetTotalMarketsCountAsync(includeDeleted);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get active markets count
        /// </summary>
        [HttpGet("statistics/active-count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveCount()
        {
            var result = await _marketService.GetActiveMarketsCountAsync();
            return HandleResponse(result);
        }

        /// <summary>
        /// Get markets count by admin
        /// </summary>
        [HttpGet("statistics/by-admin/{adminUserId}/count")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetByAdminCount(string adminUserId)
        {
            var result = await _marketService.GetMarketsByAdminCountAsync(adminUserId);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get market count by status
        /// </summary>
        [HttpGet("statistics/count-by-status")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCountByStatus()
        {
            var result = await _marketService.GetMarketCountByStatusAsync();
            return HandleResponse(result);
        }

        /// <summary>
        /// Get complete market statistics dashboard
        /// </summary>
        [HttpGet("statistics/dashboard")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetStatistics()
        {
            var result = await _marketService.GetMarketStatisticsAsync();
            return HandleResponse(result);
        }

        // ==============================
        // Product Limit Management
        // ==============================

        /// <summary>
        /// Get product count for a market
        /// </summary>
        [HttpGet("{id:int}/product-count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductCount(int id)
        {
            var result = await _marketService.GetMarketProductCountAsync(id);
            return HandleResponse(result);
        }

        /// <summary>
        /// Check if market can add more products
        /// </summary>
        [HttpGet("{id:int}/can-add-products")]
        [AllowAnonymous]
        public async Task<IActionResult> CanAddMoreProducts(int id)
        {
            var result = await _marketService.CanMarketAddMoreProductsAsync(id);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get remaining product slots for a market
        /// </summary>
        [HttpGet("{id:int}/remaining-slots")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRemainingSlots(int id)
        {
            var result = await _marketService.GetRemainingProductSlotsAsync(id);
            return HandleResponse(result);
        }

        // ==============================
        // Utility
        // ==============================

        /// <summary>
        /// Check if market exists
        /// </summary>
        [HttpGet("{id:int}/exists")]
        [AllowAnonymous]
        public async Task<IActionResult> Exists(int id)
        {
            var result = await _marketService.MarketExistsAsync(id);
            return HandleResponse(result);
        }

        /// <summary>
        /// Check if market is active
        /// </summary>
        [HttpGet("{id:int}/is-active")]
        [AllowAnonymous]
        public async Task<IActionResult> IsActive(int id)
        {
            var result = await _marketService.IsMarketActiveAsync(id);
            return HandleResponse(result);
        }

        /// <summary>
        /// Check if user owns the market
        /// </summary>
        [HttpGet("{id:int}/is-owned-by-user")]
        [Authorize]
        public async Task<IActionResult> IsOwnedByUser(int id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _marketService.IsMarketOwnedByUserAsync(id, userId);
            return HandleResponse(result);
        }

        // ==============================
        // Options
        // ==============================

        /// <summary>
        /// Get available market status options
        /// </summary>
        [HttpGet("options/statuses")]
        [AllowAnonymous]
        public IActionResult GetStatusOptions()
        {
            var values = Enum.GetValues(typeof(MarketStatus))
                             .Cast<MarketStatus>()
                             .Select(e => new
                             {
                                 Id = (int)e,
                                 Name = e.ToString()
                             });

            return Ok(values);
        }
    }
}
