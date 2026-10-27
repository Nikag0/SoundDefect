using ScottPlot.Plottables;
using ScottPlot.WPF;
using SoundDefect.ViewModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SoundDefect.View
{
    /// <summary>
    /// Связывает коллекцию сигналов из ViewModel с графиком ScottPlot:
    /// добавляет и удаляет кривые, переключает их видимость.
    /// Единственное место, которое знает о ScottPlot.
    /// </summary>
    public sealed class SignalPlotRenderer : IDisposable
    {
        private readonly WpfPlot _plot;
        private readonly ObservableCollection<SignalItemViewModel> _signals;
        private readonly Dictionary<SignalItemViewModel, SignalXY> _plottables = new();

        public SignalPlotRenderer(WpfPlot plot, ObservableCollection<SignalItemViewModel> signals)
        {
            _plot = plot;
            _signals = signals;

            _signals.CollectionChanged += OnCollectionChanged;

            foreach (var item in _signals)
                AddPlottable(item);

            RescaleAndRefresh();
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var item in _plottables.Keys.ToList())
                    RemovePlottable(item);
            }
            else
            {
                if (e.OldItems != null)
                    foreach (SignalItemViewModel item in e.OldItems)
                        RemovePlottable(item);

                if (e.NewItems != null)
                    foreach (SignalItemViewModel item in e.NewItems)
                        AddPlottable(item);
            }

            RescaleAndRefresh();
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not SignalItemViewModel item || !_plottables.TryGetValue(item, out var plottable))
                return;

            switch (e.PropertyName)
            {
                case nameof(SignalItemViewModel.IsVisible):
                    plottable.IsVisible = item.IsVisible;
                    _plot.Refresh();
                    break;

                case nameof(SignalItemViewModel.Xs):
                case nameof(SignalItemViewModel.Ys):
                    RemovePlottable(item);
                    AddPlottable(item);
                    RescaleAndRefresh();
                    break;
            }
        }

        private void AddPlottable(SignalItemViewModel item)
        {
            var plottable = _plot.Plot.Add.SignalXY(item.Xs, item.Ys);
            plottable.LegendText = item.Name;
            plottable.Color = new ScottPlot.Color(item.Color.R, item.Color.G, item.Color.B);
            plottable.IsVisible = item.IsVisible;

            _plottables.Add(item, plottable);
            item.PropertyChanged += OnItemPropertyChanged;
        }

        private void RemovePlottable(SignalItemViewModel item)
        {
            if (!_plottables.Remove(item, out var plottable))
                return;

            item.PropertyChanged -= OnItemPropertyChanged;
            _plot.Plot.Remove(plottable);
        }

        private void RescaleAndRefresh()
        {
            if (_plottables.Count > 0)
                _plot.Plot.Axes.AutoScale();
            else
                _plot.Plot.Axes.SetLimits(0, 10, 0, 10);

            _plot.Refresh();
        }

        public void Dispose()
        {
            _signals.CollectionChanged -= OnCollectionChanged;

            foreach (var item in _plottables.Keys)
                item.PropertyChanged -= OnItemPropertyChanged;

            _plottables.Clear();
        }
    }
}
