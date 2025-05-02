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
        public DbSet<PixTransactionAdmin> Pix_TransactionsAdmin { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<GatewaySettings> GatewaySettings { get; set; }
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

            modelBuilder.Entity<User>()
                .HasOne(u => u.Plano)
                .WithMany()
                .HasForeignKey(u => u.Plan_id);

            modelBuilder.Entity<AdminUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

            modelBuilder.Entity<PixTransaction>()
          .HasOne(pt => pt.Customer)
          .WithMany()  // Supondo que a relação seja de 1 para N
          .HasForeignKey(pt => pt.CustomerId);
            modelBuilder.Entity<PixTransaction>()
    .Property(pt => pt.NumberSoldId)
    .HasColumnName("numberSoldId"); // Nome exato da coluna no BD
        }

    }

}
