using Inventory.API.Models;
using Inventory.API.Models.DTOs;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<LatexClients> latex_clients { get; set; }
    public DbSet<LatexStockIn> latex_stock_in { get; set; }
}
