using System.ComponentModel.DataAnnotations;

namespace BasiQBLL.DTOs.GlobalProductDTOs
{
    public class UpdateGlobalProductDTO
    {
        [Required]
        public int Id { get; set; }

        [StringLength(200, MinimumLength = 2, ErrorMessage = "Global product name must be between 2 and 200 characters.")]
        public string? Name { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string? Description { get; set; }

        [Url(ErrorMessage = "Invalid URL format.")]
        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")]
        public string? PrimaryImageUrl { get; set; }
    }
}
