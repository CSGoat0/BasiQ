using BasiQBLL.DTOs;
using BasiQBLL.DTOs.GlobalProductDTOs;
using BasiQBLL.DTOs.PaginationDTOs;
using BasiQBLL.Extensions;
using BasiQBLL.Mapper;
using BasiQBLL.Services.Abstraction;
using BasiQDAL.Repositories.Abstraction;
using Microsoft.Extensions.Logging;

namespace BasiQBLL.Services.Implementation
{
    public class GlobalProductService : IGlobalProductService
    {
        private readonly IGlobalProductRepository _globalProductRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMarketRepository _marketRepository;
        private readonly GlobalProductMapper _mapper;
        private readonly ILogger<GlobalProductService> _logger;

        public GlobalProductService(
            IGlobalProductRepository globalProductRepository,
            IProductRepository productRepository,
            IMarketRepository marketRepository,
            ILogger<GlobalProductService> logger)
        {
            _globalProductRepository = globalProductRepository;
            _productRepository = productRepository;
            _marketRepository = marketRepository;
            _logger = logger;
            _mapper = new GlobalProductMapper();
        }

        // ===== CRUD Operations =====

        public async Task<ServiceResponse<GlobalProductResponseDTO>> GetGlobalProductByIdAsync(int id)
        {
            try
            {
                var globalProduct = await _globalProductRepository.GetGlobalProductByIdAsync(id);
                if (globalProduct == null)
                {
                    return new ServiceResponse<GlobalProductResponseDTO>
                    {
                        Success = false,
                        Message = $"Global product with ID {id} not found."
                    };
                }

                var response = _mapper.MapToResponseDTO(globalProduct);
                return new ServiceResponse<GlobalProductResponseDTO>
                {
                    Success = true,
                    Data = response,
                    Message = "Global product retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting global product with ID: {GlobalProductId}", id);
                return new ServiceResponse<GlobalProductResponseDTO>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving the global product: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDTO<GlobalProductResponseDTO>>> GetAllGlobalProductsAsync(PaginationParametersDTO parametersDTO, bool includeDeleted = false)
        {
            try
            {
                var globalProducts = await _globalProductRepository.GetAllGlobalProductsAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize,
                    includeDeleted);

                var globalProductDTOs = _mapper.MapToResponseDTOs(globalProducts.Data);
                var response = globalProducts.ToPagedResult(globalProductDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<GlobalProductResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = "Global products retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all global products");
                return new ServiceResponse<PagedResultDTO<GlobalProductResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving global products: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<int>> CreateGlobalProductAsync(CreateGlobalProductDTO createDTO)
        {
            try
            {
                // Check if global product name already exists
                var existing = await _globalProductRepository.GetGlobalProductByNameAsync(createDTO.Name!);
                if (existing != null)
                {
                    return new ServiceResponse<int>
                    {
                        Success = false,
                        Message = $"Global product with name '{createDTO.Name}' already exists."
                    };
                }

                var globalProduct = _mapper.MapToEntity(createDTO);
                var created = await _globalProductRepository.AddGlobalProductAsync(globalProduct);

                _logger.LogInformation("Global product created successfully with ID: {GlobalProductId}", created.Id);

                return new ServiceResponse<int>
                {
                    Success = true,
                    Data = created.Id,
                    Message = "Global product created successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating global product");
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = $"An error occurred while creating the global product: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> UpdateGlobalProductAsync(UpdateGlobalProductDTO updateDTO)
        {
            try
            {
                var existingGlobalProduct = await _globalProductRepository.GetGlobalProductByIdAsync(updateDTO.Id);
                if (existingGlobalProduct == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Global product with ID {updateDTO.Id} not found."
                    };
                }

                // Check if new name conflicts with existing global product (excluding itself)
                if (!string.IsNullOrWhiteSpace(updateDTO.Name) &&
                    updateDTO.Name != existingGlobalProduct.Name)
                {
                    var existing = await _globalProductRepository.GetGlobalProductByNameAsync(updateDTO.Name);
                    if (existing != null && existing.Id != updateDTO.Id)
                    {
                        return new ServiceResponse<bool>
                        {
                            Success = false,
                            Message = $"Global product with name '{updateDTO.Name}' already exists."
                        };
                    }
                }

                _mapper.MapToEntity(updateDTO, existingGlobalProduct);
                await _globalProductRepository.UpdateGlobalProductAsync(existingGlobalProduct);

                _logger.LogInformation("Global product updated successfully with ID: {GlobalProductId}", existingGlobalProduct.Id);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Global product updated successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating global product with ID: {GlobalProductId}", updateDTO.Id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while updating the global product: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> DeleteGlobalProductAsync(int id)
        {
            try
            {
                var globalProduct = await _globalProductRepository.GetGlobalProductByIdAsync(id);
                if (globalProduct == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Global product with ID {id} not found."
                    };
                }

                // Check if global product has linked products
                var linkedCount = await _globalProductRepository.GetLinkedProductsCountAsync(id);
                if (linkedCount > 0)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Cannot delete global product with {linkedCount} linked products. Please unlink all products first."
                    };
                }

                await _globalProductRepository.DeleteGlobalProductAsync(id);
                _logger.LogInformation("Global product deleted successfully with ID: {GlobalProductId}", id);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Global product deleted successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting global product with ID: {GlobalProductId}", id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while deleting the global product: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> RestoreGlobalProductAsync(int id)
        {
            try
            {
                await _globalProductRepository.RestoreGlobalProductAsync(id);
                _logger.LogInformation("Global product restored successfully with ID: {GlobalProductId}", id);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Global product restored successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring global product with ID: {GlobalProductId}", id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while restoring the global product: {ex.Message}"
                };
            }
        }

        // ===== Filtering & Querying =====

        public async Task<ServiceResponse<PagedResultDTO<GlobalProductResponseDTO>>> SearchGlobalProductsAsync(PaginationParametersDTO parametersDTO, string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllGlobalProductsAsync(parametersDTO);
                }

                var globalProducts = await _globalProductRepository.SearchGlobalProductsAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize,
                    searchTerm);

                var globalProductDTOs = _mapper.MapToResponseDTOs(globalProducts.Data);
                var response = globalProducts.ToPagedResult(globalProductDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<GlobalProductResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = $"Search results for '{searchTerm}' retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching global products with term: {SearchTerm}", searchTerm);
                return new ServiceResponse<PagedResultDTO<GlobalProductResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while searching global products: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDTO<GlobalProductResponseDTO>>> GetDeletedGlobalProductsAsync(PaginationParametersDTO parametersDTO)
        {
            try
            {
                var globalProducts = await _globalProductRepository.GetDeletedGlobalProductsAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize);

                var globalProductDTOs = _mapper.MapToResponseDTOs(globalProducts.Data);
                var response = globalProducts.ToPagedResult(globalProductDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<GlobalProductResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = "Deleted global products retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deleted global products");
                return new ServiceResponse<PagedResultDTO<GlobalProductResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving deleted global products: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<GlobalProductResponseDTO>> GetGlobalProductByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return new ServiceResponse<GlobalProductResponseDTO>
                    {
                        Success = false,
                        Message = "Global product name is required."
                    };
                }

                var globalProduct = await _globalProductRepository.GetGlobalProductByNameAsync(name);
                if (globalProduct == null)
                {
                    return new ServiceResponse<GlobalProductResponseDTO>
                    {
                        Success = false,
                        Message = $"Global product with name '{name}' not found."
                    };
                }

                var response = _mapper.MapToResponseDTO(globalProduct);
                return new ServiceResponse<GlobalProductResponseDTO>
                {
                    Success = true,
                    Data = response,
                    Message = "Global product retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting global product by name: {Name}", name);
                return new ServiceResponse<GlobalProductResponseDTO>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving the global product: {ex.Message}"
                };
            }
        }

