using BasiQDAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace BasiQBLL.DTOs.ProductDTOs
{
    public class UpdateProductStatusDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public ProductStatus Status { get; set; }
    }
}
