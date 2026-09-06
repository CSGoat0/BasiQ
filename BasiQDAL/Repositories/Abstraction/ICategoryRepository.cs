using BasiQDAL.Entities;

namespace BasiQDAL.Repositories.Abstraction
{
    public interface ICategoryRepository
    {
        // ===== CRUD Operations =====
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<(IEnumerable<Category> Data, int TotalCount)> GetAllCategoriesAsync(int pageNumber, int pageSize, bool includeDeleted = false);
        Task<Category> AddCategoryAsync(Category category);
        Task<Category> UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(int id);
        Task RestoreCategoryAsync(int id);

        // ===== Filtering & Querying =====
        Task<(IEnumerable<Category> Data, int TotalCount)> SearchCategoriesAsync(int pageNumber, int pageSize, string searchTerm);
        Task<(IEnumerable<Category> Data, int TotalCount)> GetDeletedCategoriesAsync(int pageNumber, int pageSize);
        Task<Category?> GetCategoryByNameAsync(string name);

        // ===== Statistics =====
        Task<int> GetTotalCategoriesCountAsync(bool includeDeleted = false);
        Task<int> GetProductCountForCategoryAsync(int categoryId);

        // ===== Utility =====
        Task<bool> CategoryExistsAsync(int id);
        Task<bool> CategoryNameExistsAsync(string name);
    }
}
