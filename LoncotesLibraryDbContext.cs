using Microsoft.EntityFrameworkCore;
using Library.Models;

public class LoncotesLibraryDbContext : DbContext
{
    public DbSet<Material> Materials { get; set; }
    public DbSet<Patron> Patrons { get; set; }
    public DbSet<MaterialType> MaterialTypes { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Checkout> Checkouts { get; set; }

    public LoncotesLibraryDbContext(DbContextOptions<LoncotesLibraryDbContext> context) : base(context)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // seed data with at least 10 materials
        modelBuilder.Entity<Material>().HasData(new Material[]
        {
            new Material { Id = 1, MaterialName = "Oliver Twist", MaterialTypeId = 1, GenreId = 2, OutOfCirculationSince = DateTime.Now.AddDays(-10) },
            new Material { Id = 2, MaterialName = "The Great Gatsby", MaterialTypeId = 1, GenreId = 3, OutOfCirculationSince = null },
            new Material { Id = 3, MaterialName = "1984", MaterialTypeId = 1, GenreId = 1, OutOfCirculationSince = null },
            new Material { Id = 4, MaterialName = "The Lord Of The Rings", MaterialTypeId = 1, GenreId = 5, OutOfCirculationSince = null },
            new Material { Id = 5, MaterialName = "Harry Potter and the Sorcerers Stone", MaterialTypeId = 1, GenreId = 4, OutOfCirculationSince = DateTime.Now.AddDays(-32) },
            new Material { Id = 6, MaterialName = "Abbey Road", MaterialTypeId = 2, GenreId = 2, OutOfCirculationSince = DateTime.Now.AddDays(-10) },
            new Material { Id = 7, MaterialName = "The Dark Side of the Moon", MaterialTypeId = 2, GenreId = 2, OutOfCirculationSince = null },
            new Material { Id = 8, MaterialName = "The Shawshank Redemption", MaterialTypeId = 3, GenreId = 1, OutOfCirculationSince = DateTime.Now.AddDays(-15) },
            new Material { Id = 9, MaterialName = "Inception", MaterialTypeId = 3, GenreId = 3, OutOfCirculationSince = DateTime.Now.AddDays(-20) },
            new Material { Id = 10, MaterialName = "The Dark Knight", MaterialTypeId = 3, GenreId = 5, OutOfCirculationSince = DateTime.Now.AddDays(-25) },

        });

        // seed data with at least 2 patrons 
        modelBuilder.Entity<Patron>().HasData(new Patron[]
        {
            new Patron { Id = 1, FirstName = "Peter", LastName = "Parker", Email = "Spiderman@example.com", Address = "123 Main Street", IsActive = true },
            new Patron { Id = 2, FirstName = "Tony", LastName = "Stark", Email = "IronMan@example.com", Address = "456 Oak Avenue", IsActive = true }

        });

       // seed data with at least 3 material types
        modelBuilder.Entity<MaterialType>().HasData(new MaterialType[]
        {
            new MaterialType { Id = 1, Name = "Book", CheckoutDays = 14 },
            new MaterialType { Id = 2, Name = "CD", CheckoutDays = 7 },
            new MaterialType { Id = 3, Name = "DVD", CheckoutDays = 10 }

        });

        // seed data with at least 5 genres
        modelBuilder.Entity<Genre>().HasData(new Genre[]
        {
            new Genre { Id = 1, Name = "Fiction" },
            new Genre { Id = 2, Name = "Classics" },
            new Genre { Id = 3, Name = "Drama" },
            new Genre { Id = 4, Name = "Fantasy" },
            new Genre { Id = 5, Name = "Adventure" }

        });

        // seed data with checkouts (none needed initially)
        modelBuilder.Entity<Checkout>().HasData(new Checkout[]
        {
    
        });

    }
}