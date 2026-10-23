using Microsoft.EntityFrameworkCore;
using PizzaShop.Web.Domain;

namespace PizzaShop.Web.Data;

public class PizzaDbContext(DbContextOptions<PizzaDbContext> options) : DbContext(options)
{
    public DbSet<Pizza> Pizzas => Set<Pizza>();
    public DbSet<Topping> Toppings => Set<Topping>();
    public DbSet<PizzaTopping> PizzaToppings => Set<PizzaTopping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Topping>(topping =>
        {
            topping.ToTable("Toppings");
            topping.Property(t => t.Name).HasMaxLength(50).IsRequired();
            topping.HasIndex(t => t.Name).IsUnique();
            topping.Property(t => t.Category).HasConversion<string>().HasMaxLength(20);
            topping.Property(t => t.Price).HasPrecision(8, 2);
            topping.HasData(SeedData.Toppings);
        });

        modelBuilder.Entity<Pizza>(pizza =>
        {
            pizza.ToTable("Pizzas");
            pizza.Property(p => p.Name).HasMaxLength(60).IsRequired();
            pizza.Property(p => p.Notes).HasMaxLength(500);
            pizza.Property(p => p.Size).HasConversion<string>().HasMaxLength(20);
            pizza.Property(p => p.Crust).HasConversion<string>().HasMaxLength(20);
            pizza.Property(p => p.Sauce).HasConversion<string>().HasMaxLength(20);
            pizza.Property(p => p.Price).HasPrecision(8, 2);
            pizza.HasIndex(p => p.CreatedAt);

            pizza.HasMany(p => p.Toppings)
                .WithOne(pt => pt.Pizza)
                .HasForeignKey(pt => pt.PizzaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PizzaTopping>(pizzaTopping =>
        {
            pizzaTopping.ToTable("PizzaToppings");
            pizzaTopping.HasKey(pt => new { pt.PizzaId, pt.ToppingId });

            pizzaTopping.HasOne(pt => pt.Topping)
                .WithMany()
                .HasForeignKey(pt => pt.ToppingId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
