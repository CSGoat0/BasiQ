namespace BasiQBLL.DTOs.GlobalProductDTOs
{
    public class GlobalProductStatisticsDTO
    {
        public int TotalGlobalProducts { get; set; }
        public int TotalLinkedProducts { get; set; }
        public int UnlinkedProducts { get; set; }
        public int GlobalProductsWithNoLinks { get; set; }
        public double AverageProductsPerGlobalProduct { get; set; }
        public GlobalProductResponseDTO? MostLinkedGlobalProduct { get; set; }
        public DateTime StatisticsDate { get; set; }
    }
}
