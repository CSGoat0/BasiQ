using System.ComponentModel.DataAnnotations;

namespace BasiQBLL.DTOs.MarketDTOs
{
    public class UpdateMarketDTO
    {
        [Required]
        public int Id { get; set; }

        [StringLength(200, MinimumLength = 2, ErrorMessage = "Market name must be between 2 and 200 characters.")]
        public string? Name { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        public string? AdminUserId { get; set; }

        public bool? IsTrusted { get; set; }

        [Range(1, 1000, ErrorMessage = "Max products must be between 1 and 1000.")]
        public int? MaxProducts { get; set; }
    }
}
