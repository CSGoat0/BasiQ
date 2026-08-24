using BasiQDAL.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BasiQDAL.Database
{
    public class BasiQDbContext : IdentityDbContext<User>
    {
        public BasiQDbContext(DbContextOptions<BasiQDbContext> options) : base(options)
        { }

        public BasiQDbContext() { }

        // ===== DbSets =====
        public DbSet<Market> Markets { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<GlobalProduct> GlobalProducts { get; set; }
        public DbSet<ProductRejection> ProductRejections { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== Global Query Filters =====
            // Automatically filter out soft-deleted records
            modelBuilder.Entity<Market>().HasQueryFilter(m => !m.IsDeleted);
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<ProductImage>().HasQueryFilter(pi => !pi.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<GlobalProduct>().HasQueryFilter(gp => !gp.IsDeleted);
            modelBuilder.Entity<ProductRejection>().HasQueryFilter(pr => !pr.IsDeleted);

            // ===== Market Configuration =====
            modelBuilder.Entity<Market>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Name)
                    .HasMaxLength(200)
                    .IsRequired(false);

                entity.Property(m => m.Description)
                    .HasMaxLength(1000)
                    .IsRequired(false);

                entity.Property(m => m.AdminUserId)
                    .HasMaxLength(450)
                    .IsRequired(false);

                entity.Property(m => m.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                entity.Property(m => m.MaxProducts)
                    .IsRequired()
                    .HasDefaultValue(100);

                // Relationships
                entity.HasOne(m => m.AdminUser)
                    .WithMany()
                    .HasForeignKey(m => m.AdminUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(m => m.Products)
                    .WithOne(p => p.Market)
                    .HasForeignKey(p => p.MarketId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Product Configuration =====
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                    .HasMaxLength(200)
                    .IsRequired(false);

                entity.Property(p => p.Description)
                    .HasMaxLength(2000)
                    .IsRequired(false);

                entity.Property(p => p.BasePrice)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired(false);

                entity.Property(p => p.SalePrice)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired(false);

                entity.Property(p => p.Stock)
                    .IsRequired(false);

                entity.Property(p => p.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                // Relationships
                entity.HasOne(p => p.Market)
                    .WithMany(m => m.Products)
                    .HasForeignKey(p => p.MarketId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.GlobalProduct)
                    .WithMany(gp => gp.Products)
                    .HasForeignKey(p => p.GlobalProductId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(p => p.ProductImages)
                    .WithOne(pi => pi.Product)
                    .HasForeignKey(pi => pi.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Rejections)
                    .WithOne(pr => pr.Product)
                    .HasForeignKey(pr => pr.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Many-to-Many with Category (configured separately)
                entity.HasMany(p => p.Categories)
                    .WithMany(c => c.Products)
                    .UsingEntity<ProductCategory>(
                        j => j.HasOne(pc => pc.Category)
                            .WithMany()
                            .HasForeignKey(pc => pc.CategoryId)
                            .OnDelete(DeleteBehavior.Restrict),
                        j => j.HasOne(pc => pc.Product)
                            .WithMany()
                            .HasForeignKey(pc => pc.ProductId)
                            .OnDelete(DeleteBehavior.Cascade),
                        j =>
                        {
                            j.HasKey(pc => new { pc.ProductId, pc.CategoryId });
                            j.ToTable("ProductCategories");
                        });
            });

            // ===== ProductImage Configuration =====
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(pi => pi.Id);

                entity.Property(pi => pi.ImageUrl)
                    .HasMaxLength(500)
                    .IsRequired(false);

                entity.Property(pi => pi.IsPrimary)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.HasOne(pi => pi.Product)
                    .WithMany(p => p.ProductImages)
                    .HasForeignKey(pi => pi.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== Category Configuration =====
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Name)
                    .HasMaxLength(100)
                    .IsRequired(false);

                entity.Property(c => c.Description)
                    .HasMaxLength(500)
                    .IsRequired(false);

                entity.HasMany(c => c.Products)
                    .WithMany(p => p.Categories)
                    .UsingEntity<ProductCategory>(
                        j => j.HasOne(pc => pc.Product)
                            .WithMany()
                            .HasForeignKey(pc => pc.ProductId)
                            .OnDelete(DeleteBehavior.Cascade),
                        j => j.HasOne(pc => pc.Category)
                            .WithMany()
                            .HasForeignKey(pc => pc.CategoryId)
                            .OnDelete(DeleteBehavior.Restrict),
                        j =>
                        {
                            j.HasKey(pc => new { pc.ProductId, pc.CategoryId });
                            j.ToTable("ProductCategories");
                        });
            });

            // ===== GlobalProduct Configuration =====
            modelBuilder.Entity<GlobalProduct>(entity =>
            {
                entity.HasKey(gp => gp.Id);

                entity.Property(gp => gp.Name)
                    .HasMaxLength(200)
                    .IsRequired(false);

                entity.Property(gp => gp.Description)
                    .HasMaxLength(2000)
                    .IsRequired(false);

                entity.Property(gp => gp.PrimaryImageUrl)
                    .HasMaxLength(500)
                    .IsRequired(false);

                entity.HasMany(gp => gp.Products)
                    .WithOne(p => p.GlobalProduct)
                    .HasForeignKey(p => p.GlobalProductId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ===== ProductRejection Configuration =====
            modelBuilder.Entity<ProductRejection>(entity =>
            {
                entity.HasKey(pr => pr.Id);

                entity.Property(pr => pr.Reason)
                    .HasMaxLength(1000)
                    .IsRequired(false);

                entity.Property(pr => pr.RejectedByUserId)
                    .HasMaxLength(450)
                    .IsRequired(false);

                entity.HasOne(pr => pr.Product)
                    .WithMany(p => p.Rejections)
                    .HasForeignKey(pr => pr.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pr => pr.RejectedByUser)
                    .WithMany()
                    .HasForeignKey(pr => pr.RejectedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== ProductCategory Configuration (Explicit) =====
            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.HasKey(pc => new { pc.ProductId, pc.CategoryId });

                entity.HasOne(pc => pc.Product)
                    .WithMany()
                    .HasForeignKey(pc => pc.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pc => pc.Category)
                    .WithMany()
                    .HasForeignKey(pc => pc.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable("ProductCategories");
            });

            // ===== Indexes for Performance =====
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.MarketId);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Status);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.GlobalProductId);

            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.MarketId, p.Status });

            modelBuilder.Entity<Market>()
                .HasIndex(m => m.AdminUserId);

            modelBuilder.Entity<Market>()
                .HasIndex(m => m.Status);

            modelBuilder.Entity<ProductImage>()
                .HasIndex(pi => pi.ProductId);

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique(false);

            modelBuilder.Entity<ProductRejection>()
                .HasIndex(pr => pr.ProductId);

            modelBuilder.Entity<ProductRejection>()
                .HasIndex(pr => pr.RejectedByUserId);
        }
    }
}
