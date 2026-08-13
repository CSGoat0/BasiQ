using System.ComponentModel.DataAnnotations;

namespace BasiQBLL.DTOs.UserDTOs
{
    public class ChangePasswordDTO
    {
        public string? CurrentPassword { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [MaxLength(100, ErrorMessage = "Password must not exceed 100 characters")]
        [RegularExpression(@"^[A-Za-z\d!@#$%^&*()_+\-=\[\]{};:'"",.<>?/\\|`~]+$",
            ErrorMessage = "Password can only contain English letters, digits, and special characters")]
        public string? NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }
    }
}
