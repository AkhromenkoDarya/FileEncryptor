using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows;

namespace FileEncryptor.WPF
{
    public partial class App : Application
    {
        private static IHost _host;

        public static IHost Host => _host ??= Program.CreateHostBuilder(Environment
            .GetCommandLineArgs()).Build();

        public static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
        {
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            IHost host = Host;
            base.OnStartup(e);
            await host.StartAsync();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            using (Host) await Host.StopAsync();
        }
    }
}
