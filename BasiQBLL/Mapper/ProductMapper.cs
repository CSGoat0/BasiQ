using BasiQBLL.DTOs.CategoryDTOs;
using BasiQBLL.DTOs.GlobalProductDTOs;
using BasiQBLL.DTOs.ProductDTOs;
using BasiQBLL.DTOs.ProductImageDTOs;
using BasiQDAL.Entities;

namespace BasiQBLL.Mapper
{
    public class ProductMapper
    {
        private readonly CategoryMapper _categoryMapper;
        private readonly ProductImageMapper _productImageMapper;
        private readonly ProductRejectionMapper _productRejectionMapper;

        public ProductMapper()
        {
            _categoryMapper = new CategoryMapper();
            _productImageMapper = new ProductImageMapper();
            _productRejectionMapper = new ProductRejectionMapper();
        }

        // ===== Response Mappings =====

        public ProductResponseDTO MapToResponseDTO(Product product)
        {
            if (product == null) return null!;

            var primaryImage = product.ProductImages?.FirstOrDefault(i => i.IsPrimary && !i.IsDeleted);

            return new ProductResponseDTO
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                MarketId = product.MarketId,
                MarketName = product.Market?.Name ?? string.Empty,
                BasePrice = product.BasePrice,
                SalePrice = product.SalePrice,
                CurrentPrice = product.GetCurrentPrice(),
                IsOnSale = product.IsOnSale(),
                DiscountPercentage = product.GetDiscountPercentage(),
                Stock = product.Stock,
                IsInStock = product.IsInStock(),
                Status = product.Status,
                StatusDisplay = product.Status.ToString(),
                GlobalProductId = product.GlobalProductId,
                PrimaryImageUrl = primaryImage?.ImageUrl,
                ImageCount = product.ProductImages?.Count(i => !i.IsDeleted) ?? 0,
                CategoryNames = product.Categories?.Where(c => !c.IsDeleted).Select(c => c.Name).ToList() ?? new List<string>(),
                IsDeleted = product.IsDeleted,
                RegistrationDate = product.RegistrationDate,
                UpdatedOn = product.UpdatedOn,
                DeletedOn = product.DeletedOn
            };
        }

        public IEnumerable<ProductResponseDTO> MapToResponseDTOs(IEnumerable<Product> products)
        {
            var result = new List<ProductResponseDTO>();
            foreach (var product in products)
            {
                result.Add(MapToResponseDTO(product));
            }
            return result;
        }

        // ===== Details Mapping =====

        public ProductDetailsDTO MapToDetailsDTO(Product product)
        {
            if (product == null) return null!;

            return new ProductDetailsDTO
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                MarketId = product.MarketId,
                MarketName = product.Market?.Name ?? string.Empty,
                MarketAdminUserId = product.Market?.AdminUserId,
                IsMarketTrusted = product.Market?.IsTrusted ?? false,
                BasePrice = product.BasePrice,
                SalePrice = product.SalePrice,
                CurrentPrice = product.GetCurrentPrice(),
                IsOnSale = product.IsOnSale(),
                DiscountPercentage = product.GetDiscountPercentage(),
                Stock = product.Stock,
                IsInStock = product.IsInStock(),
                Status = product.Status,
                StatusDisplay = product.Status.ToString(),
                GlobalProductId = product.GlobalProductId,
                GlobalProduct = product.GlobalProduct != null ? new GlobalProductSummaryDTO
                {
                    Id = product.GlobalProduct.Id,
                    Name = product.GlobalProduct.Name,
                    PrimaryImageUrl = product.GlobalProduct.PrimaryImageUrl,
                    LinkedProductsCount = product.GlobalProduct.Products?.Count(p => !p.IsDeleted) ?? 0
                } : null,
                ProductImages = product.ProductImages?.Where(i => !i.IsDeleted)
                    .Select(i => _productImageMapper.MapToResponseDTO(i))
                    .ToList() ?? new List<ProductImageResponseDTO>(),
                Categories = product.Categories?.Where(c => !c.IsDeleted)
                    .Select(c => _categoryMapper.MapToResponseDTO(c))
                    .ToList() ?? new List<CategoryResponseDTO>(),
                LatestRejection = product.Rejections?
                    .Where(r => !r.IsDeleted)
                    .OrderByDescending(r => r.RegistrationDate)
                    .Select(r => _productRejectionMapper.MapToResponseDTO(r))
                    .FirstOrDefault(),
                IsDeleted = product.IsDeleted,
                RegistrationDate = product.RegistrationDate,
                UpdatedOn = product.UpdatedOn,
                DeletedOn = product.DeletedOn
            };
        }

        // ===== Create Mappings =====

        public Product MapToEntity(CreateProductDTO dto)
        {
            if (dto == null) return null!;

            return new Product(
                name: dto.Name,
                description: dto.Description,
                marketId: dto.MarketId,
                basePrice: dto.BasePrice,
                salePrice: dto.SalePrice,
                stock: dto.Stock ?? 0
            );
        }

        // ===== Update Mappings =====

        public Product MapToEntity(UpdateProductDTO dto, Product existingProduct)
        {
            if (dto == null || existingProduct == null) return existingProduct!;

            if (!string.IsNullOrWhiteSpace(dto.Name))
                existingProduct.EditName(dto.Name);

            if (!string.IsNullOrWhiteSpace(dto.Description))
                existingProduct.EditDescription(dto.Description);

            if (dto.BasePrice.HasValue)
                existingProduct.EditBasePrice(dto.BasePrice.Value);

            if (dto.SalePrice.HasValue)
                existingProduct.EditSalePrice(dto.SalePrice.Value);

            if (dto.Stock.HasValue)
                existingProduct.EditStock(dto.Stock.Value);

            return existingProduct;
        }

        // ===== Status Mapping =====

        public Product MapToEntity(UpdateProductStatusDTO dto, Product existingProduct)
        {
            if (dto == null || existingProduct == null) return existingProduct!;

            existingProduct.UpdateStatus(dto.Status);
            return existingProduct;
        }

        // ===== Stock Mapping =====

        public Product MapToEntity(UpdateProductStockDTO dto, Product existingProduct)
        {
            if (dto == null || existingProduct == null) return existingProduct!;

            existingProduct.EditStock(dto.Stock);
            return existingProduct;
        }
    }
}
