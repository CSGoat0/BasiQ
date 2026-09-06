namespace BasiQDAL.Entities
{
    public class ProductCategory
    {
        public int ProductId { get; private set; }
        public int CategoryId { get; private set; }

        // Navigation Properties
        public virtual Product? Product { get; private set; }
        public virtual Category? Category { get; private set; }

        protected ProductCategory() { }

        public ProductCategory(int productId, int categoryId)
        {
            ProductId = productId;
            CategoryId = categoryId;
        }
    }
}
