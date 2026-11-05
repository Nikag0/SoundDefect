using SoundDefect.Domain.Signals;
using System.Windows.Media;

namespace SoundDefect.ViewModel
{
    /// <summary>
    /// Состояние отображения одного сигнала: цвет, видимость и данные для графика.
    /// </summary>
    public class SignalItemViewModel : ViewModelBase
    {
        private IReadOnlyList<SignalSegment> _segments;
        private bool _isVisible;

        public SignalId Id { get; }
        public string Name { get; }
        public Color Color { get; }
        public Brush Brush { get; }

        /// <summary>Исходные точки сигнала, из которых строятся отрезки для графика.</summary>
        public double[] OriginalXs { get; }
        public double[] OriginalYs { get; }

        /// <summary>
        /// Что рисуется на графике: один отрезок со всеми точками или несколько, если часть точек скрыта фильтром.
        /// Значения Y — исходные или нормализованные. Изменение отслеживает отрисовка и перестраивает кривую.
        /// </summary>
        public IReadOnlyList<SignalSegment> Segments
        {
            get => _segments;
            private set => SetField(ref _segments, value);
        }

        public SignalItemViewModel(Signal signal, Color color, bool isVisible = true)
        {
            Id = signal.Id;
            Name = signal.Name;
            Color = color;
            _isVisible = isVisible;

            var brush = new SolidColorBrush(color);
            brush.Freeze(); // Оптимизация WPF для неизменяемых ресурсов
            Brush = brush;

            OriginalXs = signal.Xs.ToArray();
            OriginalYs = signal.Ys.ToArray();
            _segments = new[] { new SignalSegment(OriginalXs, OriginalYs) };
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetField(ref _isVisible, value);
        }

        public void ShowSegments(IReadOnlyList<SignalSegment> segments)
        {
            ArgumentNullException.ThrowIfNull(segments);
            Segments = segments;
        }
    }
}
