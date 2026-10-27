using SoundDefect.Domain.Signals;
using System.Windows.Media;

namespace SoundDefect.ViewModel
{
    /// <summary>
    /// Состояние отображения одного сигнала: цвет, видимость и данные для графика.
    /// </summary>
    public class SignalItemViewModel : ViewModelBase
    {
        private readonly double[] _originalYs;
        private double[] _ys;
        private bool _isVisible = true;

        public SignalId Id { get; }
        public string Name { get; }
        public Color Color { get; }
        public Brush Brush { get; }

        /// <summary>
        /// Точки, которые рисуются на графике: исходные или нормализованные значения Y.
        /// Изменение Ys отслеживает отрисовка и перестраивает кривую.
        /// </summary>
        public double[] Xs { get; }

        public double[] Ys
        {
            get => _ys;
            private set => SetField(ref _ys, value);
        }

        public SignalItemViewModel(Signal signal, Color color)
        {
            Id = signal.Id;
            Name = signal.Name;
            Color = color;

            var brush = new SolidColorBrush(color);
            brush.Freeze(); // Оптимизация WPF для неизменяемых ресурсов
            Brush = brush;

            Xs = signal.Xs.ToArray();
            _originalYs = signal.Ys.ToArray();
            _ys = _originalYs;
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetField(ref _isVisible, value);
        }

        public void ShowOriginal() => Ys = _originalYs;

        public void ShowValues(double[] ys)
        {
            if (ys.Length != _originalYs.Length)
                throw new ArgumentException("Количество значений должно совпадать с количеством точек сигнала.", nameof(ys));

            Ys = ys;
        }
    }
}
