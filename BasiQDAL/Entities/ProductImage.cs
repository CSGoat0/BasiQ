namespace BasiQDAL.Entities
{
    public class ProductImage : BaseEntity
    {
        public int Id { get; private set; }
        public int ProductId { get; private set; }
        public string? ImageUrl { get; private set; }
        public bool IsPrimary { get; private set; } = false;

        // Navigation Properties
        public virtual Product? Product { get; private set; }

        protected ProductImage() { }

        public ProductImage(int productId, string? imageUrl, bool isPrimary = false)
        {
            ProductId = productId;
            ImageUrl = imageUrl;
            IsPrimary = isPrimary;
        }

        // ===== Edit Methods =====
        public void EditImageUrl(string? imageUrl)
        {
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                ImageUrl = imageUrl;
                UpdateTimestamp();
            }
        }

        public void SetAsPrimary()
        {
            IsPrimary = true;
            UpdateTimestamp();
        }

        public void UnsetPrimary()
        {
            IsPrimary = false;
            UpdateTimestamp();
        }
    }
}
