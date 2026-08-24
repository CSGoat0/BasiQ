using System.ComponentModel.DataAnnotations;

namespace BasiQBLL.DTOs.RejectionDTOs
{
    public class RejectProductDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Rejection reason is required.")]
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "Rejection reason must be between 5 and 1000 characters.")]
        public string? Reason { get; set; }

        public string? RejectedByUserId { get; set; }
    }
}
