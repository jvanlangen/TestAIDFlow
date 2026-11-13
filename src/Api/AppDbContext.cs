using Microsoft.EntityFrameworkCore;

namespace Api;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<PackageSize> PackageSizes => Set<PackageSize>();
    public DbSet<RecipeMeasure> RecipeMeasures => Set<RecipeMeasure>();
    public DbSet<IngredientMeasureEquivalent> IngredientMeasureEquivalents => Set<IngredientMeasureEquivalent>();
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<MealIngredient> MealIngredients => Set<MealIngredient>();
    public DbSet<GroceryList> GroceryLists => Set<GroceryList>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.BaseUnit).HasMaxLength(12).IsRequired();
        });

        modelBuilder.Entity<PackageSize>(entity =>
        {
            entity.Property(x => x.Quantity).HasColumnType("numeric").IsRequired();
            entity.Property(x => x.Unit).HasMaxLength(12);
            entity.HasOne<Ingredient>().WithMany(x => x.PackageSizes)
                .HasForeignKey(x => x.IngredientId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<RecipeMeasure>().WithMany()
                .HasForeignKey(x => x.RecipeMeasureId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RecipeMeasure>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(80).IsRequired();
        });

        modelBuilder.Entity<IngredientMeasureEquivalent>(entity =>
        {
            entity.HasKey(x => new { x.IngredientId, x.RecipeMeasureId });
            entity.Property(x => x.Quantity).HasColumnType("numeric").IsRequired();
            entity.Property(x => x.Unit).HasMaxLength(12).IsRequired();
            entity.HasOne<Ingredient>().WithMany(x => x.MeasureEquivalents)
                .HasForeignKey(x => x.IngredientId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<RecipeMeasure>().WithMany(x => x.Equivalents)
                .HasForeignKey(x => x.RecipeMeasureId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Meal>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
        });

        modelBuilder.Entity<MealIngredient>(entity =>
        {
            entity.Property(x => x.Quantity).HasColumnType("numeric").IsRequired();
            entity.Property(x => x.Unit).HasMaxLength(12);
            entity.HasOne<Meal>().WithMany(x => x.Ingredients)
                .HasForeignKey(x => x.MealId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Ingredient>().WithMany()
                .HasForeignKey(x => x.IngredientId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<RecipeMeasure>().WithMany()
                .HasForeignKey(x => x.RecipeMeasureId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GroceryList>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.SelectionsJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.ItemsJson).HasColumnType("jsonb").IsRequired();
        });
    }
}