        // ===== Product Management =====

        public async Task<ServiceResponse<bool>> LinkProductToGlobalAsync(int productId, int globalProductId)
        {
            try
            {
                // Validate product exists
                if (!await _productRepository.ProductExistsAsync(productId))
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {productId} not found."
                    };
                }

                // Validate global product exists
                if (!await _globalProductRepository.GlobalProductExistsAsync(globalProductId))
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Global product with ID {globalProductId} not found."
                    };
                }

                // Check if product is already linked
                if (await _globalProductRepository.IsProductLinkedToGlobalAsync(productId))
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {productId} is already linked to a global product."
                    };
                }

                var success = await _globalProductRepository.LinkProductToGlobalAsync(productId, globalProductId);
                if (!success)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "Failed to link product to global product."
                    };
                }

                _logger.LogInformation(
                    "Product {ProductId} linked to global product {GlobalProductId}",
                    productId,
                    globalProductId);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Product linked to global product successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error linking product {ProductId} to global product {GlobalProductId}",
                    productId,
                    globalProductId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while linking product to global product: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> UnlinkProductFromGlobalAsync(int productId)
        {
            try
            {
                // Validate product exists
                if (!await _productRepository.ProductExistsAsync(productId))
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {productId} not found."
                    };
                }

                // Check if product is linked
                if (!await _globalProductRepository.IsProductLinkedToGlobalAsync(productId))
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {productId} is not linked to any global product."
                    };
                }

                var success = await _globalProductRepository.UnlinkProductFromGlobalAsync(productId);
                if (!success)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "Failed to unlink product from global product."
                    };
                }

                _logger.LogInformation("Product {ProductId} unlinked from global product", productId);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Product unlinked from global product successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlinking product {ProductId} from global product", productId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while unlinking product from global product: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<int>> GetLinkedProductsCountAsync(int globalProductId)
        {
            try
            {
                if (!await _globalProductRepository.GlobalProductExistsAsync(globalProductId))
                {
                    return new ServiceResponse<int>
                    {
                        Success = false,
                        Message = $"Global product with ID {globalProductId} not found."
                    };
                }

                var count = await _globalProductRepository.GetLinkedProductsCountAsync(globalProductId);
                return new ServiceResponse<int>
                {
                    Success = true,
                    Data = count,
                    Message = "Linked products count retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting linked products count for global product {GlobalProductId}", globalProductId);
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving linked products count: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDTO<LinkedProductSummaryDTO>>> GetLinkedProductsAsync(PaginationParametersDTO parametersDTO, int globalProductId)
        {
            try
            {
                if (!await _globalProductRepository.GlobalProductExistsAsync(globalProductId))
                {
                    return new ServiceResponse<PagedResultDTO<LinkedProductSummaryDTO>>
                    {
                        Success = false,
                        Message = $"Global product with ID {globalProductId} not found."
                    };
                }

                var globalProduct = await _globalProductRepository.GetGlobalProductByIdAsync(globalProductId);
                if (globalProduct == null || globalProduct.Products == null)
                {
                    return new ServiceResponse<PagedResultDTO<LinkedProductSummaryDTO>>
                    {
                        Success = true,
                        Data = new PagedResultDTO<LinkedProductSummaryDTO>
                        {
                            Items = new List<LinkedProductSummaryDTO>(),
                            TotalCount = 0,
                            PageNumber = parametersDTO.PageNumber,
                            PageSize = parametersDTO.PageSize
                        },
                        Message = "No linked products found."
                    };
                }

                var linkedProducts = globalProduct.Products
                    .Where(p => !p.IsDeleted)
                    .Skip((parametersDTO.PageNumber - 1) * parametersDTO.PageSize)
                    .Take(parametersDTO.PageSize)
                    .ToList();

                var totalCount = globalProduct.Products.Count(p => !p.IsDeleted);

                var linkedProductDTOs = linkedProducts.Select(p => new LinkedProductSummaryDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    MarketId = p.MarketId,
                    MarketName = p.Market?.Name ?? string.Empty,
                    CurrentPrice = p.GetCurrentPrice(),
                    IsInStock = p.IsInStock(),
                    PrimaryImageUrl = p.ProductImages?.FirstOrDefault(i => i.IsPrimary && !i.IsDeleted)?.ImageUrl
                }).ToList();

                var response = new PagedResultDTO<LinkedProductSummaryDTO>
                {
                    Items = linkedProductDTOs,
                    TotalCount = totalCount,
                    PageNumber = parametersDTO.PageNumber,
                    PageSize = parametersDTO.PageSize
                };

                return new ServiceResponse<PagedResultDTO<LinkedProductSummaryDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = "Linked products retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting linked products for global product {GlobalProductId}", globalProductId);
                return new ServiceResponse<PagedResultDTO<LinkedProductSummaryDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving linked products: {ex.Message}"
                };
            }
        }

        // ===== Statistics =====

        public async Task<ServiceResponse<int>> GetTotalGlobalProductsCountAsync(bool includeDeleted = false)
        {
            try
            {
                var count = await _globalProductRepository.GetTotalGlobalProductsCountAsync(includeDeleted);
                return new ServiceResponse<int>
                {
                    Success = true,
                    Data = count,
                    Message = "Total global products count retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total global products count");
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving count: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<GlobalProductStatisticsDTO>> GetGlobalProductStatisticsAsync()
        {
            try
            {
                var totalGlobalProducts = await _globalProductRepository.GetTotalGlobalProductsCountAsync(false);
                var totalProducts = await _productRepository.GetTotalProductsCountAsync(false);
                var productCounts = await _globalProductRepository.GetGlobalProductProductCountsAsync();

                var linkedProductsCount = productCounts.Values.Sum();
                var unlinkedProducts = totalProducts - linkedProductsCount;
                var globalProductsWithNoLinks = productCounts.Count(kvp => kvp.Value == 0);

                // Get the most linked global product
                var mostLinkedGlobalProductId = productCounts.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;
                GlobalProductResponseDTO? mostLinkedGlobalProduct = null;

                if (mostLinkedGlobalProductId > 0)
                {
                    var mostLinked = await _globalProductRepository.GetGlobalProductByIdAsync(mostLinkedGlobalProductId);
                    if (mostLinked != null)
                    {
                        mostLinkedGlobalProduct = _mapper.MapToResponseDTO(mostLinked);
                    }
                }

                var averageProductsPerGlobalProduct = totalGlobalProducts > 0
                    ? Math.Round((double)linkedProductsCount / totalGlobalProducts, 2)
                    : 0;

                var statistics = _mapper.MapToStatisticsDTO(
                    totalGlobalProducts,
                    linkedProductsCount,
                    unlinkedProducts,
                    globalProductsWithNoLinks,
                    averageProductsPerGlobalProduct,
                    mostLinkedGlobalProduct);

                return new ServiceResponse<GlobalProductStatisticsDTO>
                {
                    Success = true,
                    Data = statistics,
                    Message = "Global product statistics retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting global product statistics");
                return new ServiceResponse<GlobalProductStatisticsDTO>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving statistics: {ex.Message}"
                };
            }
        }

        // ===== Utility =====

        public async Task<ServiceResponse<bool>> GlobalProductExistsAsync(int id)
        {
            try
            {
                var exists = await _globalProductRepository.GlobalProductExistsAsync(id);
                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = exists,
                    Message = exists ? "Global product exists." : "Global product does not exist."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if global product exists: {GlobalProductId}", id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while checking global product existence: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> IsProductLinkedToGlobalAsync(int productId)
        {
            try
            {
                if (!await _productRepository.ProductExistsAsync(productId))
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {productId} not found."
                    };
                }

                var isLinked = await _globalProductRepository.IsProductLinkedToGlobalAsync(productId);
                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = isLinked,
                    Message = isLinked ? "Product is linked to a global product." : "Product is not linked to any global product."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product {ProductId} is linked to global product", productId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while checking product link status: {ex.Message}"
                };
            }
        }
    }
}
