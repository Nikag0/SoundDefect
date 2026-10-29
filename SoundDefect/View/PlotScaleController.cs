using ScottPlot;
using ScottPlot.WPF;
using SoundDefect.ViewModel;

namespace SoundDefect.View
{
    /// <summary>
    /// Связывает настройки масштаба из ViewModel с графиком ScottPlot:
    /// показывает текущие границы осей (в том числе после прокрутки и зума мышью),
    /// применяет границы, введённые вручную, и вписывает данные по кнопке.
    /// </summary>
    public sealed class PlotScaleController : IDisposable
    {
        private readonly WpfPlot _plot;
        private readonly PlotScaleViewModel _scale;

        public PlotScaleController(WpfPlot plot, PlotScaleViewModel scale)
        {
            _plot = plot;
            _scale = scale;

            _plot.Plot.RenderManager.AxisLimitsChanged += OnAxisLimitsChanged;
            _scale.RangeEdited += OnRangeEdited;
            _scale.FitRequested += OnFitRequested;

            // Вкладка могла быть пересоздана: возвращаем графику масштаб, который был до этого
            if (_scale.IsXLocked || _scale.IsYLocked)
                OnRangeEdited(this, EventArgs.Empty);
        }

        /// <summary>
        /// Автомасштаб с учётом фиксации: зафиксированные оси сохраняют свои границы.
        /// Вызывается при любом изменении данных на графике.
        /// </summary>
        public static void AutoScale(WpfPlot plot, PlotScaleViewModel scale)
        {
            AxisLimits before = plot.Plot.Axes.GetLimits();
            plot.Plot.Axes.AutoScale();

            if (scale.IsXLocked) plot.Plot.Axes.SetLimitsX(before.Left, before.Right);
            if (scale.IsYLocked) plot.Plot.Axes.SetLimitsY(before.Bottom, before.Top);
        }

        private void OnAxisLimitsChanged(object? sender, RenderDetails e)
        {
            AxisLimits limits = _plot.Plot.Axes.GetLimits();
            _scale.UpdateFromPlot(limits.Left, limits.Right, limits.Bottom, limits.Top);
        }

        private void OnRangeEdited(object? sender, EventArgs e)
        {
            _plot.Plot.Axes.SetLimits(_scale.XMin, _scale.XMax, _scale.YMin, _scale.YMax);
            _plot.Refresh();
        }

        private void OnFitRequested(object? sender, EventArgs e)
        {
            // Вписать всё — явная команда пользователя, поэтому фиксация здесь не мешает.
            // Новые границы остаются зафиксированными, если ось была зафиксирована.
            _plot.Plot.Axes.AutoScale();
            _plot.Refresh();
        }

        public void Dispose()
        {
            // AxisLimitsChanged в ScottPlot — свойство, которое не должно становиться null:
            // если наш обработчик был единственным, оставляем пустой
            var remaining = _plot.Plot.RenderManager.AxisLimitsChanged - OnAxisLimitsChanged;
            _plot.Plot.RenderManager.AxisLimitsChanged = remaining ?? delegate { };

            _scale.RangeEdited -= OnRangeEdited;
            _scale.FitRequested -= OnFitRequested;
        }
    }
}
