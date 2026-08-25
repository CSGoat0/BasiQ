using BasiQBLL.DTOs;
using BasiQBLL.DTOs.PaginationDTOs;
using BasiQBLL.DTOs.ProductDTOs;
using BasiQBLL.DTOs.ProductImageDTOs;
using BasiQBLL.DTOs.RejectionDTOs;
using BasiQBLL.Extensions;
using BasiQBLL.Mapper;
using BasiQBLL.Services.Abstraction;
using BasiQDAL.Entities;
using BasiQDAL.Enums;
using BasiQDAL.Repositories.Abstraction;
using Microsoft.Extensions.Logging;

namespace BasiQBLL.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMarketRepository _marketRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IGlobalProductRepository _globalProductRepository;
        private readonly ProductMapper _mapper;
        private readonly ProductImageMapper _imageMapper;
        private readonly ProductRejectionMapper _rejectionMapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepository,
            IMarketRepository marketRepository,
            ICategoryRepository categoryRepository,
            IGlobalProductRepository globalProductRepository,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _marketRepository = marketRepository;
            _categoryRepository = categoryRepository;
            _globalProductRepository = globalProductRepository;
            _logger = logger;
            _mapper = new ProductMapper();
            _imageMapper = new ProductImageMapper();
            _rejectionMapper = new ProductRejectionMapper();
        }

        // ===== CRUD Operations =====

        public async Task<ServiceResponse<ProductResponseDTO>> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _productRepository.GetProductByIdAsync(id);
                if (product == null)
                {
                    return new ServiceResponse<ProductResponseDTO>
                    {
                        Success = false,
                        Message = $"Product with ID {id} not found."
                    };
                }

                var response = _mapper.MapToResponseDTO(product);
                return new ServiceResponse<ProductResponseDTO>
                {
                    Success = true,
                    Data = response,
                    Message = "Product retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product with ID: {ProductId}", id);
                return new ServiceResponse<ProductResponseDTO>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving the product: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<ProductDetailsDTO>> GetProductDetailsByIdAsync(int id)
        {
            try
            {
                var product = await _productRepository.GetProductByIdAsync(id);
                if (product == null)
                {
                    return new ServiceResponse<ProductDetailsDTO>
                    {
                        Success = false,
                        Message = $"Product with ID {id} not found."
                    };
                }

                var response = _mapper.MapToDetailsDTO(product);
                return new ServiceResponse<ProductDetailsDTO>
                {
                    Success = true,
                    Data = response,
                    Message = "Product details retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product details with ID: {ProductId}", id);
                return new ServiceResponse<ProductDetailsDTO>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving product details: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDTO<ProductResponseDTO>>> GetAllProductsAsync(PaginationParametersDTO parametersDTO, bool includeDeleted = false)
        {
            try
            {
                var products = await _productRepository.GetAllProductsAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize,
                    includeDeleted);

                var productDTOs = _mapper.MapToResponseDTOs(products.Data);
                var response = products.ToPagedResult(productDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = "Products retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving products: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<int>> CreateProductAsync(CreateProductDTO createDTO)
        {
            try
            {
                // Validate market exists
                if (!await _marketRepository.MarketExistsAsync(createDTO.MarketId))
                {
                    return new ServiceResponse<int>
                    {
                        Success = false,
                        Message = $"Market with ID {createDTO.MarketId} not found."
                    };
                }

                // Check product limit for the market
                if (!await _marketRepository.CanMarketAddMoreProductsAsync(createDTO.MarketId))
                {
                    return new ServiceResponse<int>
                    {
                        Success = false,
                        Message = "Market has reached its maximum product limit of 100 products."
                    };
                }

                var market = await _marketRepository.GetMarketByIdAsync(createDTO.MarketId);
                var product = _mapper.MapToEntity(createDTO);

                // Auto-approve if market is trusted
                if (market != null && market.IsTrusted)
                {
                    product.Approve();
                    _logger.LogInformation("Product auto-approved for trusted market {MarketId}", createDTO.MarketId);
                }

                var created = await _productRepository.AddProductAsync(product);

                // Add images if provided
                if (createDTO.ImageUrls != null && createDTO.ImageUrls.Any())
                {
                    foreach (var imageUrl in createDTO.ImageUrls)
                    {
                        var image = new ProductImage(created.Id, imageUrl);
                        await _productRepository.AddProductImageAsync(image);
                    }

                    // Set first image as primary
                    var firstImage = await _productRepository.GetProductImagesAsync(1, 1, created.Id);
                    if (firstImage.Data.Any())
                    {
                        await _productRepository.SetPrimaryImageAsync(firstImage.Data.First().Id);
                    }
                }

                // Add categories if provided
                if (createDTO.CategoryIds != null && createDTO.CategoryIds.Any())
                {
                    foreach (var categoryId in createDTO.CategoryIds)
                    {
                        if (await _categoryRepository.CategoryExistsAsync(categoryId))
                        {
                            await _productRepository.AddCategoryToProductAsync(created.Id, categoryId);
                        }
                    }
                }

                _logger.LogInformation("Product created successfully with ID: {ProductId}", created.Id);

                return new ServiceResponse<int>
                {
                    Success = true,
                    Data = created.Id,
                    Message = market != null && market.IsTrusted
                        ? "Product created and auto-approved successfully."
                        : "Product created successfully. Waiting for SuperAdmin approval."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = $"An error occurred while creating the product: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> UpdateProductAsync(UpdateProductDTO updateDTO)
        {
            try
            {
                var existingProduct = await _productRepository.GetProductByIdAsync(updateDTO.Id);
                if (existingProduct == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {updateDTO.Id} not found."
                    };
                }

                // Don't allow updates to rejected products
                if (existingProduct.Status == ProductStatus.Rejected)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "Cannot update a rejected product. Please reopen for review first."
                    };
                }

                _mapper.MapToEntity(updateDTO, existingProduct);
                await _productRepository.UpdateProductAsync(existingProduct);

                // Update categories if provided
                if (updateDTO.CategoryIds != null)
                {
                    await _productRepository.UpdateProductCategoriesAsync(updateDTO.Id, updateDTO.CategoryIds);
                }

                _logger.LogInformation("Product updated successfully with ID: {ProductId}", existingProduct.Id);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Product updated successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID: {ProductId}", updateDTO.Id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while updating the product: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _productRepository.GetProductByIdAsync(id);
                if (product == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {id} not found."
                    };
                }

                await _productRepository.DeleteProductAsync(id);
                _logger.LogInformation("Product deleted successfully with ID: {ProductId}", id);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Product deleted successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while deleting the product: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> RestoreProductAsync(int id)
        {
            try
            {
                await _productRepository.RestoreProductAsync(id);
                _logger.LogInformation("Product restored successfully with ID: {ProductId}", id);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Product restored successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring product with ID: {ProductId}", id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while restoring the product: {ex.Message}"
                };
            }
        }

        // ===== Filtering & Querying =====

        public async Task<ServiceResponse<PagedResultDTO<ProductResponseDTO>>> GetProductsByMarketAsync(PaginationParametersDTO parametersDTO, int marketId)
        {
            try
            {
                if (!await _marketRepository.MarketExistsAsync(marketId))
                {
                    return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                    {
                        Success = false,
                        Message = $"Market with ID {marketId} not found."
                    };
                }

                var products = await _productRepository.GetProductsByMarketAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize,
                    marketId);

                var productDTOs = _mapper.MapToResponseDTOs(products.Data);
                var response = products.ToPagedResult(productDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = "Products retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by market: {MarketId}", marketId);
                return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving products: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDTO<ProductResponseDTO>>> GetProductsByStatusAsync(PaginationParametersDTO parametersDTO, ProductStatus status)
        {
            try
            {
                var products = await _productRepository.GetProductsByStatusAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize,
                    status);

                var productDTOs = _mapper.MapToResponseDTOs(products.Data);
                var response = products.ToPagedResult(productDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = $"Products with status {status} retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by status: {Status}", status);
                return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving products: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDTO<ProductResponseDTO>>> GetProductsByGlobalProductAsync(PaginationParametersDTO parametersDTO, int globalProductId)
        {
            try
            {
                if (!await _globalProductRepository.GlobalProductExistsAsync(globalProductId))
                {
                    return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                    {
                        Success = false,
                        Message = $"Global product with ID {globalProductId} not found."
                    };
                }

                var products = await _productRepository.GetProductsByGlobalProductAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize,
                    globalProductId);

                var productDTOs = _mapper.MapToResponseDTOs(products.Data);
                var response = products.ToPagedResult(productDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = "Products retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by global product: {GlobalProductId}", globalProductId);
                return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving products: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDTO<ProductResponseDTO>>> SearchProductsAsync(PaginationParametersDTO parametersDTO, string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllProductsAsync(parametersDTO);
                }

                var products = await _productRepository.SearchProductsAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize,
                    searchTerm);

                var productDTOs = _mapper.MapToResponseDTOs(products.Data);
                var response = products.ToPagedResult(productDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = $"Search results for '{searchTerm}' retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products with term: {SearchTerm}", searchTerm);
                return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while searching products: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDTO<ProductResponseDTO>>> GetProductsByPriceRangeAsync(PaginationParametersDTO parametersDTO, double minPrice, double maxPrice)
        {
            try
            {
                if (minPrice < 0 || maxPrice < minPrice)
                {
                    return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                    {
                        Success = false,
                        Message = "Invalid price range. Min price must be >= 0 and max price must be >= min price."
                    };
                }

                var products = await _productRepository.GetProductsByPriceRangeAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize,
                    minPrice,
                    maxPrice);

                var productDTOs = _mapper.MapToResponseDTOs(products.Data);
                var response = products.ToPagedResult(productDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = $"Products with price between {minPrice} and {maxPrice} retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by price range: {MinPrice} - {MaxPrice}", minPrice, maxPrice);
                return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving products: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDTO<ProductResponseDTO>>> GetAvailableProductsAsync(PaginationParametersDTO parametersDTO)
        {
            try
            {
                var products = await _productRepository.GetAvailableProductsAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize);

                var productDTOs = _mapper.MapToResponseDTOs(products.Data);
                var response = products.ToPagedResult(productDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = "Available products retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available products");
                return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving available products: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDTO<ProductResponseDTO>>> GetProductsOnSaleAsync(PaginationParametersDTO parametersDTO)
        {
            try
            {
                var products = await _productRepository.GetProductsOnSaleAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize);

                var productDTOs = _mapper.MapToResponseDTOs(products.Data);
                var response = products.ToPagedResult(productDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = "Products on sale retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products on sale");
                return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving products on sale: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDTO<ProductResponseDTO>>> GetDeletedProductsAsync(PaginationParametersDTO parametersDTO)
        {
            try
            {
                var products = await _productRepository.GetDeletedProductsAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize);

                var productDTOs = _mapper.MapToResponseDTOs(products.Data);
                var response = products.ToPagedResult(productDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = "Deleted products retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deleted products");
                return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving deleted products: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDTO<ProductResponseDTO>>> FilterProductsAsync(PaginationParametersDTO parametersDTO, ProductFilterDTO filterDTO)
        {
            try
            {
                var (products, totalCount) = await _productRepository.FilterProductsAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize,
                    filterDTO.SearchTerm,
                    filterDTO.MarketId,
                    filterDTO.Status,
                    filterDTO.GlobalProductId,
                    filterDTO.MinPrice,
                    filterDTO.MaxPrice,
                    filterDTO.InStock,
                    filterDTO.OnSale,
                    filterDTO.CategoryIds,
                    filterDTO.CreatedFrom,
                    filterDTO.CreatedTo,
                    filterDTO.IncludeDeleted);

                var productDTOs = _mapper.MapToResponseDTOs(products);
                var response = new PagedResultDTO<ProductResponseDTO>
                {
                    Items = productDTOs,
                    TotalCount = totalCount,
                    PageNumber = parametersDTO.PageNumber,
                    PageSize = parametersDTO.PageSize
                };

                return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = "Filtered products retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering products");
                return new ServiceResponse<PagedResultDTO<ProductResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while filtering products: {ex.Message}"
                };
            }
        }

        // ===== Product Status Management =====

        public async Task<ServiceResponse<bool>> UpdateProductStatusAsync(int productId, ProductStatus status)
        {
            try
            {
                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {productId} not found."
                    };
                }

                var oldStatus = product.Status;
                product.UpdateStatus(status);
                await _productRepository.UpdateProductAsync(product);

                _logger.LogInformation(
                    "Product {ProductId} status updated from {OldStatus} to {NewStatus}",
                    productId,
                    oldStatus,
                    status);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Product status updated successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product status for ID: {ProductId}", productId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while updating product status: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> ApproveProductAsync(int productId)
        {
            try
            {
                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {productId} not found."
                    };
                }

                if (product.Status != ProductStatus.Pending)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {productId} is not in pending status. Current status: {product.Status}"
                    };
                }

                product.Approve();
                await _productRepository.UpdateProductAsync(product);

                _logger.LogInformation("Product {ProductId} approved successfully", productId);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Product approved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving product with ID: {ProductId}", productId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while approving the product: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> RejectProductAsync(RejectProductDTO rejectDTO)
        {
            try
            {
                var product = await _productRepository.GetProductByIdAsync(rejectDTO.ProductId);
                if (product == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {rejectDTO.ProductId} not found."
                    };
                }

                if (product.Status != ProductStatus.Pending)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {rejectDTO.ProductId} is not in pending status. Current status: {product.Status}"
                    };
                }

                // Create rejection record
                var rejection = _rejectionMapper.MapToEntity(rejectDTO);
                await _productRepository.AddRejectionToProductAsync(rejection);

                // Update product status
                product.Reject();
                await _productRepository.UpdateProductAsync(product);

                _logger.LogInformation("Product {ProductId} rejected successfully", rejectDTO.ProductId);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Product rejected successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting product with ID: {ProductId}", rejectDTO.ProductId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while rejecting the product: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> DiscontinueProductAsync(int productId)
        {
            try
            {
                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {productId} not found."
                    };
                }

                if (product.Status != ProductStatus.Available && product.Status != ProductStatus.OutOfStock)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {productId} cannot be discontinued. Current status: {product.Status}"
                    };
                }

                product.Discontinue();
                await _productRepository.UpdateProductAsync(product);

                _logger.LogInformation("Product {ProductId} discontinued successfully", productId);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Product discontinued successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discontinuing product with ID: {ProductId}", productId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while discontinuing the product: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> ReopenProductForReviewAsync(int productId)
        {
            try
            {
                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {productId} not found."
                    };
                }

                if (product.Status != ProductStatus.Rejected)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {productId} is not in rejected status. Current status: {product.Status}"
                    };
                }

                product.ReopenForReview();
                await _productRepository.UpdateProductAsync(product);

                _logger.LogInformation("Product {ProductId} reopened for review", productId);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Product reopened for review successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reopening product for review with ID: {ProductId}", productId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while reopening the product: {ex.Message}"
                };
            }
        }

        // ===== Stock Management =====

        public async Task<ServiceResponse<bool>> UpdateProductStockAsync(int productId, int stock)
        {
            try
            {
                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {productId} not found."
                    };
                }

                var oldStock = product.Stock ?? 0;
                product.EditStock(stock);
                await _productRepository.UpdateProductAsync(product);

                _logger.LogInformation(
                    "Product {ProductId} stock updated from {OldStock} to {NewStock}",
                    productId,
                    oldStock,
                    stock);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Product stock updated successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product stock for ID: {ProductId}", productId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while updating product stock: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> IncrementProductStockAsync(int productId, int amount)
        {
            try
            {
                if (amount <= 0)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "Amount must be greater than 0."
                    };
                }

                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {productId} not found."
                    };
                }

                var oldStock = product.Stock ?? 0;
                var newStock = oldStock + amount;
                product.EditStock(newStock);
                await _productRepository.UpdateProductAsync(product);

                _logger.LogInformation(
                    "Product {ProductId} stock incremented by {Amount} from {OldStock} to {NewStock}",
                    productId,
                    amount,
                    oldStock,
                    newStock);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Product stock incremented successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing product stock for ID: {ProductId}", productId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while incrementing product stock: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> DecrementProductStockAsync(int productId, int amount)
        {
            try
            {
                if (amount <= 0)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "Amount must be greater than 0."
                    };
                }

                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {productId} not found."
                    };
                }

                var oldStock = product.Stock ?? 0;
                var newStock = Math.Max(0, oldStock - amount);
                product.EditStock(newStock);
                await _productRepository.UpdateProductAsync(product);

                _logger.LogInformation(
                    "Product {ProductId} stock decremented by {Amount} from {OldStock} to {NewStock}",
                    productId,
                    amount,
                    oldStock,
                    newStock);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Product stock decremented successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrementing product stock for ID: {ProductId}", productId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while decrementing product stock: {ex.Message}"
                };
            }
        }

        // ===== Product Image Management =====

        public async Task<ServiceResponse<bool>> AddProductImageAsync(AddProductImageDTO addImageDTO)
        {
            try
            {
                if (!await _productRepository.ProductExistsAsync(addImageDTO.ProductId))
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {addImageDTO.ProductId} not found."
                    };
                }

                var imageCount = await _productRepository.GetProductImageCountAsync(addImageDTO.ProductId);
                if (imageCount >= 5)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "Maximum 5 images allowed per product."
                    };
                }

                var image = _imageMapper.MapToEntity(addImageDTO);
                await _productRepository.AddProductImageAsync(image);

                // If this is the first image or marked as primary, set it as primary
                if (imageCount == 0 || addImageDTO.IsPrimary)
                {
                    await _productRepository.SetPrimaryImageAsync(image.Id);
                }

                _logger.LogInformation("Image added to product {ProductId}", addImageDTO.ProductId);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Product image added successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding image to product {ProductId}", addImageDTO.ProductId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while adding the image: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> UpdateProductImageAsync(UpdateProductImageDTO updateImageDTO)
        {
            try
            {
                var image = await _productRepository.GetProductImageByIdAsync(updateImageDTO.Id);
                if (image == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product image with ID {updateImageDTO.Id} not found."
                    };
                }

                _imageMapper.MapToEntity(updateImageDTO, image);
                await _productRepository.UpdateProductImageAsync(image);

                // If marked as primary, set it
                if (updateImageDTO.IsPrimary.HasValue && updateImageDTO.IsPrimary.Value)
                {
                    await _productRepository.SetPrimaryImageAsync(image.Id);
                }

                _logger.LogInformation("Product image {ImageId} updated successfully", updateImageDTO.Id);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Product image updated successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product image {ImageId}", updateImageDTO.Id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while updating the image: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> RemoveProductImageAsync(int imageId)
        {
            try
            {
                var image = await _productRepository.GetProductImageByIdAsync(imageId);
                if (image == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product image with ID {imageId} not found."
                    };
                }

                await _productRepository.RemoveProductImageAsync(imageId);
                _logger.LogInformation("Product image {ImageId} removed successfully", imageId);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Product image removed successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing product image {ImageId}", imageId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while removing the image: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> SetPrimaryImageAsync(int imageId)
        {
            try
            {
                var image = await _productRepository.GetProductImageByIdAsync(imageId);
                if (image == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product image with ID {imageId} not found."
                    };
                }

                await _productRepository.SetPrimaryImageAsync(imageId);
                _logger.LogInformation("Product image {ImageId} set as primary", imageId);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Primary image set successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary image {ImageId}", imageId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while setting primary image: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDTO<ProductImageResponseDTO>>> GetProductImagesAsync(PaginationParametersDTO parametersDTO, int productId)
        {
            try
            {
                if (!await _productRepository.ProductExistsAsync(productId))
                {
                    return new ServiceResponse<PagedResultDTO<ProductImageResponseDTO>>
                    {
                        Success = false,
                        Message = $"Product with ID {productId} not found."
                    };
                }

                var images = await _productRepository.GetProductImagesAsync(
                    parametersDTO.PageNumber,
                    parametersDTO.PageSize,
                    productId);

                var imageDTOs = _imageMapper.MapToResponseDTOs(images.Data);
                var response = images.ToPagedResult(imageDTOs, parametersDTO);

                return new ServiceResponse<PagedResultDTO<ProductImageResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = "Product images retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product images for product {ProductId}", productId);
                return new ServiceResponse<PagedResultDTO<ProductImageResponseDTO>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving product images: {ex.Message}"
                };
            }
        }

        // ===== Category Management =====

        public async Task<ServiceResponse<bool>> AddCategoryToProductAsync(int productId, int categoryId)
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

                if (!await _categoryRepository.CategoryExistsAsync(categoryId))
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Category with ID {categoryId} not found."
                    };
                }

                await _productRepository.AddCategoryToProductAsync(productId, categoryId);
                _logger.LogInformation("Category {CategoryId} added to product {ProductId}", categoryId, productId);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Category added to product successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding category {CategoryId} to product {ProductId}", categoryId, productId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while adding category to product: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> RemoveCategoryFromProductAsync(int productId, int categoryId)
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

                await _productRepository.RemoveCategoryFromProductAsync(productId, categoryId);
                _logger.LogInformation("Category {CategoryId} removed from product {ProductId}", categoryId, productId);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Category removed from product successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing category {CategoryId} from product {ProductId}", categoryId, productId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while removing category from product: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> UpdateProductCategoriesAsync(int productId, List<int> categoryIds)
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

                // Validate all categories exist
                foreach (var categoryId in categoryIds)
                {
                    if (!await _categoryRepository.CategoryExistsAsync(categoryId))
                    {
                        return new ServiceResponse<bool>
                        {
                            Success = false,
                            Message = $"Category with ID {categoryId} not found."
                        };
                    }
                }

                await _productRepository.UpdateProductCategoriesAsync(productId, categoryIds);
                _logger.LogInformation("Categories updated for product {ProductId}", productId);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Product categories updated successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating categories for product {ProductId}", productId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while updating product categories: {ex.Message}"
                };
            }
        }

        // ===== Global Product Management =====

        public async Task<ServiceResponse<bool>> LinkProductToGlobalAsync(int productId, int globalProductId)
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

                if (!await _globalProductRepository.GlobalProductExistsAsync(globalProductId))
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Global product with ID {globalProductId} not found."
                    };
                }

                await _productRepository.LinkToGlobalProductAsync(productId, globalProductId);
                _logger.LogInformation("Product {ProductId} linked to global product {GlobalProductId}", productId, globalProductId);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Product linked to global product successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking product {ProductId} to global product {GlobalProductId}", productId, globalProductId);
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
                if (!await _productRepository.ProductExistsAsync(productId))
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Product with ID {productId} not found."
                    };
                }

                await _productRepository.UnlinkFromGlobalProductAsync(productId);
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

        // ===== Statistics =====

        public async Task<ServiceResponse<int>> GetTotalProductsCountAsync(bool includeDeleted = false)
        {
            try
            {
                var count = await _productRepository.GetTotalProductsCountAsync(includeDeleted);
                return new ServiceResponse<int>
                {
                    Success = true,
                    Data = count,
                    Message = "Total products count retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total products count");
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving count: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<int>> GetProductsByMarketCountAsync(int marketId)
        {
            try
            {
                var count = await _productRepository.GetProductsByMarketCountAsync(marketId);
                return new ServiceResponse<int>
                {
                    Success = true,
                    Data = count,
                    Message = "Products by market count retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by market count for market {MarketId}", marketId);
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving count: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<int>> GetProductsByStatusCountAsync(ProductStatus status)
        {
            try
            {
                var count = await _productRepository.GetProductsByStatusCountAsync(status);
                return new ServiceResponse<int>
                {
                    Success = true,
                    Data = count,
                    Message = $"Products with status {status} count retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by status count for status {Status}", status);
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving count: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<int>> GetAvailableProductsCountAsync()
        {
            try
            {
                var count = await _productRepository.GetAvailableProductsCountAsync();
                return new ServiceResponse<int>
                {
                    Success = true,
                    Data = count,
                    Message = "Available products count retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available products count");
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving count: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<double>> GetAveragePriceAsync()
        {
            try
            {
                var average = await _productRepository.GetAveragePriceAsync();
                return new ServiceResponse<double>
                {
                    Success = true,
                    Data = average,
                    Message = "Average price retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting average price");
                return new ServiceResponse<double>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving average price: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<double>> GetMinPriceAsync()
        {
            try
            {
                var min = await _productRepository.GetMinPriceAsync();
                return new ServiceResponse<double>
                {
                    Success = true,
                    Data = min,
                    Message = "Minimum price retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting minimum price");
                return new ServiceResponse<double>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving minimum price: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<double>> GetMaxPriceAsync()
        {
            try
            {
                var max = await _productRepository.GetMaxPriceAsync();
                return new ServiceResponse<double>
                {
                    Success = true,
                    Data = max,
                    Message = "Maximum price retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting maximum price");
                return new ServiceResponse<double>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving maximum price: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<Dictionary<ProductStatus, int>>> GetProductCountByStatusAsync()
        {
            try
            {
                var counts = await _productRepository.GetProductCountByStatusAsync();
                return new ServiceResponse<Dictionary<ProductStatus, int>>
                {
                    Success = true,
                    Data = counts,
                    Message = "Product count by status retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product count by status");
                return new ServiceResponse<Dictionary<ProductStatus, int>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving counts: {ex.Message}"
                };
            }
        }

        // ===== Utility =====

        public async Task<ServiceResponse<bool>> ProductExistsAsync(int id)
        {
            try
            {
                var exists = await _productRepository.ProductExistsAsync(id);
                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = exists,
                    Message = exists ? "Product exists." : "Product does not exist."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product exists: {ProductId}", id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while checking product existence: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> IsProductAvailableAsync(int id)
        {
            try
            {
                var isAvailable = await _productRepository.IsProductAvailableAsync(id);
                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = isAvailable,
                    Message = isAvailable ? "Product is available." : "Product is not available."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product is available: {ProductId}", id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while checking product availability: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> IsProductOwnedByMarketAsync(int productId, int marketId)
        {
            try
            {
                var isOwned = await _productRepository.IsProductOwnedByMarketAsync(productId, marketId);
                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = isOwned,
                    Message = isOwned ? "Product belongs to this market." : "Product does not belong to this market."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product {ProductId} belongs to market {MarketId}", productId, marketId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while checking product ownership: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<int>> GetProductImageCountAsync(int productId)
        {
            try
            {
                var count = await _productRepository.GetProductImageCountAsync(productId);
                return new ServiceResponse<int>
                {
                    Success = true,
                    Data = count,
                    Message = "Product image count retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product image count for product {ProductId}", productId);
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving image count: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<ProductRejectionResponseDTO>> GetLatestProductRejectionAsync(int productId)
        {
            try
            {
                if (!await _productRepository.ProductExistsAsync(productId))
                {
                    return new ServiceResponse<ProductRejectionResponseDTO>
                    {
                        Success = false,
                        Message = $"Product with ID {productId} not found."
                    };
                }

                var rejection = await _productRepository.GetLatestRejectionAsync(productId);
                if (rejection == null)
                {
                    return new ServiceResponse<ProductRejectionResponseDTO>
                    {
                        Success = true,
                        Message = "No rejection found for this product.",
                        Data = null
                    };
                }

                var response = _rejectionMapper.MapToResponseDTO(rejection);
                return new ServiceResponse<ProductRejectionResponseDTO>
                {
                    Success = true,
                    Data = response,
                    Message = "Latest product rejection retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest rejection for product {ProductId}", productId);
                return new ServiceResponse<ProductRejectionResponseDTO>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving rejection: {ex.Message}"
                };
            }
        }
    }
}
