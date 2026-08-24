using BasiQBLL.DTOs.CategoryDTOs;
using BasiQDAL.Entities;

namespace BasiQBLL.Mapper
{
    public class CategoryMapper
    {
        // ===== Response Mappings =====

        public CategoryResponseDTO MapToResponseDTO(Category category)
        {
            if (category == null) return null!;

            return new CategoryResponseDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ProductCount = category.Products?.Count(p => !p.IsDeleted) ?? 0,
                IsDeleted = category.IsDeleted,
                RegistrationDate = category.RegistrationDate,
                UpdatedOn = category.UpdatedOn,
                DeletedOn = category.DeletedOn
            };
        }

        public IEnumerable<CategoryResponseDTO> MapToResponseDTOs(IEnumerable<Category> categories)
        {
            var result = new List<CategoryResponseDTO>();
            foreach (var category in categories)
            {
                result.Add(MapToResponseDTO(category));
            }
            return result;
        }

        // ===== Create Mappings =====

        public Category MapToEntity(CreateCategoryDTO dto)
        {
            if (dto == null) return null!;

            return new Category(
                name: dto.Name,
                description: dto.Description
            );
        }

        // ===== Update Mappings =====

        public Category MapToEntity(UpdateCategoryDTO dto, Category existingCategory)
        {
            if (dto == null || existingCategory == null) return existingCategory!;

            if (!string.IsNullOrWhiteSpace(dto.Name))
                existingCategory.EditName(dto.Name);

            if (!string.IsNullOrWhiteSpace(dto.Description))
                existingCategory.EditDescription(dto.Description);

            return existingCategory;
        }
    }
}
