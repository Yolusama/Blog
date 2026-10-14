using Blog.EFCoreMigration;
using Blog.Infrastructure;
using Blog.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddDbContext<SysDbContext>(options =>
{
    var dbConnectionStr =  builder.Configuration.GetConnectionString(Constants.SySDb);
    options.UseMySql(dbConnectionStr,
        new MySqlServerVersion(new Version(9,4,0)),e=>e.MigrationsAssembly("Blog.EFCoreMigration"));
});

builder.Services.AddDbContext<MigrationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString(Constants.MigrationDb));
});

var app = builder.Build();

var rootPath = Path.Combine(app.Environment.ContentRootPath, "Root");
var rootProvider = new PhysicalFileProvider(rootPath);
app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = rootProvider
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = rootProvider
});

app.MapControllers();

app.Run();
