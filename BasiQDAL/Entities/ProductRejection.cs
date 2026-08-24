namespace BasiQDAL.Entities
{
    public class ProductRejection : BaseEntity
    {
        public int Id { get; private set; }
        public int ProductId { get; private set; }
        public string? Reason { get; private set; }
        public string? RejectedByUserId { get; private set; }

        // Navigation Properties
        public virtual Product? Product { get; private set; }
        public virtual User? RejectedByUser { get; private set; }

        protected ProductRejection() { }

        public ProductRejection(int productId, string? reason, string? rejectedByUserId)
        {
            ProductId = productId;
            Reason = reason;
            RejectedByUserId = rejectedByUserId;
        }

        // ===== Edit Methods =====
        public void EditReason(string? reason)
        {
            if (!string.IsNullOrWhiteSpace(reason))
            {
                Reason = reason;
                UpdateTimestamp();
            }
        }
    }
}
