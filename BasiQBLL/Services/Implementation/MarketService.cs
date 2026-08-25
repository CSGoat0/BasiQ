using BasiQBLL.DTOs;
using BasiQBLL.DTOs.MarketDTOs;
using BasiQBLL.DTOs.PaginationDTOs;
using BasiQBLL.Extensions;
using BasiQBLL.Mapper;
using BasiQBLL.Services.Abstraction;
using BasiQDAL.Enums;
using BasiQDAL.Repositories.Abstraction;
using Microsoft.Extensions.Logging;

namespace BasiQBLL.Services.Implementation
{
    public class MarketService : IMarketService
    {
        private readonly IMarketRepository _marketRepository;
        private readonly IProductRepository _productRepository;
        private readonly MarketMapper _mapper;
        private readonly ILogger<MarketService> _logger;

        public MarketService(
            IMarketRepository marketRepository,
            IProductRepository productRepository,
            ILogger<MarketService> logger)
        {
            _marketRepository = marketRepository;
            _productRepository = productRepository;
            _logger = logger;
            _mapper = new MarketMapper();
        }

        // ===== CRUD Operations =====

        public async Task<ServiceResponse<MarketResponseDTO>> GetMarketByIdAsync(int id)
        {
            try
            {
                var market = await _marketRepository.GetMarketByIdAsync(id);
                if (market == null)
                {
                    return new ServiceResponse<MarketResponseDTO>
                    {
                        Success = false,
                        Message = $"Market with ID {id} not found."
                    };
                }

                var response = _mapper.MapToResponseDTO(market);
                return new ServiceResponse<MarketResponseDTO>
                {
                    Success = true,
                    Data = response,
                    Message = "Market retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting market with ID: {MarketId}", id);
                return new ServiceResponse<MarketResponseDTO>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving the market: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDTO<MarketResponseDTO>>> GetAllMarketsAsync(PaginationParametersDTO parametersDTO, bool includeDeleted = false)
        {
            try
            {
                var markets = await _marketRepository.GetAllMarketsAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize,
                    includeDeleted);

                var marketDTOs = _mapper.MapToResponseDTOs(markets.Data);
                var response = markets.ToPagedResult(marketDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<MarketResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = "Markets retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all markets");
                return new ServiceResponse<PagedResultDTO<MarketResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving markets: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<int>> CreateMarketAsync(CreateMarketDTO createDTO)
        {
            try
            {
                // Validate user exists (optional - can be checked via UserService)
                if (string.IsNullOrWhiteSpace(createDTO.AdminUserId))
                {
                    return new ServiceResponse<int>
                    {
                        Success = false,
                        Message = "Admin user ID is required."
                    };
                }

                var market = _mapper.MapToEntity(createDTO);
                var created = await _marketRepository.AddMarketAsync(market);

                _logger.LogInformation("Market created successfully with ID: {MarketId}", created.Id);

                return new ServiceResponse<int>
                {
                    Success = true,
                    Data = created.Id,
                    Message = "Market created successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating market");
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = $"An error occurred while creating the market: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> UpdateMarketAsync(UpdateMarketDTO updateDTO)
        {
            try
            {
                var existingMarket = await _marketRepository.GetMarketByIdAsync(updateDTO.Id);
                if (existingMarket == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Market with ID {updateDTO.Id} not found."
                    };
                }

                _mapper.MapToEntity(updateDTO, existingMarket);
                await _marketRepository.UpdateMarketAsync(existingMarket);

                _logger.LogInformation("Market updated successfully with ID: {MarketId}", existingMarket.Id);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Market updated successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating market with ID: {MarketId}", updateDTO.Id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while updating the market: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> UpdateMarketStatusAsync(int marketId, MarketStatus status)
        {
            try
            {
                var market = await _marketRepository.GetMarketByIdAsync(marketId);
                if (market == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Market with ID {marketId} not found."
                    };
                }

                var oldStatus = market.Status;
                market.UpdateStatus(status);
                await _marketRepository.UpdateMarketAsync(market);

                _logger.LogInformation(
                    "Market {MarketId} status updated from {OldStatus} to {NewStatus}",
                    marketId,
                    oldStatus,
                    status);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Market status updated successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating market status for ID: {MarketId}", marketId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while updating market status: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> DeleteMarketAsync(int id)
        {
            try
            {
                var market = await _marketRepository.GetMarketByIdAsync(id);
                if (market == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Market with ID {id} not found."
                    };
                }

                await _marketRepository.DeleteMarketAsync(id);
                _logger.LogInformation("Market deleted successfully with ID: {MarketId}", id);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Market deleted successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting market with ID: {MarketId}", id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while deleting the market: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> RestoreMarketAsync(int id)
        {
            try
            {
                await _marketRepository.RestoreMarketAsync(id);
                _logger.LogInformation("Market restored successfully with ID: {MarketId}", id);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Market restored successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring market with ID: {MarketId}", id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while restoring the market: {ex.Message}"
                };
            }
        }

        // ===== Filtering & Querying =====

        public async Task<ServiceResponse<PagedResultDTO<MarketResponseDTO>>> GetMarketsByAdminAsync(PaginationParametersDTO parametersDTO, string adminUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(adminUserId))
                {
                    return new ServiceResponse<PagedResultDTO<MarketResponseDTO>>
                    {
                        Success = false,
                        Message = "Admin user ID is required."
                    };
                }

                var markets = await _marketRepository.GetMarketsByAdminAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize,
                    adminUserId);

                var marketDTOs = _mapper.MapToResponseDTOs(markets.Data);
                var response = markets.ToPagedResult(marketDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<MarketResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = "Markets retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting markets by admin: {AdminUserId}", adminUserId);
                return new ServiceResponse<PagedResultDTO<MarketResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving markets: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDTO<MarketResponseDTO>>> GetMarketsByStatusAsync(PaginationParametersDTO parametersDTO, MarketStatus status)
        {
            try
            {
                var markets = await _marketRepository.GetMarketsByStatusAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize,
                    status);

                var marketDTOs = _mapper.MapToResponseDTOs(markets.Data);
                var response = markets.ToPagedResult(marketDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<MarketResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = $"Markets with status {status} retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting markets by status: {Status}", status);
                return new ServiceResponse<PagedResultDTO<MarketResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving markets: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDTO<MarketResponseDTO>>> SearchMarketsAsync(PaginationParametersDTO parametersDTO, string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllMarketsAsync(parametersDTO);
                }

                var markets = await _marketRepository.SearchMarketsAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize,
                    searchTerm);

                var marketDTOs = _mapper.MapToResponseDTOs(markets.Data);
                var response = markets.ToPagedResult(marketDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<MarketResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = $"Search results for '{searchTerm}' retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching markets with term: {SearchTerm}", searchTerm);
                return new ServiceResponse<PagedResultDTO<MarketResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while searching markets: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDTO<MarketResponseDTO>>> GetTrustedMarketsAsync(PaginationParametersDTO parametersDTO)
        {
            try
            {
                var markets = await _marketRepository.GetTrustedMarketsAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize);

                var marketDTOs = _mapper.MapToResponseDTOs(markets.Data);
                var response = markets.ToPagedResult(marketDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<MarketResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = "Trusted markets retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trusted markets");
                return new ServiceResponse<PagedResultDTO<MarketResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving trusted markets: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDTO<MarketResponseDTO>>> GetDeletedMarketsAsync(PaginationParametersDTO parametersDTO)
        {
            try
            {
                var markets = await _marketRepository.GetDeletedMarketsAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize);

                var marketDTOs = _mapper.MapToResponseDTOs(markets.Data);
                var response = markets.ToPagedResult(marketDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<MarketResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = "Deleted markets retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deleted markets");
                return new ServiceResponse<PagedResultDTO<MarketResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving deleted markets: {ex.Message}"
                };
            }
        }

        // ===== Statistics =====

        public async Task<ServiceResponse<int>> GetTotalMarketsCountAsync(bool includeDeleted = false)
        {
            try
            {
                var count = await _marketRepository.GetTotalMarketsCountAsync(includeDeleted);
                return new ServiceResponse<int>
                {
                    Success = true,
                    Data = count,
                    Message = "Total markets count retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total markets count");
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving count: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<int>> GetActiveMarketsCountAsync()
        {
            try
            {
                var count = await _marketRepository.GetActiveMarketsCountAsync();
                return new ServiceResponse<int>
                {
                    Success = true,
                    Data = count,
                    Message = "Active markets count retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active markets count");
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving count: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<int>> GetMarketsByAdminCountAsync(string adminUserId)
        {
            try
            {
                var count = await _marketRepository.GetMarketsByAdminCountAsync(adminUserId);
                return new ServiceResponse<int>
                {
                    Success = true,
                    Data = count,
                    Message = "Markets by admin count retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting markets by admin count: {AdminUserId}", adminUserId);
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving count: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<Dictionary<MarketStatus, int>>> GetMarketCountByStatusAsync()
        {
            try
            {
                var counts = await _marketRepository.GetMarketCountByStatusAsync();
                return new ServiceResponse<Dictionary<MarketStatus, int>>
                {
                    Success = true,
                    Data = counts,
                    Message = "Market count by status retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting market count by status");
                return new ServiceResponse<Dictionary<MarketStatus, int>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving counts: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<MarketStatisticsDTO>> GetMarketStatisticsAsync()
        {
            try
            {
                var totalMarkets = await _marketRepository.GetTotalMarketsCountAsync(false);
                var activeMarkets = await _marketRepository.GetActiveMarketsCountAsync();
                var countByStatus = await _marketRepository.GetMarketCountByStatusAsync();
                var totalProducts = await _productRepository.GetTotalProductsCountAsync(false);
                var availableProducts = await _productRepository.GetAvailableProductsCountAsync();

                var suspendedMarkets = countByStatus.GetValueOrDefault(MarketStatus.Suspended, 0);
                var closedMarkets = countByStatus.GetValueOrDefault(MarketStatus.Closed, 0);
                var trustedMarkets = (await _marketRepository.GetTrustedMarketsAsync(1, int.MaxValue)).Data.Count();
                var untrustedMarkets = totalMarkets - trustedMarkets;

                var statistics = _mapper.MapToStatisticsDTO(
                    totalMarkets,
                    activeMarkets,
                    suspendedMarkets,
                    closedMarkets,
                    trustedMarkets,
                    untrustedMarkets,
                    totalProducts,
                    availableProducts,
                    countByStatus);

                return new ServiceResponse<MarketStatisticsDTO>
                {
                    Success = true,
                    Data = statistics,
                    Message = "Market statistics retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting market statistics");
                return new ServiceResponse<MarketStatisticsDTO>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving statistics: {ex.Message}"
                };
            }
        }

        // ===== Trust Management =====

        public async Task<ServiceResponse<bool>> SetMarketTrustedAsync(int marketId, bool isTrusted)
        {
            try
            {
                var market = await _marketRepository.GetMarketByIdAsync(marketId);
                if (market == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Market with ID {marketId} not found."
                    };
                }

                market.SetTrusted(isTrusted);
                await _marketRepository.UpdateMarketAsync(market);

                _logger.LogInformation(
                    "Market {MarketId} trusted status set to {IsTrusted}",
                    marketId,
                    isTrusted);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = $"Market trusted status updated to {isTrusted}."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting market trusted status for ID: {MarketId}", marketId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while updating market trust status: {ex.Message}"
                };
            }
        }

        // ===== Product Limit Management =====

        public async Task<ServiceResponse<int>> GetMarketProductCountAsync(int marketId)
        {
            try
            {
                var count = await _marketRepository.GetMarketProductCountAsync(marketId);
                return new ServiceResponse<int>
                {
                    Success = true,
                    Data = count,
                    Message = "Market product count retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting market product count for ID: {MarketId}", marketId);
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving product count: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> CanMarketAddMoreProductsAsync(int marketId)
        {
            try
            {
                var canAdd = await _marketRepository.CanMarketAddMoreProductsAsync(marketId);
                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = canAdd,
                    Message = canAdd ? "Market can add more products." : "Market has reached product limit."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if market can add more products for ID: {MarketId}", marketId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while checking product limit: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<int>> GetRemainingProductSlotsAsync(int marketId)
        {
            try
            {
                var slots = await _marketRepository.GetRemainingProductSlotsAsync(marketId);
                return new ServiceResponse<int>
                {
                    Success = true,
                    Data = slots,
                    Message = "Remaining product slots retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting remaining product slots for ID: {MarketId}", marketId);
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving remaining slots: {ex.Message}"
                };
            }
        }

        // ===== Utility =====

        public async Task<ServiceResponse<bool>> MarketExistsAsync(int id)
        {
            try
            {
                var exists = await _marketRepository.MarketExistsAsync(id);
                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = exists,
                    Message = exists ? "Market exists." : "Market does not exist."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if market exists: {MarketId}", id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while checking market existence: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> IsMarketActiveAsync(int id)
        {
            try
            {
                var isActive = await _marketRepository.IsMarketActiveAsync(id);
                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = isActive,
                    Message = isActive ? "Market is active." : "Market is not active."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if market is active: {MarketId}", id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while checking market status: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> IsMarketOwnedByUserAsync(int marketId, string userId)
        {
            try
            {
                var isOwned = await _marketRepository.IsMarketOwnedByUserAsync(marketId, userId);
                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = isOwned,
                    Message = isOwned ? "User owns this market." : "User does not own this market."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user owns market: {MarketId}, {UserId}", marketId, userId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while checking market ownership: {ex.Message}"
                };
            }
        }
    }
}
