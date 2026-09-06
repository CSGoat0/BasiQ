using BasiQDAL.Enums;

namespace BasiQBLL.DTOs.MarketDTOs
{
    public class MarketStatisticsDTO
    {
        public int TotalMarkets { get; set; }
        public int ActiveMarkets { get; set; }
        public int SuspendedMarkets { get; set; }
        public int ClosedMarkets { get; set; }
        public int TrustedMarkets { get; set; }
        public int UntrustedMarkets { get; set; }
        public int TotalProductsAcrossAllMarkets { get; set; }
        public int TotalAvailableProducts { get; set; }
        public Dictionary<MarketStatus, int>? CountByStatus { get; set; }
        public DateTime StatisticsDate { get; set; }
    }
}
