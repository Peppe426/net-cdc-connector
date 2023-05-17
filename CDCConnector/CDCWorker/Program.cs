using CDCConnector;
using CDCworker.Models;
using MSSQLConnector;
using MSSQLConnector.Interfaces;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((Action<HostBuilderContext, IServiceCollection>)((hostContext, services) =>
    {
        AppSettings appSettings = InitAppsettings(hostContext, services);

        services.AddSingleton(appSettings);

        services.AddScoped<ICDCConnector<DbEntity, RedisEntry>, CDCConnector<DbEntity, RedisEntry>>();
        services.AddHostedService<Worker>();
    }))
    .Build();

host.Run();

static AppSettings InitAppsettings(HostBuilderContext hostContext, IServiceCollection services)
{

    //https://stackoverflow.com/questions/58183920/how-to-setup-app-settings-in-a-net-core-3-worker-service

    IConfiguration configuration = hostContext.Configuration;
    //snygga upp det fungerar nu men går inte att initiera en worker troligen för att redis inte är uppe ---> debugga
    var appSettingsSection = configuration.GetSection(AppSettings.SectionName).Get<AppSettings>();
    //services.Configure<AppSettings>(appSettingsSection);
    //var appSettings = appSettingsSection.Get<AppSettings>();


    //WorkerOptions options = hostContext.Configuration.GetSection("WCF").Get<WorkerOptions>();


    if (appSettingsSection is null)
    {
        throw new ArgumentException(nameof(AppSettings));
    }
    return appSettingsSection;
}