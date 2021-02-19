using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using System;
using System.Windows;

using WpfSample.Services;
using WpfSample.ViewModels;
using WpfSample.Views;

namespace WpfSample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost host;

        public static IServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            host = Host.CreateDefaultBuilder()

                //Added logging
                .UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration))
                .ConfigureServices((context, services) => ConfigureAppServices(context, services))
                .Build();
            ServiceProvider = host.Services;
        }

        private void ConfigureAppServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<MainViewModel>();
            services.AddTransient<MainWindow>();
            services.AddScoped<IFolderService, FolderService>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await host.StartAsync();
            MainWindow mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (host)
            {
                await host.StopAsync();
            }
            base.OnExit(e);
        }
    }
}