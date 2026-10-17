using Blog.EFCoreMigration.Entity;
using Microsoft.EntityFrameworkCore;

namespace Blog.EFCoreMigration;

public class MigrationDbContext : DbContext
{
    public DbSet<Migration>  Migrations { get; set; }
    public MigrationDbContext(DbContextOptions<MigrationDbContext> options) : base(options)
    {
        
    }
    
}