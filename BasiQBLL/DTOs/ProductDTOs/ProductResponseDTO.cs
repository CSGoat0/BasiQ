using BasiQDAL.Enums;

namespace BasiQBLL.DTOs.ProductDTOs
{
    public class ProductResponseDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int MarketId { get; set; }
        public string? MarketName { get; set; }
        public double? BasePrice { get; set; }
        public double? SalePrice { get; set; }
        public double? CurrentPrice { get; set; }
        public bool IsOnSale { get; set; }
        public double? DiscountPercentage { get; set; }
        public int? Stock { get; set; }
        public bool IsInStock { get; set; }
        public ProductStatus Status { get; set; }
        public string? StatusDisplay { get; set; }
        public int? GlobalProductId { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public int ImageCount { get; set; }
        public List<string>? CategoryNames { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
}
