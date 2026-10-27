using SoundDefect.Domain.Signals;
using System.Windows.Media;

namespace SoundDefect.ViewModel
{
    /// <summary>
    /// Состояние отображения одного сигнала: цвет, видимость и данные для графика.
    /// </summary>
    public class SignalItemViewModel : ViewModelBase
    {
        private bool _isVisible = true;

        public SignalId Id { get; }
        public string Name { get; }
        public Color Color { get; }
        public Brush Brush { get; }

        /// <summary>
        /// Точки, которые рисуются на графике. Сейчас это исходные данные сигнала;
        /// при выборе режима нормализации здесь будут подменяться значения Y
        /// (с уведомлением PropertyChanged, на которое реагирует отрисовка).
        /// </summary>
        public double[] Xs { get; }
        public double[] Ys { get; }

        public SignalItemViewModel(Signal signal, Color color)
        {
            Id = signal.Id;
            Name = signal.Name;
            Color = color;

            var brush = new SolidColorBrush(color);
            brush.Freeze(); // Оптимизация WPF для неизменяемых ресурсов
            Brush = brush;

            Xs = signal.Xs.ToArray();
            Ys = signal.Ys.ToArray();
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetField(ref _isVisible, value);
        }
    }
}
