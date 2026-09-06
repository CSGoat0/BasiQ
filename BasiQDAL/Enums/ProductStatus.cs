namespace BasiQDAL.Enums
{
    public enum ProductStatus
    {
        Pending,      // Waiting for approval (untrusted markets)
        Available,    // Approved, in stock, visible
        OutOfStock,   // Approved, stock = 0
        Discontinued, // No longer sold
        Rejected      // SuperAdmin rejected
    }
}
