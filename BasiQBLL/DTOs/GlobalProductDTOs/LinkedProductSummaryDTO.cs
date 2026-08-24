namespace BasiQBLL.DTOs.GlobalProductDTOs
{
    public class LinkedProductSummaryDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int MarketId { get; set; }
        public string? MarketName { get; set; }
        public double? CurrentPrice { get; set; }
        public bool IsInStock { get; set; }
        public string? PrimaryImageUrl { get; set; }
    }
}
