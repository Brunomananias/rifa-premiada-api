using API_Rifa.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;


namespace API_Rifa.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Raffle> Raffles { get; set; }
        public DbSet<NumberSold> Numbers_Sold { get; set; }
        public DbSet<PixTransaction> Pix_Transactions { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<Plan> Plans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PixTransaction>(entity =>
            {
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()"); // Valor padrão se necessário

                entity.Property(e => e.UpdatedAt)
                    .IsRequired(false); // Explícito que aceita NULL
            });

            modelBuilder.Entity<AdminUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

            modelBuilder.Entity<PixTransaction>()
          .HasOne(pt => pt.User)
          .WithMany()  // Supondo que a relação seja de 1 para N
          .HasForeignKey(pt => pt.UserId);
            modelBuilder.Entity<PixTransaction>()
    .Property(pt => pt.NumberSoldId)
    .HasColumnName("numberSoldId"); // Nome exato da coluna no BD
        }

    }

}
