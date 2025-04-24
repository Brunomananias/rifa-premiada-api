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
        public DbSet<PixConfig> PixConfigs { get; set; }
        public DbSet<RifaPixAssociation> RifaPixAssociations { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<NumberSold>()
                .HasMany(ns => ns.Pix_Transactions)
                .WithOne(pt => pt.Number_Sold)
                .HasForeignKey(pt => pt.Number_Sold_Id)
                .OnDelete(DeleteBehavior.Cascade); // <- isso aqui
            modelBuilder.Entity<RifaPixAssociation>()
      .HasKey(r => new { r.RaffleId, r.PixConfigId });

            modelBuilder.Entity<RifaPixAssociation>()
                .HasOne(r => r.Rifa)
                .WithMany()
                .HasForeignKey(r => r.RaffleId);

            modelBuilder.Entity<RifaPixAssociation>()
                .HasOne(r => r.PixConfig)
                .WithMany()
                .HasForeignKey(r => r.PixConfigId);
        }

    }

}
