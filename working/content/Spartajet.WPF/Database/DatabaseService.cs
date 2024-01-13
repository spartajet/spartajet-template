using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spartajet.WPF.Database.Entity;
using SqlSugar;

namespace Spartajet.WPF.Database;

public class DatabaseService : IHostedService
{
    private readonly ISqlSugarClient db;
    private readonly ILogger<DatabaseService> logger;

    public DatabaseService(ISqlSugarClient db, ILogger<DatabaseService> logger)
    {
        this.db = db;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await this.PreOperateDatabase();
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task PreOperateDatabase()
    {
        if (!File.Exists(App.DataBasePath))
        {
            // this.db.CodeFirst.InitTables<ClipHistory>();
            // this.db.CodeFirst.InitTables<DatabaseConfig>();

            SnowFlakeSingle.WorkId = this.InitialSnowFlakeWorkId();
            this.logger.LogInformation("Create Local Sqlite Database");
        }

        // DatabaseConfig config = await this.db.Queryable<DatabaseConfig>().Where(t => t.Key.Equals("WorkId")).FirstAsync();
        // if (config == null)
        // {
        //     SnowFlakeSingle.WorkId = this.InitialSnowFlakeWorkId();
        // }
    }

    private int InitialSnowFlakeWorkId()
    {
        int workId = 1;
        this.db.Insertable(new DatabaseConfig()
        {
            Key = "WorkId",
            IntValue = 1
        }).ExecuteCommand();
        this.logger.LogInformation("Insert Snow Flake Work Id");
        return workId;
    }
}