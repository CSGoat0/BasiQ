using System.ComponentModel.DataAnnotations;

namespace BasiQBLL.DTOs.ProductDTOs
{
    public class CreateProductDTO
    {
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters.")]
        public string? Name { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string? Description { get; set; }

        [Required]
        public int MarketId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Base price must be greater than 0.")]
        public double? BasePrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Sale price must be greater than or equal to 0.")]
        public double? SalePrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int? Stock { get; set; } = 0;

        public List<string>? ImageUrls { get; set; } = new List<string>();

        public List<int>? CategoryIds { get; set; } = new List<int>();
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SalePrice.HasValue && BasePrice.HasValue && SalePrice.Value > BasePrice.Value)
            {
                yield return new ValidationResult(
                    "Sale price cannot be greater than base price.",
                    new[] { nameof(SalePrice) }
                );
            }
        }
    }
}
