using System.ComponentModel.DataAnnotations;

namespace BasiQBLL.DTOs.ProductImageDTOs
{
    public class UpdateProductImageDTO
    {
        [Required]
        public int Id { get; set; }

        [Url(ErrorMessage = "Invalid URL format.")]
        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")]
        public string? ImageUrl { get; set; }

        public bool? IsPrimary { get; set; }
    }
}
