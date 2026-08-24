using System.ComponentModel.DataAnnotations;

namespace BasiQBLL.DTOs.ProductImageDTOs
{
    public class ReorderProductImagesDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public List<int> ImageIdsInOrder { get; set; } = new List<int>();
    }
}
