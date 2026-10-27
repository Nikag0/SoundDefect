using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using SoundDefect.Application.Import;
using SoundDefect.Application.Normalization;
using SoundDefect.Application.Workspace;
using SoundDefect.Dialogs;
using SoundDefect.Infrastructure.Parsing;
using SoundDefect.Infrastructure.Parsing.Formats;
using SoundDefect.ViewModel;
using SoundDefect.View;

namespace SoundDefect
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
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
            // Application
            services.AddSingleton<ISignalWorkspace, SignalWorkspace>();
            services.AddTransient<ImportSignalsFromFile>();
            services.AddSingleton<INormalizationMethod, GlobalMinMaxNormalization>();

            // Infrastructure
            services.AddSingleton<ITextSignalFormat, ChannelXYFormat>();
            services.AddSingleton<ITextSignalFormat, MultiColumnFirstSignalFormat>();
            services.AddSingleton<ISignalFileParser, TextSignalFileParser>();

            // Presentation
            services.AddSingleton<IFileDialogService, FileDialogService>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainWindow>();
        }
    }
}
