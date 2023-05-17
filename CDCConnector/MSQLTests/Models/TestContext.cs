using Microsoft.EntityFrameworkCore;

namespace MSQLConnectorTests.Models;

public class TestContext : DbContext
{
    public TestContext(DbContextOptions options) : base(options)
    {
    }

    private DbSet<TestEntity> TestEnteties { get; set; }
}