using BasiQBLL.DTOs;
using BasiQBLL.DTOs.MarketDTOs;
using BasiQBLL.DTOs.PaginationDTOs;
using BasiQDAL.Enums;

namespace BasiQBLL.Services.Abstraction
{
    public interface IMarketService
    {
        // ===== CRUD Operations =====
        Task<ServiceResponse<MarketResponseDTO>> GetMarketByIdAsync(int id);
        Task<ServiceResponse<PagedResultDTO<MarketResponseDTO>>> GetAllMarketsAsync(PaginationParametersDTO parametersDTO, bool includeDeleted = false);
        Task<ServiceResponse<int>> CreateMarketAsync(CreateMarketDTO createDTO);
        Task<ServiceResponse<bool>> UpdateMarketAsync(UpdateMarketDTO updateDTO);
        Task<ServiceResponse<bool>> UpdateMarketStatusAsync(int marketId, MarketStatus status);
        Task<ServiceResponse<bool>> DeleteMarketAsync(int id);
        Task<ServiceResponse<bool>> RestoreMarketAsync(int id);

        // ===== Filtering & Querying =====
        Task<ServiceResponse<PagedResultDTO<MarketResponseDTO>>> GetMarketsByAdminAsync(PaginationParametersDTO parametersDTO, string adminUserId);
        Task<ServiceResponse<PagedResultDTO<MarketResponseDTO>>> GetMarketsByStatusAsync(PaginationParametersDTO parametersDTO, MarketStatus status);
        Task<ServiceResponse<PagedResultDTO<MarketResponseDTO>>> SearchMarketsAsync(PaginationParametersDTO parametersDTO, string searchTerm);
        Task<ServiceResponse<PagedResultDTO<MarketResponseDTO>>> GetTrustedMarketsAsync(PaginationParametersDTO parametersDTO);
        Task<ServiceResponse<PagedResultDTO<MarketResponseDTO>>> GetDeletedMarketsAsync(PaginationParametersDTO parametersDTO);

        // ===== Statistics =====
        Task<ServiceResponse<int>> GetTotalMarketsCountAsync(bool includeDeleted = false);
        Task<ServiceResponse<int>> GetActiveMarketsCountAsync();
        Task<ServiceResponse<int>> GetMarketsByAdminCountAsync(string adminUserId);
        Task<ServiceResponse<Dictionary<MarketStatus, int>>> GetMarketCountByStatusAsync();
        Task<ServiceResponse<MarketStatisticsDTO>> GetMarketStatisticsAsync();

        // ===== Trust Management =====
        Task<ServiceResponse<bool>> SetMarketTrustedAsync(int marketId, bool isTrusted);

        // ===== Product Limit Management =====
        Task<ServiceResponse<int>> GetMarketProductCountAsync(int marketId);
        Task<ServiceResponse<bool>> CanMarketAddMoreProductsAsync(int marketId);
        Task<ServiceResponse<int>> GetRemainingProductSlotsAsync(int marketId);

        // ===== Utility =====
        Task<ServiceResponse<bool>> MarketExistsAsync(int id);
        Task<ServiceResponse<bool>> IsMarketActiveAsync(int id);
        Task<ServiceResponse<bool>> IsMarketOwnedByUserAsync(int marketId, string userId);
    }
}
