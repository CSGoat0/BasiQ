using System.ComponentModel.DataAnnotations;

namespace BasiQBLL.DTOs.UserDTOs
{
    public class CreateUserDTO
    {
        [Required(ErrorMessage = "Username is required")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        [MaxLength(50, ErrorMessage = "Username must not exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$",
            ErrorMessage = "Username can only contain letters, numbers, and underscores")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(100, ErrorMessage = "Full name must not exceed 100 characters")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(256, ErrorMessage = "Email must not exceed 256 characters")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [MaxLength(20, ErrorMessage = "Phone number must not exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [MaxLength(100, ErrorMessage = "Password must not exceed 100 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{6,}$",
           ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, " +
                         "one number, and one special character (@$!%*?&#)")]
        public string? Password { get; set; }
        public string? ImgPath { get; set; }

        [MaxLength(200, ErrorMessage = "Address must not exceed 200 characters")]
        public string? Address { get; set; }
    }
}
