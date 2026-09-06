using BasiQBLL.DTOs.MarketDTOs;
using BasiQDAL.Entities;
using BasiQDAL.Enums;

namespace BasiQBLL.Mapper
{
    public class MarketMapper
    {
        // ===== Response Mappings =====

        public MarketResponseDTO MapToResponseDTO(Market market)
        {
            if (market == null) return null!;

            return new MarketResponseDTO
            {
                Id = market.Id,
                Name = market.Name,
                Description = market.Description,
                AdminUserId = market.AdminUserId,
                AdminUserName = market.AdminUser?.FullName ?? market.AdminUser?.UserName ?? string.Empty,
                AdminUserEmail = market.AdminUser?.Email ?? string.Empty,
                IsTrusted = market.IsTrusted,
                Status = market.Status,
                MaxProducts = market.MaxProducts,
                ProductCount = market.Products?.Count(p => !p.IsDeleted) ?? 0,
                RemainingProductSlots = Math.Max(0, market.MaxProducts - (market.Products?.Count(p => !p.IsDeleted) ?? 0)),
                IsDeleted = market.IsDeleted,
                RegistrationDate = market.RegistrationDate,
                UpdatedOn = market.UpdatedOn,
                DeletedOn = market.DeletedOn
            };
        }

        public IEnumerable<MarketResponseDTO> MapToResponseDTOs(IEnumerable<Market> markets)
        {
            var result = new List<MarketResponseDTO>();
            foreach (var market in markets)
            {
                result.Add(MapToResponseDTO(market));
            }
            return result;
        }

        // ===== Create Mappings =====

        public Market MapToEntity(CreateMarketDTO dto)
        {
            if (dto == null) return null!;

            return new Market(
                name: dto.Name,
                description: dto.Description,
                adminUserId: dto.AdminUserId,
                isTrusted: dto.IsTrusted
            );
        }

        // ===== Update Mappings =====

        public Market MapToEntity(UpdateMarketDTO dto, Market existingMarket)
        {
            if (dto == null || existingMarket == null) return existingMarket!;

            if (!string.IsNullOrWhiteSpace(dto.Name))
                existingMarket.EditName(dto.Name);

            if (!string.IsNullOrWhiteSpace(dto.Description))
                existingMarket.EditDescription(dto.Description);

            if (!string.IsNullOrWhiteSpace(dto.AdminUserId))
                existingMarket.EditAdmin(dto.AdminUserId);

            if (dto.IsTrusted.HasValue)
                existingMarket.SetTrusted(dto.IsTrusted.Value);

            if (dto.MaxProducts.HasValue)
                existingMarket.SetMaxProducts(dto.MaxProducts.Value);

            return existingMarket;
        }

        // ===== Statistics Mappings =====

        public MarketStatisticsDTO MapToStatisticsDTO(
            int totalMarkets,
            int activeMarkets,
            int suspendedMarkets,
            int closedMarkets,
            int trustedMarkets,
            int untrustedMarkets,
            int totalProducts,
            int totalAvailableProducts,
            Dictionary<MarketStatus, int> countByStatus)
        {
            return new MarketStatisticsDTO
            {
                TotalMarkets = totalMarkets,
                ActiveMarkets = activeMarkets,
                SuspendedMarkets = suspendedMarkets,
                ClosedMarkets = closedMarkets,
                TrustedMarkets = trustedMarkets,
                UntrustedMarkets = untrustedMarkets,
                TotalProductsAcrossAllMarkets = totalProducts,
                TotalAvailableProducts = totalAvailableProducts,
                CountByStatus = countByStatus,
                StatisticsDate = DateTime.UtcNow
            };
        }
    }
}
