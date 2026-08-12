namespace BasicBLL.DTOs.UserDTOs
{
    public class LoginResponseDTO
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
