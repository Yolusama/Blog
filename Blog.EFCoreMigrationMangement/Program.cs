using Blog.EFCoreMigration;
using Blog.EFCoreMigration.Services;
using Blog.EFCoreMigration.Services.Interfaces;
using Blog.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddControllers();

builder.Services.AddDbContext<MigrationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString(Constants.MigrationDb));
});

builder.Services.AddSingleton<IProcessDispatcher, ProcessDispatcher>();
builder.Services.AddScoped<IMigrationService, MigrationService>();

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

app.LaunchMigrationView();

app.Run();