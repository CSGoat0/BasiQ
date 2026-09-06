using BasiQBLL.DTOs;
using BasiQBLL.DTOs.CategoryDTOs;
using BasiQBLL.DTOs.PaginationDTOs;
using BasiQBLL.Extensions;
using BasiQBLL.Mapper;
using BasiQBLL.Services.Abstraction;
using BasiQDAL.Repositories.Abstraction;
using Microsoft.Extensions.Logging;

namespace BasiQBLL.Services.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly CategoryMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IProductRepository productRepository,
            ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _logger = logger;
            _mapper = new CategoryMapper();
        }

        // ===== CRUD Operations =====

        public async Task<ServiceResponse<CategoryResponseDTO>> GetCategoryByIdAsync(int id)
        {
            try
            {
                var category = await _categoryRepository.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return new ServiceResponse<CategoryResponseDTO>
                    {
                        Success = false,
                        Message = $"Category with ID {id} not found."
                    };
                }

                var response = _mapper.MapToResponseDTO(category);
                return new ServiceResponse<CategoryResponseDTO>
                {
                    Success = true,
                    Data = response,
                    Message = "Category retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category with ID: {CategoryId}", id);
                return new ServiceResponse<CategoryResponseDTO>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving the category: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDTO<CategoryResponseDTO>>> GetAllCategoriesAsync(PaginationParametersDTO parametersDTO, bool includeDeleted = false)
        {
            try
            {
                var categories = await _categoryRepository.GetAllCategoriesAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize,
                    includeDeleted);

                var categoryDTOs = _mapper.MapToResponseDTOs(categories.Data);
                var response = categories.ToPagedResult(categoryDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<CategoryResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = "Categories retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return new ServiceResponse<PagedResultDTO<CategoryResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving categories: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<int>> CreateCategoryAsync(CreateCategoryDTO createDTO)
        {
            try
            {
                // Check if category name already exists
                if (await _categoryRepository.CategoryNameExistsAsync(createDTO.Name!))
                {
                    return new ServiceResponse<int>
                    {
                        Success = false,
                        Message = $"Category with name '{createDTO.Name}' already exists."
                    };
                }

                var category = _mapper.MapToEntity(createDTO);
                var created = await _categoryRepository.AddCategoryAsync(category);

                _logger.LogInformation("Category created successfully with ID: {CategoryId}", created.Id);

                return new ServiceResponse<int>
                {
                    Success = true,
                    Data = created.Id,
                    Message = "Category created successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = $"An error occurred while creating the category: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> UpdateCategoryAsync(UpdateCategoryDTO updateDTO)
        {
            try
            {
                var existingCategory = await _categoryRepository.GetCategoryByIdAsync(updateDTO.Id);
                if (existingCategory == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Category with ID {updateDTO.Id} not found."
                    };
                }

                // Check if new name conflicts with existing category (excluding itself)
                if (!string.IsNullOrWhiteSpace(updateDTO.Name) &&
                    updateDTO.Name != existingCategory.Name &&
                    await _categoryRepository.CategoryNameExistsAsync(updateDTO.Name))
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Category with name '{updateDTO.Name}' already exists."
                    };
                }

                _mapper.MapToEntity(updateDTO, existingCategory);
                await _categoryRepository.UpdateCategoryAsync(existingCategory);

                _logger.LogInformation("Category updated successfully with ID: {CategoryId}", existingCategory.Id);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Category updated successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category with ID: {CategoryId}", updateDTO.Id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while updating the category: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _categoryRepository.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Category with ID {id} not found."
                    };
                }

                // Check if category has products
                var productCount = await _categoryRepository.GetProductCountForCategoryAsync(id);
                if (productCount > 0)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Cannot delete category with {productCount} products assigned. Please remove all products from this category first."
                    };
                }

                await _categoryRepository.DeleteCategoryAsync(id);
                _logger.LogInformation("Category deleted successfully with ID: {CategoryId}", id);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Category deleted successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category with ID: {CategoryId}", id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while deleting the category: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> RestoreCategoryAsync(int id)
        {
            try
            {
                await _categoryRepository.RestoreCategoryAsync(id);
                _logger.LogInformation("Category restored successfully with ID: {CategoryId}", id);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Category restored successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring category with ID: {CategoryId}", id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while restoring the category: {ex.Message}"
                };
            }
        }

        // ===== Filtering & Querying =====

        public async Task<ServiceResponse<PagedResultDTO<CategoryResponseDTO>>> SearchCategoriesAsync(PaginationParametersDTO parametersDTO, string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllCategoriesAsync(parametersDTO);
                }

                var categories = await _categoryRepository.SearchCategoriesAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize,
                    searchTerm);

                var categoryDTOs = _mapper.MapToResponseDTOs(categories.Data);
                var response = categories.ToPagedResult(categoryDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<CategoryResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = $"Search results for '{searchTerm}' retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching categories with term: {SearchTerm}", searchTerm);
                return new ServiceResponse<PagedResultDTO<CategoryResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while searching categories: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDTO<CategoryResponseDTO>>> GetDeletedCategoriesAsync(PaginationParametersDTO parametersDTO)
        {
            try
            {
                var categories = await _categoryRepository.GetDeletedCategoriesAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize);

                var categoryDTOs = _mapper.MapToResponseDTOs(categories.Data);
                var response = categories.ToPagedResult(categoryDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<CategoryResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = "Deleted categories retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deleted categories");
                return new ServiceResponse<PagedResultDTO<CategoryResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving deleted categories: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<CategoryResponseDTO>> GetCategoryByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return new ServiceResponse<CategoryResponseDTO>
                    {
                        Success = false,
                        Message = "Category name is required."
                    };
                }

                var category = await _categoryRepository.GetCategoryByNameAsync(name);
                if (category == null)
                {
                    return new ServiceResponse<CategoryResponseDTO>
                    {
                        Success = false,
                        Message = $"Category with name '{name}' not found."
                    };
                }

                var response = _mapper.MapToResponseDTO(category);
                return new ServiceResponse<CategoryResponseDTO>
                {
                    Success = true,
                    Data = response,
                    Message = "Category retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by name: {Name}", name);
                return new ServiceResponse<CategoryResponseDTO>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving the category: {ex.Message}"
                };
            }
        }

        // ===== Statistics =====

        public async Task<ServiceResponse<int>> GetTotalCategoriesCountAsync(bool includeDeleted = false)
        {
            try
            {
                var count = await _categoryRepository.GetTotalCategoriesCountAsync(includeDeleted);
                return new ServiceResponse<int>
                {
                    Success = true,
                    Data = count,
                    Message = "Total categories count retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total categories count");
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving count: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<int>> GetProductCountForCategoryAsync(int categoryId)
        {
            try
            {
                if (!await _categoryRepository.CategoryExistsAsync(categoryId))
                {
                    return new ServiceResponse<int>
                    {
                        Success = false,
                        Message = $"Category with ID {categoryId} not found."
                    };
                }

                var count = await _categoryRepository.GetProductCountForCategoryAsync(categoryId);
                return new ServiceResponse<int>
                {
                    Success = true,
                    Data = count,
                    Message = "Product count for category retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product count for category {CategoryId}", categoryId);
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving product count: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<Dictionary<int, int>>> GetProductCountForAllCategoriesAsync()
        {
            try
            {
                var allCategories = await _categoryRepository.GetAllCategoriesAsync(1, int.MaxValue);
                var result = new Dictionary<int, int>();

                foreach (var category in allCategories.Data)
                {
                    var count = await _categoryRepository.GetProductCountForCategoryAsync(category.Id);
                    result.Add(category.Id, count);
                }

                return new ServiceResponse<Dictionary<int, int>>
                {
                    Success = true,
                    Data = result,
                    Message = "Product count for all categories retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product count for all categories");
                return new ServiceResponse<Dictionary<int, int>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving product counts: {ex.Message}"
                };
            }
        }

        // ===== Utility =====

        public async Task<ServiceResponse<bool>> CategoryExistsAsync(int id)
        {
            try
            {
                var exists = await _categoryRepository.CategoryExistsAsync(id);
                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = exists,
                    Message = exists ? "Category exists." : "Category does not exist."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if category exists: {CategoryId}", id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while checking category existence: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> CategoryNameExistsAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "Category name is required."
                    };
                }

                var exists = await _categoryRepository.CategoryNameExistsAsync(name);
                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = exists,
                    Message = exists ? "Category name exists." : "Category name does not exist."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if category name exists: {Name}", name);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while checking category name: {ex.Message}"
                };
            }
        }
    }
}
