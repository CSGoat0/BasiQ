using System.ComponentModel.DataAnnotations;

namespace BasiQBLL.DTOs.ProductImageDTOs
{
    public class AddProductImageDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Image URL is required.")]
        [Url(ErrorMessage = "Invalid URL format.")]
        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")]
        public string? ImageUrl { get; set; }

        public bool IsPrimary { get; set; } = false;
    }
}
