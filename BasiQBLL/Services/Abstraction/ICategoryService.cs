using BasiQBLL.DTOs;
using BasiQBLL.DTOs.CategoryDTOs;
using BasiQBLL.DTOs.PaginationDTOs;

namespace BasiQBLL.Services.Abstraction
{
    public interface ICategoryService
    {
        // ===== CRUD Operations =====
        Task<ServiceResponse<CategoryResponseDTO>> GetCategoryByIdAsync(int id);
        Task<ServiceResponse<PagedResultDTO<CategoryResponseDTO>>> GetAllCategoriesAsync(PaginationParametersDTO parametersDTO, bool includeDeleted = false);
        Task<ServiceResponse<int>> CreateCategoryAsync(CreateCategoryDTO createDTO);
        Task<ServiceResponse<bool>> UpdateCategoryAsync(UpdateCategoryDTO updateDTO);
        Task<ServiceResponse<bool>> DeleteCategoryAsync(int id);
        Task<ServiceResponse<bool>> RestoreCategoryAsync(int id);

        // ===== Filtering & Querying =====
        Task<ServiceResponse<PagedResultDTO<CategoryResponseDTO>>> SearchCategoriesAsync(PaginationParametersDTO parametersDTO, string searchTerm);
        Task<ServiceResponse<PagedResultDTO<CategoryResponseDTO>>> GetDeletedCategoriesAsync(PaginationParametersDTO parametersDTO);
        Task<ServiceResponse<CategoryResponseDTO>> GetCategoryByNameAsync(string name);

        // ===== Statistics =====
        Task<ServiceResponse<int>> GetTotalCategoriesCountAsync(bool includeDeleted = false);
        Task<ServiceResponse<int>> GetProductCountForCategoryAsync(int categoryId);
        Task<ServiceResponse<Dictionary<int, int>>> GetProductCountForAllCategoriesAsync();

        // ===== Utility =====
        Task<ServiceResponse<bool>> CategoryExistsAsync(int id);
        Task<ServiceResponse<bool>> CategoryNameExistsAsync(string name);
    }
}
