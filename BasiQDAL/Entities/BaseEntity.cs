namespace BasiQDAL.Entities
{
    public abstract class BaseEntity
    {
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedOn { get; private set; }
        public DateTime? UpdatedOn { get; private set; }
        public DateTime? RegistrationDate { get; private set; } = DateTime.Now;

        public virtual void Delete()
        {
            IsDeleted = true;
            DeletedOn = DateTime.Now;
        }

        public virtual void Restore()
        {
            IsDeleted = false;
            UpdatedOn = DateTime.Now;
        }

        protected void UpdateTimestamp()
        {
            UpdatedOn = DateTime.Now;
        }
    }
}
