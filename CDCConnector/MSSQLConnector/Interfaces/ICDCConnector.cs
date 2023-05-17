using MSSQLConnector.Models;
using StackExchange.Redis;

namespace MSSQLConnector.Interfaces;

public interface ICDCConnector<TEntity, TStreamEvent> where TEntity : class where TStreamEvent : class
{
    Task<CDCConnector<TEntity, TStreamEvent>> InitConnector(DbConfiguration? DbConnection, RedisConfiguration redisConfiguration, string dbConnectionString = "");

    Task ConnectToDatabase(DbConfiguration? connection, string connectionString = "");

    Task Subscribe(string table, int interval = 1000, CancellationToken cancellationToken = default);

    Task<(bool isSuccess, string response)> EmitEntryToStream(TStreamEvent entry, CommandFlags commandFlags = CommandFlags.None);
}