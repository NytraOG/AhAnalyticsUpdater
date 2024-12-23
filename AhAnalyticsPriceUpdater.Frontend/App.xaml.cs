using System.IO;
using System.Windows;
using AhAnalyticsPriceUpdater.Frontend.Models;
using AhAnalyticsPriceUpdater.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AhAnalyticsPriceUpdater.Frontend;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly ServiceProvider serviceProvider;

    public App()
    {
        Configuration = new ConfigurationBuilder()
                       .SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("appsettings.json", false, true)
                       .Build();

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        serviceProvider = serviceCollection.BuildServiceProvider();
    }

    public IConfiguration Configuration { get; private set; }

    private void ConfigureServices(IServiceCollection services)
    {


        services.AddSingleton(Configuration);
        //services.AddTransient<ILogger, AhAnalyticsPriceUpdater.LogProvider>()
        // services.AddTransient<ScanDataDecrypter>();
        // services.AddTransient<SpreadsheetService>();
        services.AddSingleton<UpdateProcessViewModel>();
        services.AddSingleton<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}