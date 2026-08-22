using System.ComponentModel.DataAnnotations;

namespace BasiQBLL.DTOs.UserDTOs
{
    public class UpdateUserDTO
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? ImgPath { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [MaxLength(20, ErrorMessage = "Phone number must not exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        [MaxLength(200, ErrorMessage = "Address must not exceed 200 characters")]
        public string? Address { get; set; }
    }
}
