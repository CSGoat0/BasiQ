using System.ComponentModel.DataAnnotations;

namespace BasiQBLL.DTOs.UserDTOs
{
    public class ResetPasswordResponseDTO
    {
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [MaxLength(100, ErrorMessage = "Password must not exceed 100 characters")]
        public string? Password { get; set; }
        public string? Token { get; set; }
    }
}
