using System.ComponentModel.DataAnnotations;

namespace MSQLConnectorTests.Models;

public class TestEntity
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}