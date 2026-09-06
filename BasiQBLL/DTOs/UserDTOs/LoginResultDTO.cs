namespace BasiQBLL.DTOs.UserDTOs
{
    public class LoginResultDTO
    {
        public string Token { get; set; } = string.Empty;
        public UserResponseDTO User { get; set; } = null!;
    }
}
