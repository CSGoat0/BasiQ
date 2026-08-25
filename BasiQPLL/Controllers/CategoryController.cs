using BasiQBLL.DTOs;
using BasiQBLL.DTOs.CategoryDTOs;
using BasiQBLL.DTOs.PaginationDTOs;
using BasiQBLL.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasiQPLL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(
            ICategoryService categoryService,
            ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
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
        /// Get all categories with pagination
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParametersDTO parametersDTO, [FromQuery] bool includeDeleted = false)
        {
            var result = await _categoryService.GetAllCategoriesAsync(parametersDTO, includeDeleted);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Create a new category (SuperAdmin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDTO dto)
        {
            var result = await _categoryService.CreateCategoryAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data }, result);
        }

        /// <summary>
        /// Update category (SuperAdmin only)
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDTO dto)
        {
            dto.Id = id;
            var result = await _categoryService.UpdateCategoryAsync(dto);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Delete category (soft delete - SuperAdmin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Restore deleted category (SuperAdmin only)
        /// </summary>
        [HttpPatch("{id:int}/restore")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _categoryService.RestoreCategoryAsync(id);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        // ==============================
        // Filtering & Querying
        // ==============================

        /// <summary>
        /// Search categories by name or description
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] PaginationParametersDTO parametersDTO, [FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(new { message = "Search term is required." });

            var result = await _categoryService.SearchCategoriesAsync(parametersDTO, term);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get category by name
        /// </summary>
        [HttpGet("by-name/{name}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByName(string name)
        {
            var result = await _categoryService.GetCategoryByNameAsync(name);
            return HandleResponse(result, notFoundOnFailure: true);
        }

        /// <summary>
        /// Get deleted categories (SuperAdmin only)
        /// </summary>
        [HttpGet("deleted")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetDeleted([FromQuery] PaginationParametersDTO parametersDTO)
        {
            var result = await _categoryService.GetDeletedCategoriesAsync(parametersDTO);
            return HandleResponse(result);
        }

        // ==============================
        // Statistics
        // ==============================

        /// <summary>
        /// Get total categories count
        /// </summary>
        [HttpGet("statistics/total-count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTotalCount([FromQuery] bool includeDeleted = false)
        {
            var result = await _categoryService.GetTotalCategoriesCountAsync(includeDeleted);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get product count for a category
        /// </summary>
        [HttpGet("{id:int}/product-count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductCount(int id)
        {
            var result = await _categoryService.GetProductCountForCategoryAsync(id);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get product count for all categories
        /// </summary>
        [HttpGet("statistics/product-count-all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductCountForAll()
        {
            var result = await _categoryService.GetProductCountForAllCategoriesAsync();
            return HandleResponse(result);
        }

        // ==============================
        // Utility
        // ==============================

        /// <summary>
        /// Check if category exists
        /// </summary>
        [HttpGet("{id:int}/exists")]
        [AllowAnonymous]
        public async Task<IActionResult> Exists(int id)
        {
            var result = await _categoryService.CategoryExistsAsync(id);
            return HandleResponse(result);
        }

        /// <summary>
        /// Check if category name exists
        /// </summary>
        [HttpGet("by-name/{name}/exists")]
        [AllowAnonymous]
        public async Task<IActionResult> NameExists(string name)
        {
            var result = await _categoryService.CategoryNameExistsAsync(name);
            return HandleResponse(result);
        }
    }
}
