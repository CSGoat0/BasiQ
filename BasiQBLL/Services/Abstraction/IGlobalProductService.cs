using BasiQBLL.DTOs;
using BasiQBLL.DTOs.GlobalProductDTOs;
using BasiQBLL.DTOs.PaginationDTOs;

namespace BasiQBLL.Services.Abstraction
{
    public interface IGlobalProductService
    {
        // ===== CRUD Operations =====
        Task<ServiceResponse<GlobalProductResponseDTO>> GetGlobalProductByIdAsync(int id);
        Task<ServiceResponse<PagedResultDTO<GlobalProductResponseDTO>>> GetAllGlobalProductsAsync(PaginationParametersDTO parametersDTO, bool includeDeleted = false);
        Task<ServiceResponse<int>> CreateGlobalProductAsync(CreateGlobalProductDTO createDTO);
        Task<ServiceResponse<bool>> UpdateGlobalProductAsync(UpdateGlobalProductDTO updateDTO);
        Task<ServiceResponse<bool>> DeleteGlobalProductAsync(int id);
        Task<ServiceResponse<bool>> RestoreGlobalProductAsync(int id);

        // ===== Filtering & Querying =====
        Task<ServiceResponse<PagedResultDTO<GlobalProductResponseDTO>>> SearchGlobalProductsAsync(PaginationParametersDTO parametersDTO, string searchTerm);
        Task<ServiceResponse<PagedResultDTO<GlobalProductResponseDTO>>> GetDeletedGlobalProductsAsync(PaginationParametersDTO parametersDTO);
        Task<ServiceResponse<GlobalProductResponseDTO>> GetGlobalProductByNameAsync(string name);

        // ===== Product Management =====
        Task<ServiceResponse<bool>> LinkProductToGlobalAsync(int productId, int globalProductId);
        Task<ServiceResponse<bool>> UnlinkProductFromGlobalAsync(int productId);
        Task<ServiceResponse<int>> GetLinkedProductsCountAsync(int globalProductId);
        Task<ServiceResponse<PagedResultDTO<LinkedProductSummaryDTO>>> GetLinkedProductsAsync(PaginationParametersDTO parametersDTO, int globalProductId);

        // ===== Statistics =====
        Task<ServiceResponse<int>> GetTotalGlobalProductsCountAsync(bool includeDeleted = false);
        Task<ServiceResponse<GlobalProductStatisticsDTO>> GetGlobalProductStatisticsAsync();

        // ===== Utility =====
        Task<ServiceResponse<bool>> GlobalProductExistsAsync(int id);
        Task<ServiceResponse<bool>> IsProductLinkedToGlobalAsync(int productId);
    }
}
