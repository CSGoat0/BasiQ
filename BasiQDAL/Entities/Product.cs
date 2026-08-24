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

            // Validate sale price
            ValidateSalePrice();
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
                ValidateSalePrice();
            }
        }

        public void EditSalePrice(double? salePrice)
        {
            SalePrice = salePrice;
            UpdateTimestamp();
            ValidateSalePrice();
        }

        public void EditStock(int? stock)
        {
            if (stock.HasValue && stock >= 0)
            {
                var oldStock = Stock;
                Stock = stock;
                UpdateTimestamp();

                // Auto-update status based on stock
                AutoUpdateStatusBasedOnStock();
            }
        }

        // ===== Status Management =====
        public void UpdateStatus(ProductStatus status)
        {
            if (status == Status)
                return;

            var oldStatus = Status;
            Status = status;
            UpdateTimestamp();

            // If status is Rejected, don't auto-change
            // If status is Pending, remove any existing rejections (re-open for review)
            if (status == ProductStatus.Pending)
            {
                // Keep rejections for history but product is re-opened
            }
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

        private void ValidateSalePrice()
        {
            if (SalePrice.HasValue && BasePrice.HasValue && SalePrice > BasePrice)
            {
                SalePrice = BasePrice; // Reset to BasePrice if invalid
            }
        }

        // ===== Image Management =====
        public void AddImage(ProductImage image)
        {
            if (ProductImages == null)
                ProductImages = new List<ProductImage>();

            if (ProductImages.Count >= 5)
                throw new InvalidOperationException("Maximum 5 images allowed per product.");

            // If this is the first image, make it primary
            if (!ProductImages.Any() && !image.IsPrimary)
            {
                image.SetAsPrimary();
            }

            ProductImages.Add(image);
            UpdateTimestamp();
        }

        public void RemoveImage(int imageId)
        {
            var image = ProductImages?.FirstOrDefault(i => i.Id == imageId && !i.IsDeleted);
            if (image != null)
            {
                image.Delete();
                UpdateTimestamp();
            }
        }

        public ProductImage? GetPrimaryImage()
        {
            return ProductImages?.FirstOrDefault(i => i.IsPrimary && !i.IsDeleted);
        }

        // ===== Category Management =====
        public void AddCategory(Category category)
        {
            if (Categories == null)
                Categories = new List<Category>();

            if (!Categories.Any(c => c.Id == category.Id && !c.IsDeleted))
            {
                Categories.Add(category);
                UpdateTimestamp();
            }
        }

        public void RemoveCategory(int categoryId)
        {
            var category = Categories?.FirstOrDefault(c => c.Id == categoryId && !c.IsDeleted);
            if (category != null)
            {
                Categories.Remove(category);
                UpdateTimestamp();
            }
        }

        // ===== Global Product Linking =====
        public void LinkToGlobalProduct(GlobalProduct globalProduct)
        {
            GlobalProductId = globalProduct.Id;
            GlobalProduct = globalProduct;
            UpdateTimestamp();
        }

        public void UnlinkFromGlobalProduct()
        {
            GlobalProductId = null;
            GlobalProduct = null;
            UpdateTimestamp();
        }

        // ===== Business Logic =====
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

        public void AddRejection(ProductRejection rejection)
        {
            if (Rejections == null)
                Rejections = new List<ProductRejection>();

            Rejections.Add(rejection);
            UpdateTimestamp();
        }

        public ProductRejection? GetLatestRejection()
        {
            return Rejections?
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.RegistrationDate)
                .FirstOrDefault();
        }
    }
}
