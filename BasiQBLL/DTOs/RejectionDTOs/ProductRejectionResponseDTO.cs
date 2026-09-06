namespace BasiQBLL.DTOs.RejectionDTOs
{
    public class ProductRejectionResponseDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? Reason { get; set; }
        public string? RejectedByUserId { get; set; }
        public string? RejectedByUserName { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
