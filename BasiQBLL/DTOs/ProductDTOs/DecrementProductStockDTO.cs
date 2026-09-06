using System.ComponentModel.DataAnnotations;

namespace BasiQBLL.DTOs.ProductDTOs
{
    public class DecrementProductStockDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public int Amount { get; set; }
    }
}
