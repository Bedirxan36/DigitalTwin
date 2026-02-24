using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DigitalTwin.Data;
using DigitalTwin.Services.Core;
using DigitalTwin.Services.Security;
using DigitalTwin.ViewModels;

namespace DigitalTwin;

public partial class App : Application
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize database
        DbInitializer.Initialize();

        // Setup DI
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // DbContext
        services.AddDbContext<DigitalTwinDbContext>();

        // Services
        services.AddSingleton<EncryptionService>();
        services.AddSingleton<ActivityTrackingService>();
        services.AddSingleton<AnalysisService>();
        services.AddSingleton<ExportImportService>();

        // ViewModels
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<InsightsViewModel>();
        services.AddTransient<SettingsViewModel>();
    }
}
