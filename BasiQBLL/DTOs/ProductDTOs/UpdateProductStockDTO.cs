using System.ComponentModel.DataAnnotations;

namespace BasiQBLL.DTOs.ProductDTOs
{
    public class UpdateProductStockDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int Stock { get; set; }
    }
}
