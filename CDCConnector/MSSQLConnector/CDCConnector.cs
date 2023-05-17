using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MSSQLConnector.Exceptions;
using MSSQLConnector.Interfaces;
using MSSQLConnector.Models;
using StackExchange.Redis;
using System.Data;
using System.Text.Json;
using static Dapper.SqlMapper;

namespace MSSQLConnector;

public class CDCConnector<TEntity, TStreamEvent> : ICDCConnector<TEntity, TStreamEvent> where TEntity : class where TStreamEvent : class
{
    public delegate void TableChangeEventHandler(object source, EventArgs args);

    public event TableChangeEventHandler TableChanged;

    protected virtual void OnTableChanged()
    {
        if (TableChanged != null)
        {
            TableChanged(this, EventArgs.Empty);
        }
    }

    private readonly ILogger<CDCConnector<TEntity, TStreamEvent>> _logger;

    public SqlConnection? SqlConnection = null;
    public ConnectionMultiplexer? RedisConnection = null;
    private RedisConfiguration? _redisConfiguration = null;

    public CDCConnector(ILogger<CDCConnector<TEntity, TStreamEvent>> logger)
    {
        _logger = logger;
    }

    public async Task<CDCConnector<TEntity, TStreamEvent>> InitConnector(DbConfiguration? dbConfiguration, RedisConfiguration redisConfiguration, string dbConnectionString = "")
    {
        _redisConfiguration = redisConfiguration;

        await ConnectToDatabase(dbConfiguration, dbConnectionString);
        await ConnectToRedis(redisConfiguration);

        return this;
    }

    public virtual async Task ConnectToDatabase(DbConfiguration? connection, string connectionString = "")
    {
        var initConnection = await OpenDbConnection(connection, connectionString);
        if (initConnection is null || initConnection.State is not ConnectionState.Open)
        {
            SqlConnection = null;
            throw new CDCConnectorError("Faild to connect to database");
        }
        SqlConnection = initConnection;
    }

    private async Task<SqlConnection?> OpenDbConnection(DbConfiguration? configuration, string connectionString = "")
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = $"Data Source={configuration.DataSource};Initial Catalog={configuration.InitialCatalog};User ID={configuration.UserId};Password={configuration.Password}";
        }
        try
        {
            SqlConnection? connection = new SqlConnection(connectionString);

            connection.Open();
            return await Task.FromResult(connection);
        }
        catch (Exception ex)
        {
            throw new CDCConnectorError(ex.Message, ex.InnerException);
        }
    }

    public virtual async Task Subscribe(string table, int interval = 1000, CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            List<TEntity> ChangedData = await GetCdcLog(table);

            foreach (TEntity entity in ChangedData)
            {
                TStreamEvent streamEntry = AsStreamEvent(entity);

                StackExchange.Redis.CommandFlags commandFlag = default;
                var output = await EmitEntryToStream(streamEntry, commandFlag);
                OnTableChanged();
                if (output.isSuccess is false)
                {
                    _logger.LogError("Faild to emit event");
                }
            }

            await Task.Delay(interval, cancellationToken);
        }
    }

    private async Task<List<TEntity>> GetCdcLog(string table)
    {
        //TODO call SP to get latest changes --> https://learn.microsoft.com/en-us/sql/relational-databases/track-changes/about-change-data-capture-sql-server?view=sql-server-ver16#capture-instance-1
        throw new NotImplementedException();
    }

    private TStreamEvent AsStreamEvent(TEntity entity)
    {
        var properties = entity.GetType().GetProperties();
        var inst = Activator.CreateInstance(typeof(TStreamEvent));
        foreach (var i in properties)
        {
            if (((TStreamEvent)inst).GetType().GetProperty(i.Name) == null)
                continue;
            var valor = i.GetValue(entity, null);
            ((TStreamEvent)inst).GetType().GetProperty(i.Name).SetValue((TStreamEvent)inst, valor);
        }
        return (TStreamEvent)inst;
    }

    public async Task<(bool isSuccess, string response)> EmitEntryToStream(TStreamEvent entry, StackExchange.Redis.CommandFlags commandFlags = StackExchange.Redis.CommandFlags.None)
    {
        if (RedisConnection is null)
        {
            var connectionResult = await ConnectToRedis(_redisConfiguration);
            if (connectionResult.isSuccess is false)
            {
                throw new CDCConnectorError("Faild to connect to redis");
            }
        }
        var db = RedisConnection!.GetDatabase();
        //TODO add support for protobuffer
        string streamEntry = JsonSerializer.Serialize(entry);
        RedisValue messageId = await db.StreamAddAsync(_redisConfiguration.Key, _redisConfiguration.StreamField, streamEntry, null, null, false, commandFlags);

        if (messageId.IsNullOrEmpty)
        {
            throw new RedisException("Failed to emit message to a Redis Stream. Return message was either null or empty.");
        }
        return (true, messageId!);
    }

    private async Task<(bool isSuccess, ConnectionMultiplexer? connection)> ConnectToRedis(RedisConfiguration redisConfiguration)
    {
        var options = new ConfigurationOptions
        {
            AbortOnConnectFail = redisConfiguration.AbortOnConnectFail,
            IncludeDetailInExceptions = redisConfiguration.IncludeDetailInExceptions,
            IncludePerformanceCountersInExceptions = redisConfiguration.IncludePerformanceCountersInExceptions,
            EndPoints = { { _redisConfiguration.ConnectionString, _redisConfiguration.Port } },
            ConnectRetry = redisConfiguration.ConnectRetry,
            DefaultDatabase = redisConfiguration.DefaultDatabase,
            Ssl = redisConfiguration.SSL,
            Password = redisConfiguration.Password
        };
        try
        {
            RedisConnection = await ConnectionMultiplexer.ConnectAsync(options);
            return (RedisConnection.IsConnected, RedisConnection);
        }
        catch (Exception ex)
        {
            throw new CDCConnectorError("Faild to connect to redis", ex);
        }
    }

    public virtual async Task OLD__Subscribe(TStreamEvent output, string query, string orderBy = "DATE_MODIFIED", CancellationToken cancellationToken = default)
    {
        //DateTime dateTime = _latestRecord is null ? DateTime.UtcNow : DateTime.Parse(_latestRecord);
        var builder = new SqlBuilder();
        var sql = query;
        builder.OrderBy($"{orderBy} DESC");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                //builder.Where($"{orderBy} > {dateTime}");
                var template = builder.AddTemplate(sql);

                var rows = SqlConnection.Query<TEntity>(template.RawSql).AsList();
                //await Produce(rows);
                //setLatesRecordDate(rows);
            }
        }
        catch (Exception ex)
        {
        }

        //void setLatesRecordDate(List<TEntity> rows)
        //{
        //    var latestRow = rows?.FirstOrDefault()?.DATE_MODIFIED;
        //    _latestRecord = latestRow?.ToString();
        //}
    }
}