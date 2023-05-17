public class AppSettings
{
    public const string SectionName = "AppSettings";
    public DatabaseConfig DataBaseConfig { get; set; }
    public StreamConfig StreamConfig { get; set; }
}

public class DatabaseConfig
{
    //public const string SectionName = "DatabaseConfig";
    public string? ConnectionString { get; set; }
    public string DataSource { get; set; } = string.Empty;
    public string InitialCatalog { get; set; } = string.Empty;
    public int Port { get; set; } = default;
    public string UserId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class StreamConfig
{
    //public const string SectionName = "StreamConfig";
    public string ConnectionString { get; set; } = string.Empty;
    public int Port { get; set; } = default(int);
    public string Key { get; set; } = string.Empty;
    public string StreamField { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public bool AbortOnConnectFail { get; set; } = false;
    public bool IncludeDetailInExceptions { get; set; } = true;
    public bool IncludePerformanceCountersInExceptions { get; set; } = true;
    public int ConnectRetry { get; set; } = 3;
    public int DefaultDatabase { get; set; } = 0;
    public bool SSL { get; set; } = true;
}