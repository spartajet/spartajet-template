using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using Spartajet.WPF.Database;
using Spartajet.WPF.ViewModel;
using SqlSugar;
using DbType = System.Data.DbType;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Spartajet.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static string BaseFolder { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), typeof(App).Namespace ?? "Spartajet");

    public static string LogFolder { get; } = Path.Combine(BaseFolder, "Logs");
    public static string ConfigPath { get; } = Path.Combine(BaseFolder, "config.json");

    // public static string DataFolder { get; } = Path.Combine(ClipSharpFolder, "Data");
    public static string DataBasePath { get; } = Path.Combine(BaseFolder, "ClipSharp.db");
    public static string ImageFolder { get; } = Path.Combine(BaseFolder, "Images");

    private static readonly IHost Host =
        Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                 .ConfigureAppConfiguration(c => { c.SetBasePath(AppContext.BaseDirectory); })
                 .ConfigureServices(
                     (host, services) =>
                     {
                         services.AddHostedService<DatabaseService>();
                         services.AddTransient<MainWindow>();
                         services.AddSingleton<MainWindowViewModel>();
                         services.AddSingleton<ISqlSugarClient>(s =>
                         {
                             SqlSugarScope sqlSugar =
                                 new(new ConnectionConfig()
                                     {
                                         DbType = SqlSugar.DbType.Sqlite,
                                         ConnectionString = $"DataSource={DataBasePath}",
                                         IsAutoCloseConnection = true,
                                         InitKeyType = InitKeyType.Attribute,
                                         MoreSettings = new()
                                         {
                                             SqliteCodeFirstEnableDefaultValue = true //启用默认值
                                         }
                                     },
                                     db => { db.Aop.OnLogExecuting = (sql, pars) => { }; });
                             return sqlSugar;
                         });
                         services.AddLogging(loggingBuilder =>
                         {
                             loggingBuilder.ClearProviders();
                             loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                             loggingBuilder.AddNLog();
                         });
                         services.AddLocalization(option =>
                         {
                             option.ResourcesPath = "Assets/Language";
                         });
                     }
                 )
                 .Build();

    private async void OnExit(object sender, ExitEventArgs e)
    {
        await Host.StopAsync();
        Host.Dispose();
    }

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        CultureSetting();
        InitialFolders();
        NlogConfig();
        await Host.StartAsync();
        MainWindow window = Host.Services.GetRequiredService<MainWindow>();
        window.ShowDialog();
        
    }

    private static void CultureSetting()
    {
        Thread.CurrentThread.CurrentCulture = new("en-US");
        Thread.CurrentThread.CurrentUICulture = new("en-US");
    }

    private static void InitialFolders()
    {
        if (!Directory.Exists(BaseFolder))
        {
            Directory.CreateDirectory(BaseFolder);
        }


        if (!Directory.Exists(ImageFolder))
        {
            Directory.CreateDirectory(ImageFolder);
        }

        if (!Directory.Exists(LogFolder))
        {
            Directory.CreateDirectory(LogFolder);
        }
    }

    private static void NlogConfig()
    {
        LoggingConfiguration config = new();

        // Targets where to log to: File and Console
        FileTarget logFile = new("logFile")
        {
            FileName = Path.Combine(LogFolder, "Debug.log"),
            ArchiveEvery = FileArchivePeriod.Day,
            ArchiveNumbering = ArchiveNumberingMode.Rolling,
            ArchiveFileName = Path.Combine(LogFolder, "Debug.{#}.log"),
            ArchiveDateFormat = "yyyyMMdd",
            MaxArchiveFiles = 7,
            AutoFlush = true
        };
        FileTarget errorFile = new("errorFile")
        {
            FileName = Path.Combine(LogFolder, "Error.log"),
            ArchiveEvery = FileArchivePeriod.Day,
            ArchiveNumbering = ArchiveNumberingMode.Rolling,
            ArchiveFileName = Path.Combine(LogFolder, "Error.{#}.log"),
            ArchiveDateFormat = "yyyyMMdd",
            MaxArchiveFiles = 7,
            AutoFlush = true
        };

        ConsoleTarget logConsole = new("logConsole");

        // Rules for mapping loggers to targets            
        config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logConsole);
        config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Info, logFile);
        config.AddRule(NLog.LogLevel.Warn, NLog.LogLevel.Fatal, errorFile);
        // Apply config           
        LogManager.Configuration = config;
    }

    /// <summary>
    /// Gets registered service.
    /// </summary>
    /// <typeparam name="T">Type of the service to get.</typeparam>
    /// <returns>Instance of the service or <see langword="null"/>.</returns>
    public static T? GetService<T>() where T : class
    {
        return Host.Services.GetService(typeof(T)) as T;
    }
}