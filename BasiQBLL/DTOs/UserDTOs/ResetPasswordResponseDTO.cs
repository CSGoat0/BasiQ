namespace BasiQBLL.DTOs.UserDTOs
{
    public class ResetPasswordResponseDTO
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Token { get; set; }
    }
}
