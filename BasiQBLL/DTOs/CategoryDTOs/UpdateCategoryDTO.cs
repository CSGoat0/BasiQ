using System.ComponentModel.DataAnnotations;

namespace BasiQBLL.DTOs.CategoryDTOs
{
    public class UpdateCategoryDTO
    {
        [Required]
        public int Id { get; set; }

        [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters.")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
    }
}
