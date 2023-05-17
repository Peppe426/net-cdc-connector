namespace MSSQLConnector.Models;

public record RedisConfiguration
{
    public RedisConfiguration(string connectionString, int port, string key, string streamField, string? password, bool? abortOnConnectFail, bool? includeDetailInExceptions, bool? includePerformanceCountersInExceptions, int connectRetry, int defaultDatabase, bool? ssl)
    {
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        Key = key ?? throw new ArgumentNullException(nameof(key));
        StreamField = streamField ?? throw new ArgumentNullException(nameof(streamField));
        Password = password ?? string.Empty;
        Port = port == default ? throw new AbandonedMutexException(nameof(port)) : port;

        DefaultDatabase = defaultDatabase;
        AbortOnConnectFail = abortOnConnectFail ?? false;
        IncludeDetailInExceptions = includeDetailInExceptions ?? false;
        IncludePerformanceCountersInExceptions = includePerformanceCountersInExceptions ?? false;
        ConnectRetry = connectRetry == default ? 3 : connectRetry;
        SSL = ssl ?? false;
    }

    public string ConnectionString { get; private set; } = string.Empty;
    public int Port { get; set; } = default(int);
    public string Key { get; private set; } = string.Empty;
    public string StreamField { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;

    public bool AbortOnConnectFail { get; private set; } = false;
    public bool IncludeDetailInExceptions { get; private set; } = false;
    public bool IncludePerformanceCountersInExceptions { get; private set; } = false;
    public int ConnectRetry { get; private set; } = 3;
    public int DefaultDatabase { get; private set; } = 0;
    public bool SSL { get; private set; } = true;
}