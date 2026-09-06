using BasiQBLL.DTOs.RejectionDTOs;
using BasiQDAL.Entities;

namespace BasiQBLL.Mapper
{
    public class ProductRejectionMapper
    {
        // ===== Response Mappings =====

        public ProductRejectionResponseDTO MapToResponseDTO(ProductRejection productRejection)
        {
            if (productRejection == null) return null!;

            return new ProductRejectionResponseDTO
            {
                Id = productRejection.Id,
                ProductId = productRejection.ProductId,
                Reason = productRejection.Reason,
                RejectedByUserId = productRejection.RejectedByUserId,
                RejectedByUserName = productRejection.RejectedByUser?.FullName ??
                                     productRejection.RejectedByUser?.UserName ??
                                     string.Empty,
                IsDeleted = productRejection.IsDeleted,
                RegistrationDate = productRejection.RegistrationDate,
                UpdatedOn = productRejection.UpdatedOn
            };
        }

        public IEnumerable<ProductRejectionResponseDTO> MapToResponseDTOs(IEnumerable<ProductRejection> productRejections)
        {
            var result = new List<ProductRejectionResponseDTO>();
            foreach (var productRejection in productRejections)
            {
                result.Add(MapToResponseDTO(productRejection));
            }
            return result;
        }

        // ===== Create Mappings =====

        public ProductRejection MapToEntity(RejectProductDTO dto)
        {
            if (dto == null) return null!;

            return new ProductRejection(
                productId: dto.ProductId,
                reason: dto.Reason,
                rejectedByUserId: dto.RejectedByUserId
            );
        }
    }
}
