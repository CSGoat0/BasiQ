using BasiQDAL.Enums;

namespace BasiQDAL.Entities
{
    public class Market : BaseEntity
    {
        public int Id { get; private set; }
        public string? Name { get; private set; }
        public string? Description { get; private set; }
        public string? AdminUserId { get; private set; }
        public bool IsTrusted { get; private set; } = false;
        public MarketStatus Status { get; private set; } = MarketStatus.Active;
        public int MaxProducts { get; private set; } = 100;

        // Navigation Properties
        public virtual User? AdminUser { get; private set; }
        public virtual ICollection<Product>? Products { get; private set; }

        protected Market() { }

        public Market(string? name, string? description, string? adminUserId, bool isTrusted = false)
        {
            Name = name;
            Description = description;
            AdminUserId = adminUserId;
            IsTrusted = isTrusted;
            Status = MarketStatus.Active;
            Products = new List<Product>();
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

        public void EditAdmin(string? adminUserId)
        {
            if (!string.IsNullOrWhiteSpace(adminUserId))
            {
                AdminUserId = adminUserId;
                UpdateTimestamp();
            }
        }

        public void SetTrusted(bool isTrusted)
        {
            IsTrusted = isTrusted;
            UpdateTimestamp();
        }

        public void UpdateStatus(MarketStatus status)
        {
            Status = status;
            UpdateTimestamp();
        }

        public void SetMaxProducts(int maxProducts)
        {
            if (maxProducts > 0)
            {
                MaxProducts = maxProducts;
                UpdateTimestamp();
            }
        }

        // ===== Product Management =====
        public int GetProductCount()
        {
            return Products?.Count(p => !p.IsDeleted) ?? 0;
        }

        public bool CanAddMoreProducts()
        {
            return GetProductCount() < MaxProducts;
        }

        public int GetRemainingProductSlots()
        {
            return Math.Max(0, MaxProducts - GetProductCount());
        }
    }
}
