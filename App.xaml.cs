using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace WPFWeatherApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IHost AppHost { get; private set; }

    public App()
    {
        AppHost = Host.CreateDefaultBuilder().ConfigureServices((context, services) =>
        {
            services.AddMemoryCache();
            services.AddSingleton<CacheService>();
            services.AddSingleton<APIService>();
            services.AddTransient<MainWindow>();
        })
        .Build();
    }
    protected override async void OnStartup(StartupEventArgs e)
    {

        await AppHost.StartAsync();

        // Create and show your desired main window
        MainWindow window = AppHost.Services.GetRequiredService<MainWindow>(); // Or any other custom Window class


        // Optionally, set this window as the Application's MainWindow property
        Application.Current.MainWindow = window;
        window.Show();

        base.OnStartup(e); // Call the base implementation

    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost.StopAsync();
        AppHost.Dispose();
        base.OnExit(e);
    }
}

