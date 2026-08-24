using BasiQBLL.DTOs.ProductImageDTOs;
using BasiQDAL.Entities;

namespace BasiQBLL.Mapper
{
    public class ProductImageMapper
    {
        // ===== Response Mappings =====

        public ProductImageResponseDTO MapToResponseDTO(ProductImage productImage)
        {
            if (productImage == null) return null!;

            return new ProductImageResponseDTO
            {
                Id = productImage.Id,
                ProductId = productImage.ProductId,
                ImageUrl = productImage.ImageUrl,
                IsPrimary = productImage.IsPrimary,
                IsDeleted = productImage.IsDeleted,
                RegistrationDate = productImage.RegistrationDate,
                UpdatedOn = productImage.UpdatedOn
            };
        }

        public IEnumerable<ProductImageResponseDTO> MapToResponseDTOs(IEnumerable<ProductImage> productImages)
        {
            var result = new List<ProductImageResponseDTO>();
            foreach (var productImage in productImages)
            {
                result.Add(MapToResponseDTO(productImage));
            }
            return result;
        }

        // ===== Create Mappings =====

        public ProductImage MapToEntity(AddProductImageDTO dto)
        {
            if (dto == null) return null!;

            return new ProductImage(
                productId: dto.ProductId,
                imageUrl: dto.ImageUrl,
                isPrimary: dto.IsPrimary
            );
        }

        // ===== Update Mappings =====

        public ProductImage MapToEntity(UpdateProductImageDTO dto, ProductImage existingProductImage)
        {
            if (dto == null || existingProductImage == null) return existingProductImage!;

            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
                existingProductImage.EditImageUrl(dto.ImageUrl);

            if (dto.IsPrimary.HasValue)
            {
                if (dto.IsPrimary.Value)
                    existingProductImage.SetAsPrimary();
                else
                    existingProductImage.UnsetPrimary();
            }

            return existingProductImage;
        }
    }
}
