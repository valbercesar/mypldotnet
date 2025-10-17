/*
* PL/.NET Project
* valber.cesar@brickabode.com
* File: AppDbContext.cs
* 
* Natal, Oct, 15 2025
*/

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    // DbSet represents the entity table.
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Replace by a conn string
        var connectionString = "Host=localhost;Database=postgres;Username=postgres;Password=postgres";
        optionsBuilder.UseNpgsql(connectionString);
    }
}
