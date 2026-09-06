using System.ComponentModel.DataAnnotations;

namespace BasiQBLL.DTOs.ProductDTOs
{
    public class UpdateProductDTO
    {
        [Required]
        public int Id { get; set; }

        [StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters.")]
        public string? Name { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Base price must be greater than 0.")]
        public double? BasePrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Sale price must be greater than or equal to 0.")]
        public double? SalePrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int? Stock { get; set; }

        public List<int>? CategoryIds { get; set; }

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
