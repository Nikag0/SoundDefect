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
    /// Каждый сигнал рисуется набором кривых — по одной на отрезок, чтобы между отрезками были разрывы.
    /// </summary>
    public sealed class SignalPlotRenderer : IDisposable
    {
        private readonly WpfPlot _plot;
        private readonly ObservableCollection<SignalItemViewModel> _signals;
        private readonly PlotScaleViewModel _scale;
        private readonly Dictionary<SignalItemViewModel, List<SignalXY>> _plottables = new();

        public SignalPlotRenderer(WpfPlot plot, ObservableCollection<SignalItemViewModel> signals, PlotScaleViewModel scale)
        {
            _plot = plot;
            _signals = signals;
            _scale = scale;

            _signals.CollectionChanged += OnCollectionChanged;

            foreach (var item in _signals)
                AddPlottables(item);

            RescaleAndRefresh();
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var item in _plottables.Keys.ToList())
                    RemovePlottables(item);
            }
            else
            {
                if (e.OldItems != null)
                    foreach (SignalItemViewModel item in e.OldItems)
                        RemovePlottables(item);

                if (e.NewItems != null)
                    foreach (SignalItemViewModel item in e.NewItems)
                        AddPlottables(item);
            }

            RescaleAndRefresh();
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not SignalItemViewModel item || !_plottables.TryGetValue(item, out var plottables))
                return;

            switch (e.PropertyName)
            {
                case nameof(SignalItemViewModel.IsVisible):
                    foreach (var plottable in plottables)
                        plottable.IsVisible = item.IsVisible;
                    _plot.Refresh();
                    break;

                case nameof(SignalItemViewModel.Segments):
                    RemovePlottables(item);
                    AddPlottables(item);
                    RescaleAndRefresh();
                    break;
            }
        }

        private void AddPlottables(SignalItemViewModel item)
        {
            var color = new ScottPlot.Color(item.Color.R, item.Color.G, item.Color.B);
            var plottables = new List<SignalXY>(item.Segments.Count);

            foreach (var segment in item.Segments)
            {
                var plottable = _plot.Plot.Add.SignalXY(segment.Xs, segment.Ys);
                plottable.Color = color;
                plottable.IsVisible = item.IsVisible;
                plottables.Add(plottable);
            }

            _plottables.Add(item, plottables);
            item.PropertyChanged += OnItemPropertyChanged;
        }

        private void RemovePlottables(SignalItemViewModel item)
        {
            if (!_plottables.Remove(item, out var plottables))
                return;

            item.PropertyChanged -= OnItemPropertyChanged;
            foreach (var plottable in plottables)
                _plot.Plot.Remove(plottable);
        }

        /// <summary>
        /// Подгоняет масштаб под данные. Зафиксированные оси (PlotScaleViewModel) не меняются.
        /// </summary>
        private void RescaleAndRefresh()
        {
            if (_plottables.Values.Any(p => p.Count > 0))
                PlotScaleController.AutoScale(_plot, _scale);
            else if (!_scale.IsXLocked && !_scale.IsYLocked)
                _plot.Plot.Axes.SetLimits(0, 10, 0, 10);

            _plot.Refresh();
        }

        /// <summary>
        /// Отписывается от сигналов и убирает свои кривые с графика,
        /// чтобы при повторном подключении к тому же графику кривые не задвоились.
        /// </summary>
        public void Dispose()
        {
            _signals.CollectionChanged -= OnCollectionChanged;

            foreach (var (item, plottables) in _plottables)
            {
                item.PropertyChanged -= OnItemPropertyChanged;
                foreach (var plottable in plottables)
                    _plot.Plot.Remove(plottable);
            }

            _plottables.Clear();
        }
    }
}
