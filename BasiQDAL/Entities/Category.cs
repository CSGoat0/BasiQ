namespace BasiQDAL.Entities
{
    public class Category : BaseEntity
    {
        public int Id { get; private set; }
        public string? Name { get; private set; }
        public string? Description { get; private set; }

        // Navigation Properties
        public virtual ICollection<Product>? Products { get; private set; }

        protected Category() { }

        public Category(string? name, string? description = null)
        {
            Name = name;
            Description = description;
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
    }
}
