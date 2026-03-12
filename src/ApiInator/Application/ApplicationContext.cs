using ApiInator.Model;
using Microsoft.EntityFrameworkCore;

namespace ApiInator.Application;

public class ApplicationContext  : DbContext
{
   public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
   {
   }
   protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<GameInfo>(entity =>
    {
        entity.HasKey(e => e.Id);

        entity.HasOne(e => e.SteamInfo).WithOne().HasForeignKey<SteamInfo>(s => s.Id).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(e => e.GgDeals).WithOne().HasForeignKey<GgDealsInfo>(g => g.Id).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(e => e.OpenCritic).WithOne().HasForeignKey<OpenCriticInfo>(o => o.Id).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(e => e.HowLongToBeat).WithOne().HasForeignKey<HowLongToBeatInfo>(h => h.Id).OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<Currency>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.CurrencyCode).IsRequired().HasMaxLength(3);
    });

    modelBuilder.Entity<SteamInfo>(entity =>
    {
        entity.HasKey(e => e.Id);
        
        entity.OwnsOne(e => e.InitialPrice, p => 
        {
            p.Property(pr => pr.Value).HasColumnName("InitialPrice_Value");
            p.HasOne(pr => pr.Currency)
             .WithMany()
             .HasForeignKey("InitialPrice_CurrencyId")
             .OnDelete(DeleteBehavior.Restrict);
        });
    });

    modelBuilder.Entity<GgDealsInfo>(entity =>
    {
        entity.HasKey(e => e.Id);

        entity.OwnsOne(e => e.CurrentRetailPrice, p => 
        {
            p.Property(pr => pr.Value).HasColumnName("CurrentRetailPrice_Value");
            p.HasOne(pr => pr.Currency)
             .WithMany()
             .HasForeignKey("CurrentRetailPrice_CurrencyId")
             .OnDelete(DeleteBehavior.Restrict);
        });

        entity.OwnsOne(e => e.CurrentKeyshopPrice, p => 
        {
            p.Property(pr => pr.Value).HasColumnName("CurrentKeyshopPrice_Value");
            p.HasOne(pr => pr.Currency)
             .WithMany()
             .HasForeignKey("CurrentKeyshopPrice_CurrencyId")
             .OnDelete(DeleteBehavior.Restrict);
        });

        entity.OwnsOne(e => e.HistoricalRetailPrice, p => 
        {
            p.Property(pr => pr.Value).HasColumnName("HistoricalRetailPrice_Value");
            p.HasOne(pr => pr.Currency)
             .WithMany()
             .HasForeignKey("HistoricalRetailPrice_CurrencyId")
             .OnDelete(DeleteBehavior.Restrict);
        });

        entity.OwnsOne(e => e.HistoricalKeyshopPrice, p => 
        {
            p.Property(pr => pr.Value).HasColumnName("HistoricalKeyshopPrice_Value");
            p.HasOne(pr => pr.Currency)
             .WithMany()
             .HasForeignKey("HistoricalKeyshopPrice_CurrencyId")
             .OnDelete(DeleteBehavior.Restrict);
        });
    });

    modelBuilder.Entity<OpenCriticInfo>().HasKey(e => e.Id);
    modelBuilder.Entity<HowLongToBeatInfo>().HasKey(e => e.Id);
}
}