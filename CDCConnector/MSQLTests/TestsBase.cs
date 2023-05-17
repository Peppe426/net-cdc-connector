using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSQLConnectorTests.Models;
using MSSQLConnector;
using TestContext = MSQLConnectorTests.Models.TestContext;

namespace MSQLConnectorTests
{
    public class TestsBase : IDisposable
    {
        protected TestContext Context = default!;
        protected CDCConnector<TestEntity, TestEvent> CDCConnector = default;
        private IHost _host;
        private ILogger<CDCConnector<TestEntity, TestEvent>> _logger;

        [OneTimeSetUp]
        public void Init()
        {
            SetupHostBuild();
            SetupDatabase();
            InitConnector();
        }

        private void SetupHostBuild()
        {
            //https://www.christianfindlay.com/blog/ilogger
            //https://github.com/dotnet/AspNetCore.Docs/issues/21469
            var hostBuilder = Host.CreateDefaultBuilder().ConfigureLogging((builderContext, loggingBuilder) =>
            {
                loggingBuilder.AddSimpleConsole((options) =>
                {
                    options.IncludeScopes = true;
                });
            });

            _host = hostBuilder.Build();
            _logger = _host.Services.GetRequiredService<ILogger<CDCConnector<TestEntity, TestEvent>>>();
        }

        private void SetupDatabase()
        {
            string cs = @"data source=localhost;initial catalog=deleteme;persist security info=True;Integrated Security=SSPI;TrustServerCertificate=True;";

            SqlConnection connection = new SqlConnection(cs);

            //var connection = new SqliteConnection(cs);
            //connection.Open();

            var options = new DbContextOptionsBuilder<TestContext>().UseSqlServer(connection).Options;
            //.UseSqlite(connection).Options

            try
            {
                Context = new TestContext(options);
                Context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void InitConnector()
        {
            CDCConnector = new CDCConnector<TestEntity, TestEvent>(_logger);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}