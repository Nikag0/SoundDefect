using ScottPlot.Plottables;
using ScottPlot.WPF;
using SoundDefect.ViewModel;
using System.ComponentModel;

namespace SoundDefect.View
{
    /// <summary>
    /// Показывает интервалы работы лазера: подсветку фоном на основном графике
    /// и закрашенные участки на полосе «Лазер» под ним.
    /// </summary>
    public sealed class LaserIntervalsRenderer : IDisposable
    {
        private static readonly ScottPlot.Color LaserColor = new(29, 158, 117);

        private readonly WpfPlot _plot;
        private readonly WpfPlot _strip;
        private readonly ChartTabViewModel _tab;
        private readonly List<HorizontalSpan> _plotSpans = new();
        private readonly List<HorizontalSpan> _stripSpans = new();

        public LaserIntervalsRenderer(WpfPlot plot, WpfPlot strip, ChartTabViewModel tab)
        {
            _plot = plot;
            _strip = strip;
            _tab = tab;

            _tab.PropertyChanged += OnTabPropertyChanged;
            Redraw();
        }

        private void OnTabPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(ChartTabViewModel.LaserActivity) or nameof(ChartTabViewModel.HighlightLaserIntervals))
                Redraw();
        }

        private void Redraw()
        {
            RemoveSpans();

            var activity = _tab.LaserActivity;
            if (activity != null)
            {
                foreach (var interval in activity.OnIntervals)
                {
                    if (_tab.HighlightLaserIntervals)
                        _plotSpans.Add(AddSpan(_plot, interval.Start, interval.End, LaserColor.WithAlpha(0.15)));

                    _stripSpans.Add(AddSpan(_strip, interval.Start, interval.End, LaserColor));
                }
            }

            _plot.Refresh();
            _strip.Refresh();
        }

        private static HorizontalSpan AddSpan(WpfPlot target, double start, double end, ScottPlot.Color color)
        {
            var span = target.Plot.Add.HorizontalSpan(start, end);
            span.FillColor = color;
            span.LineWidth = 0;
            span.EnableAutoscale = false; // подсветка не должна влиять на масштаб графика
            target.Plot.MoveToBack(span); // и не должна перекрывать кривые
            return span;
        }

        private void RemoveSpans()
        {
            foreach (var span in _plotSpans)
                _plot.Plot.Remove(span);
            foreach (var span in _stripSpans)
                _strip.Plot.Remove(span);

            _plotSpans.Clear();
            _stripSpans.Clear();
        }

        public void Dispose()
        {
            _tab.PropertyChanged -= OnTabPropertyChanged;
            RemoveSpans();
        }
    }
}
