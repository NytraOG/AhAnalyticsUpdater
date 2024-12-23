using System.IO;
using System.Windows;
using AhAnalyticsPriceUpdater.Frontend.Interfaces;
using AhAnalyticsPriceUpdater.Frontend.Models;
using AhAnalyticsPriceUpdater.Frontend.Services;
using AhAnalyticsPriceUpdater.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AhAnalyticsPriceUpdater.Frontend;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? serviceProvider;

    public App()
    {
        BuildConfiguration();
        BuildLogger();
        BuildServiceCollection();
    }

    public IConfiguration? Configuration { get; private set; }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(configure =>
        {
            configure.ClearProviders();
            configure.AddSerilog(dispose: true);
        });

        services.AddSingleton(Configuration ?? throw new InvalidOperationException("Configuration is Null."));
        services.AddTransient<ScanDataDecrypter>();
        services.AddTransient<SpreadsheetService>();
        services.AddTransient<IDialogService, FileDialogService>();
        services.AddSingleton<UpdateProcessViewModel>();
        services.AddSingleton<MainWindow>();
    }

    private void BuildServiceCollection()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        serviceProvider = serviceCollection.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (serviceProvider == null)
            return;

        var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void BuildLogger() => Log.Logger = new LoggerConfiguration()
                                                     .WriteTo.Console()
                                                     .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
                                                     .MinimumLevel.Debug()
                                                     .CreateLogger();

    private void BuildConfiguration() => Configuration = new ConfigurationBuilder()
                                                        .SetBasePath(Directory.GetCurrentDirectory())
                                                        .AddJsonFile("appsettings.json", false, true)
                                                        .Build();
}