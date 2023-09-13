using Microsoft.EntityFrameworkCore;
using Minimal.Api.Entities;

namespace Minimal.Api.DbContext;

public class BooksContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<Book> Books => Set<Book>();

    private readonly IConfiguration _configuration;

    public BooksContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection")));
    }
}