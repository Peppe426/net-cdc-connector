namespace MSSQLConnector.Models;

public record DbConfiguration
{
    public DbConfiguration(string dataSource, string initialCatalog, int port, string userId, string password)
    {
        DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        InitialCatalog = initialCatalog ?? throw new ArgumentNullException(nameof(initialCatalog));
        Port = port;
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        Password = password ?? throw new ArgumentNullException(nameof(password));
    }

    public string DataSource { get; private set; } = string.Empty;
    public string InitialCatalog { get; private set; } = string.Empty;
    public int Port { get; private set; } = default;
    public string UserId { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
}