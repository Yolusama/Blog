using Blog.EFCoreMigration.Services.Interfaces;

namespace Blog.EFCoreMigration.Services;

public static class ExpansionService
{
    private const int LaunchDelay = 5000;
    public static void LaunchMigrationView(this WebApplication app)
    {
        app.Lifetime.ApplicationStarted.Register(async () =>
        {
            var url = app.Configuration.GetValue<string>("Urls");
            var dispatcher = app.Services.GetRequiredService<IProcessDispatcher>();
            await dispatcher.DispatchProcess(url,true);
        });
    }
}