using SoundDefect.ViewModel;
using System.Windows;

namespace SoundDefect.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SignalPlotRenderer _plotRenderer;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
            _plotRenderer = new SignalPlotRenderer(MainPlot, viewModel.Signals);

            Closed += (_, _) => _plotRenderer.Dispose();
        }
    }
}
