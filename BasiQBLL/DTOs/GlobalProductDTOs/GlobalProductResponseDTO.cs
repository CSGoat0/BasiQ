namespace BasiQBLL.DTOs.GlobalProductDTOs
{
    public class GlobalProductResponseDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public int LinkedProductsCount { get; set; }
        public List<LinkedProductSummaryDTO>? LinkedProducts { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
}
