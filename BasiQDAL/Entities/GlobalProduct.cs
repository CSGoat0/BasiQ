namespace BasiQDAL.Entities
{
    public class GlobalProduct : BaseEntity
    {
        public int Id { get; private set; }
        public string? Name { get; private set; }
        public string? Description { get; private set; }
        public string? PrimaryImageUrl { get; private set; }

        // Navigation Properties
        public virtual ICollection<Product>? Products { get; private set; }

        protected GlobalProduct() { }

        public GlobalProduct(string? name, string? description = null, string? primaryImageUrl = null)
        {
            Name = name;
            Description = description;
            PrimaryImageUrl = primaryImageUrl;
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

        public void EditPrimaryImageUrl(string? primaryImageUrl)
        {
            if (!string.IsNullOrWhiteSpace(primaryImageUrl))
            {
                PrimaryImageUrl = primaryImageUrl;
                UpdateTimestamp();
            }
        }

        // ===== Product Management =====
        public void AddProduct(Product product)
        {
            if (Products == null)
                Products = new List<Product>();

            if (!Products.Any(p => p.Id == product.Id && !p.IsDeleted))
            {
                Products.Add(product);
                product.LinkToGlobalProduct(this);
                UpdateTimestamp();
            }
        }

        public void RemoveProduct(int productId)
        {
            var product = Products?.FirstOrDefault(p => p.Id == productId && !p.IsDeleted);
            if (product != null)
            {
                Products.Remove(product);
                product.UnlinkFromGlobalProduct();
                UpdateTimestamp();
            }
        }

        public int GetLinkedProductsCount()
        {
            return Products?.Count(p => !p.IsDeleted) ?? 0;
        }
    }
}
