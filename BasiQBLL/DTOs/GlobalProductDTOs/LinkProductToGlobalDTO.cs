using System.ComponentModel.DataAnnotations;

namespace BasiQBLL.DTOs.GlobalProductDTOs
{
    public class LinkProductToGlobalDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int GlobalProductId { get; set; }
    }
}
