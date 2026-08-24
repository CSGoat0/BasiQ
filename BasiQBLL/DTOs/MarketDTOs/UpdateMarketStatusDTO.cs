using BasiQDAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace BasiQBLL.DTOs.MarketDTOs
{
    public class UpdateMarketStatusDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public MarketStatus Status { get; set; }
    }
}
