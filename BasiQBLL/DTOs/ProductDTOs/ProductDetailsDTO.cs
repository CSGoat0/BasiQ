using BasiQDAL.Enums;

namespace BasiQBLL.DTOs.ProductDTOs
{
    public class ProductDetailsDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int MarketId { get; set; }
        public string? MarketName { get; set; }
        public string? MarketAdminUserId { get; set; }
        public bool IsMarketTrusted { get; set; }
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
        public GlobalProductSummaryDTO? GlobalProduct { get; set; }
        public List<ProductImageResponseDTO>? ProductImages { get; set; }
        public List<CategoryResponseDTO>? Categories { get; set; }
        public ProductRejectionResponseDTO? LatestRejection { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
}
