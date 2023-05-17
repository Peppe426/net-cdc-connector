using CDCworker.Models;
using Microsoft.Extensions.Options;
using MSSQLConnector.Interfaces;
using MSSQLConnector.Models;

namespace CDCConnector
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private ICDCConnector<DbEntity, RedisEntry> _cDCConnector;
        private AppSettings _appSettings;

        public Worker(ILogger<Worker> logger, ICDCConnector<DbEntity, RedisEntry> cDCConnector, IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _cDCConnector = cDCConnector;
            _appSettings = appSettings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitCDCConnector();
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task InitCDCConnector()
        {
            DbConfiguration dbConfiguration = new(
                _appSettings.DataBaseConfig.DataSource,
                _appSettings.DataBaseConfig.InitialCatalog,
                _appSettings.DataBaseConfig.Port,
                _appSettings.DataBaseConfig.UserId,
                _appSettings.DataBaseConfig.Password);
            RedisConfiguration redisConfiguration = new(
                _appSettings.StreamConfig.ConnectionString,
                _appSettings.StreamConfig.Port,
                _appSettings.StreamConfig.Key,
                _appSettings.StreamConfig.StreamField,
                _appSettings.StreamConfig.Password,
                _appSettings.StreamConfig.AbortOnConnectFail,
                _appSettings.StreamConfig.IncludeDetailInExceptions,
                _appSettings.StreamConfig.IncludePerformanceCountersInExceptions,
                _appSettings.StreamConfig.ConnectRetry,
                _appSettings.StreamConfig.DefaultDatabase,
                _appSettings.StreamConfig.SSL);

            _cDCConnector = await _cDCConnector.InitConnector(dbConfiguration, redisConfiguration);
        }
    }
}