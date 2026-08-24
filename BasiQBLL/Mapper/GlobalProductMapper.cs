using BasiQBLL.DTOs.GlobalProductDTOs;
using BasiQDAL.Entities;

namespace BasiQBLL.Mapper
{
    public class GlobalProductMapper
    {
        private readonly ProductMapper _productMapper;

        public GlobalProductMapper()
        {
            _productMapper = new ProductMapper();
        }

        // ===== Response Mappings =====

        public GlobalProductResponseDTO MapToResponseDTO(GlobalProduct globalProduct)
        {
            if (globalProduct == null) return null!;

            return new GlobalProductResponseDTO
            {
                Id = globalProduct.Id,
                Name = globalProduct.Name,
                Description = globalProduct.Description,
                PrimaryImageUrl = globalProduct.PrimaryImageUrl,
                LinkedProductsCount = globalProduct.Products?.Count(p => !p.IsDeleted) ?? 0,
                LinkedProducts = globalProduct.Products?
                    .Where(p => !p.IsDeleted)
                    .Select(p => new LinkedProductSummaryDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        MarketId = p.MarketId,
                        MarketName = p.Market?.Name ?? string.Empty,
                        CurrentPrice = p.GetCurrentPrice(),
                        IsInStock = p.IsInStock(),
                        PrimaryImageUrl = p.ProductImages?.FirstOrDefault(i => i.IsPrimary && !i.IsDeleted)?.ImageUrl
                    })
                    .ToList() ?? new List<LinkedProductSummaryDTO>(),
                IsDeleted = globalProduct.IsDeleted,
                RegistrationDate = globalProduct.RegistrationDate,
                UpdatedOn = globalProduct.UpdatedOn,
                DeletedOn = globalProduct.DeletedOn
            };
        }

        public IEnumerable<GlobalProductResponseDTO> MapToResponseDTOs(IEnumerable<GlobalProduct> globalProducts)
        {
            var result = new List<GlobalProductResponseDTO>();
            foreach (var globalProduct in globalProducts)
            {
                result.Add(MapToResponseDTO(globalProduct));
            }
            return result;
        }

        // ===== Create Mappings =====

        public GlobalProduct MapToEntity(CreateGlobalProductDTO dto)
        {
            if (dto == null) return null!;

            return new GlobalProduct(
                name: dto.Name,
                description: dto.Description,
                primaryImageUrl: dto.PrimaryImageUrl
            );
        }

        // ===== Update Mappings =====

        public GlobalProduct MapToEntity(UpdateGlobalProductDTO dto, GlobalProduct existingGlobalProduct)
        {
            if (dto == null || existingGlobalProduct == null) return existingGlobalProduct!;

            if (!string.IsNullOrWhiteSpace(dto.Name))
                existingGlobalProduct.EditName(dto.Name);

            if (!string.IsNullOrWhiteSpace(dto.Description))
                existingGlobalProduct.EditDescription(dto.Description);

            if (!string.IsNullOrWhiteSpace(dto.PrimaryImageUrl))
                existingGlobalProduct.EditPrimaryImageUrl(dto.PrimaryImageUrl);

            return existingGlobalProduct;
        }

        // ===== Statistics Mapping =====

        public GlobalProductStatisticsDTO MapToStatisticsDTO(
            int totalGlobalProducts,
            int totalLinkedProducts,
            int unlinkedProducts,
            int globalProductsWithNoLinks,
            double averageProductsPerGlobalProduct,
            GlobalProductResponseDTO? mostLinkedGlobalProduct)
        {
            return new GlobalProductStatisticsDTO
            {
                TotalGlobalProducts = totalGlobalProducts,
                TotalLinkedProducts = totalLinkedProducts,
                UnlinkedProducts = unlinkedProducts,
                GlobalProductsWithNoLinks = globalProductsWithNoLinks,
                AverageProductsPerGlobalProduct = averageProductsPerGlobalProduct,
                MostLinkedGlobalProduct = mostLinkedGlobalProduct,
                StatisticsDate = DateTime.UtcNow
            };
        }
    }
}
