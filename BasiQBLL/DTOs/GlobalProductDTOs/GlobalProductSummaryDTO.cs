namespace BasiQBLL.DTOs.GlobalProductDTOs
{
    public class GlobalProductSummaryDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public int LinkedProductsCount { get; set; }
    }
}
