namespace BasiQBLL.DTOs.ProductImageDTOs
{
    public class ProductImageResponseDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
