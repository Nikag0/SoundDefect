using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using SoundDeffect.Service;
using SoundDeffect.ViewModel;
using SoundDeffect.View;

namespace SoundDeffect
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<FileReader>();

            services.AddTransient<MainViewModel>();

            services.AddTransient<MainWindow>();
        }
    }
}
