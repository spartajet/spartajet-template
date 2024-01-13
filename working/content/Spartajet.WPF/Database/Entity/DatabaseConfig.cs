using SqlSugar;

namespace Spartajet.WPF.Database.Entity;

[SugarTable("DatabaseConfig")]
public class DatabaseConfig
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    public string Key { get; set; } = string.Empty;
    public int IntValue { get; set; }
    public long LongValue { get; set; }
    public string StringValue { get; set; } = string.Empty;
}