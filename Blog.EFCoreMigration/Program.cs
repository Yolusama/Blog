using Blog.Infrastructure;
using Blog.Shared;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SysDbContext>(options =>
{
    var dbConnectionStr =  builder.Configuration.GetConnectionString(Constants.SySDb);
    options.UseMySql(dbConnectionStr,
        new MySqlServerVersion(new Version(9,4,0)),
        e=>e.MigrationsAssembly("Blog.EFCoreMigration"));
});


var app = builder.Build();

app.MapControllers();


app.Run();
