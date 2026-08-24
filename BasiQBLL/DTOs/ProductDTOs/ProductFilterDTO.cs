using BasiQDAL.Enums;

namespace BasiQBLL.DTOs.ProductDTOs
{
    public class ProductFilterDTO
    {
        public string? SearchTerm { get; set; }
        public int? MarketId { get; set; }
        public ProductStatus? Status { get; set; }
        public int? GlobalProductId { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public bool? InStock { get; set; }
        public bool? OnSale { get; set; }
        public List<int>? CategoryIds { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}
