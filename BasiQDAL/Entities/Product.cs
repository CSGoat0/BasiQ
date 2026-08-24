using BasiQDAL.Enums;

namespace BasiQDAL.Entities
{
    public class Product : BaseEntity
    {
        public int Id { get; private set; }
        public string? Name { get; private set; }
        public string? Description { get; private set; }
        public int MarketId { get; private set; }
        public double? BasePrice { get; private set; }
        public double? SalePrice { get; private set; }
        public int? Stock { get; private set; }
        public ProductStatus Status { get; private set; } = ProductStatus.Pending;
        public int? GlobalProductId { get; private set; }

        // Navigation Properties
        public virtual Market? Market { get; private set; }
        public virtual GlobalProduct? GlobalProduct { get; private set; }
        public virtual ICollection<ProductImage>? ProductImages { get; private set; }
        public virtual ICollection<Category>? Categories { get; private set; }
        public virtual ICollection<ProductRejection>? Rejections { get; private set; }

        protected Product() { }

        public Product(
            string? name,
            string? description,
            int marketId,
            double? basePrice,
            double? salePrice = null,
            int? stock = 0)
        {
            Name = name;
            Description = description;
            MarketId = marketId;
            BasePrice = basePrice;
            SalePrice = salePrice;
            Stock = stock;
            Status = ProductStatus.Pending;
            ProductImages = new List<ProductImage>();
            Categories = new List<Category>();
            Rejections = new List<ProductRejection>();
        }

        // ===== Edit Methods =====
        public void EditName(string? name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                Name = name;
                UpdateTimestamp();
            }
        }

        public void EditDescription(string? description)
        {
            if (!string.IsNullOrWhiteSpace(description))
            {
                Description = description;
                UpdateTimestamp();
            }
        }

        public void EditBasePrice(double? basePrice)
        {
            if (basePrice.HasValue && basePrice >= 0)
            {
                BasePrice = basePrice;
                UpdateTimestamp();
            }
        }

        public void EditSalePrice(double? salePrice)
        {
            SalePrice = salePrice;
            UpdateTimestamp();
        }

        public void EditStock(int? stock)
        {
            if (stock.HasValue && stock >= 0)
            {
                Stock = stock;
                UpdateTimestamp();
                AutoUpdateStatusBasedOnStock();
            }
        }

        // ===== Status Methods =====
        public void UpdateStatus(ProductStatus status)
        {
            if (status == Status)
                return;

            Status = status;
            UpdateTimestamp();
        }

        public void Approve()
        {
            if (Status == ProductStatus.Pending)
            {
                Status = ProductStatus.Available;
                UpdateTimestamp();
            }
        }

        public void Reject()
        {
            if (Status == ProductStatus.Pending)
            {
                Status = ProductStatus.Rejected;
                UpdateTimestamp();
            }
        }

        public void Discontinue()
        {
            if (Status == ProductStatus.Available || Status == ProductStatus.OutOfStock)
            {
                Status = ProductStatus.Discontinued;
                UpdateTimestamp();
            }
        }

        public void ReopenForReview()
        {
            if (Status == ProductStatus.Rejected)
            {
                Status = ProductStatus.Pending;
                UpdateTimestamp();
            }
        }

        // ===== Private Helpers =====
        private void AutoUpdateStatusBasedOnStock()
        {
            if (Status == ProductStatus.Available || Status == ProductStatus.OutOfStock)
            {
                if (Stock.HasValue && Stock > 0)
                {
                    Status = ProductStatus.Available;
                }
                else if (Stock.HasValue && Stock == 0)
                {
                    Status = ProductStatus.OutOfStock;
                }
                UpdateTimestamp();
            }
        }

        // ===== Business Logic Methods =====
        public double? GetCurrentPrice()
        {
            return SalePrice ?? BasePrice;
        }

        public bool IsInStock()
        {
            return Stock.HasValue && Stock > 0 && Status == ProductStatus.Available;
        }

        public bool IsOnSale()
        {
            return SalePrice.HasValue && BasePrice.HasValue && SalePrice < BasePrice;
        }

        public double? GetDiscountPercentage()
        {
            if (!IsOnSale() || !BasePrice.HasValue || BasePrice == 0)
                return null;

            return Math.Round(((BasePrice.Value - SalePrice!.Value) / BasePrice.Value) * 100, 2);
        }
    }
}
