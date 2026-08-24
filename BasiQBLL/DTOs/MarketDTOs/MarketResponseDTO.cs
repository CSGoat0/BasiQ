using BasiQDAL.Enums;

namespace BasiQBLL.DTOs.MarketDTOs
{
    public class MarketResponseDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? AdminUserId { get; set; }
        public string? AdminUserName { get; set; }
        public string? AdminUserEmail { get; set; }
        public bool IsTrusted { get; set; }
        public MarketStatus Status { get; set; }
        public int MaxProducts { get; set; }
        public int ProductCount { get; set; }
        public int RemainingProductSlots { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
}
