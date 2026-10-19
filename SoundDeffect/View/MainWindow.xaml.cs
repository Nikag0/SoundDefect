using SoundDeffect.Domain;
using SoundDeffect.ViewModel;
using System.Windows;

namespace SoundDeffect.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;
            _viewModel.DatasetLoaded += OnDatasetLoaded;
            _viewModel.ChartsCleared += OnChartsCleared;
            _viewModel.RequestPlotUpdate += OnRequestPlotUpdate;
        }

        private void OnDatasetLoaded(Dataset dataset)
        {
            MainPlot.Plot.Clear();

            int chartCounter = 0;

            foreach (var channelKeyValuePair in dataset.Channels)
            {
                int channelId = channelKeyValuePair.Key;
                var channelData = channelKeyValuePair.Value;

                if (channelData.XValues.Count == 0) continue;

                double[] xs = channelData.XValues.ToArray();
                double[] ys = channelData.YValues.ToArray();

                    var scatter = MainPlot.Plot.Add.Scatter(xs, ys);
                    scatter.Label = $"Канал {channelId}";

                chartCounter++;
            }

            MainPlot.Plot.Axes.AutoScale();

            MainPlot.Plot.ShowLegend();

            MainPlot.Refresh();
        }

        private void OnChartsCleared()
        {
            MainPlot.Plot.Clear();

            MainPlot.Plot.Axes.SetLimits(0, 10, 0, 10);

            MainPlot.Refresh();
        }

        private void OnRequestPlotUpdate(Dataset dataset, int[] activeChannelIds)
        {
            MainPlot.Plot.Clear();

            foreach (int channelId in activeChannelIds)
            {
                if (dataset.Channels.TryGetValue(channelId, out var channelData))
                {
                    if (channelData.XValues.Count == 0) continue;

                    var scatter = MainPlot.Plot.Add.Scatter(
                        channelData.XValues.ToArray(),
                        channelData.YValues.ToArray()
                    );
                    scatter.Label = $"Канал {channelId}";
                }
            }

            MainPlot.Plot.Axes.AutoScale();
            MainPlot.Plot.ShowLegend();

            MainPlot.Refresh();
        }
    }
}