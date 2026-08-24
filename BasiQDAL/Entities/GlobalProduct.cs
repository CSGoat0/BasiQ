namespace BasiQDAL.Entities
{
    public class GlobalProduct : BaseEntity
    {
        public int Id { get; private set; }
        public string? Name { get; private set; }
        public string? Description { get; private set; }
        public string? PrimaryImageUrl { get; private set; }

        // Navigation Properties - EF Core manages these
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
    }
}
