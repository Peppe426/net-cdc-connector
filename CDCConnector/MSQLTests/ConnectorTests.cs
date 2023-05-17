using Dapper;

namespace MSQLTests
{
    public class ConnectorTests : TestsBase
    {
        [OneTimeSetUp]
        public async Task InitSetup()
        {
            RedisConfiguration redisConfiguration = new("127.0.0.1", 6379, "test", "message", null, false, true, true, 0, 0, false);
            string cs = @"data source=localhost;initial catalog=deleteme;persist security info=True;Integrated Security=SSPI;TrustServerCertificate=True;";

            await CDCConnector.InitConnector(null, redisConfiguration!, cs);
            CDCConnector.SqlConnection.Should().NotBeNull();
            CDCConnector.RedisConnection.Should().NotBeNull();
        }

        [Test, Order(1)]
        public async Task ShouldInitializeCDCConnector()
        {
            //Given InitSetup

            //When InitSetup

            //Then
            CDCConnector.SqlConnection.Should().NotBeNull();
            CDCConnector.RedisConnection.Should().NotBeNull();
        }

        [Test, Order(2)]
        public async Task ShouldEnableCDCForDatabase()
        {
            //https://learn.microsoft.com/en-us/sql/relational-databases/track-changes/enable-and-disable-change-data-capture-sql-server?view=sql-server-ver16
            //Given InitSetup
            var dbname = CDCConnector!.SqlConnection!.Database;
            string query = @$"USE {dbname} EXEC sys.sp_cdc_enable_db";

            //When
            try
            {
                CDCConnector.SqlConnection.Query(query);
            }
            catch (Exception ex)
            {
                //Then
                Assert.Fail(ex.Message);
            }
            //Then
            Assert.Pass();
        }

        [Test, Order(3)]
        public async Task ShouldEnableCDCForTable()
        {
            //Given InitSetup
            var dbname = CDCConnector!.SqlConnection!.Database;

            string query = @$" USE {dbname} EXEC sys.sp_cdc_enable_table @source_schema = N'dbo',@source_name = N'TestEnteties', @role_name = NULL, @supports_net_changes = 1";

            //When
            try
            {
                CDCConnector.SqlConnection.Query(query);
            }
            catch (Exception ex)
            {
                //Then
                Assert.Fail(ex.Message);
            }
            //Then
            Assert.Pass();
        }

        [Test, Order(4)]
        public async Task ShouldSubscribeForCDC()
        {
            //Given InitSetup

            var startTime = DateTime.UtcNow.AddMinutes(1);
            var stopTime = DateTime.UtcNow;

            while (startTime >= stopTime)
            {
                var hej = CDCConnector.Subscribe("TestEnteties");

                hej.Wait();
            }

            Assert.Fail();
        }
    }
}