using BasiQDAL.Database;
using BasiQDAL.Entities;
using BasiQDAL.Extensions;
using BasiQDAL.Repositories.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace BasiQDAL.Repositories.Implementation
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly BasiQDbContext _context;

        public CategoryRepository(BasiQDbContext context)
        {
            _context = context;
        }

        // ===== CRUD Operations =====

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<(IEnumerable<Category> Data, int TotalCount)> GetAllCategoriesAsync(int pageNumber, int pageSize, bool includeDeleted = false)
        {
            var query = _context.Categories.AsQueryable();

            if (!includeDeleted)
                query = query.Where(c => !c.IsDeleted);

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<Category> AddCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await GetCategoryByIdAsync(id);
            if (category != null)
            {
                category.Delete();
                await _context.SaveChangesAsync();
            }
        }

        public async Task RestoreCategoryAsync(int id)
        {
            var category = await _context.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category != null)
            {
                category.Restore();
                await _context.SaveChangesAsync();
            }
        }

        // ===== Filtering & Querying =====

        public async Task<(IEnumerable<Category> Data, int TotalCount)> SearchCategoriesAsync(int pageNumber, int pageSize, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllCategoriesAsync(pageNumber, pageSize);

            var query = _context.Categories
                .Where(c => !c.IsDeleted &&
                    (c.Name != null && c.Name.Contains(searchTerm) ||
                     c.Description != null && c.Description.Contains(searchTerm)))
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<Category> Data, int TotalCount)> GetDeletedCategoriesAsync(int pageNumber, int pageSize)
        {
            var query = _context.Categories
                .IgnoreQueryFilters()
                .Where(c => c.IsDeleted)
                .AsQueryable();

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<Category?> GetCategoryByNameAsync(string name)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == name && !c.IsDeleted);
        }

        // ===== Statistics =====

        public async Task<int> GetTotalCategoriesCountAsync(bool includeDeleted = false)
        {
            if (!includeDeleted)
                return await _context.Categories
                    .Where(c => !c.IsDeleted)
                    .CountAsync();

            return await _context.Categories.CountAsync();
        }

        public async Task<int> GetProductCountForCategoryAsync(int categoryId)
        {
            var category = await GetCategoryByIdAsync(categoryId);
            return category?.Products?.Count(p => !p.IsDeleted) ?? 0;
        }

        // ===== Utility =====

        public async Task<bool> CategoryExistsAsync(int id)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<bool> CategoryNameExistsAsync(string name)
        {
            return await _context.Categories
                .AnyAsync(c => c.Name == name && !c.IsDeleted);
        }
    }
}
