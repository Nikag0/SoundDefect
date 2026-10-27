using System.Windows.Media;

namespace SoundDefect.ViewModel
{
    /// <summary>
    /// Выдаёт цвета для новых сигналов по кругу из фиксированной палитры.
    /// </summary>
    public class SignalColorPalette
    {
        private static readonly Color[] Colors =
        {
            Color.FromRgb(31, 119, 180),  // Синий
            Color.FromRgb(255, 127, 14),  // Оранжевый
            Color.FromRgb(44, 160, 44),   // Зеленый
            Color.FromRgb(214, 39, 40),   // Красный
            Color.FromRgb(148, 103, 189), // Фиолетовый
            Color.FromRgb(140, 86, 75),   // Коричневый
            Color.FromRgb(227, 119, 194), // Розовый
            Color.FromRgb(127, 127, 127), // Серый
            Color.FromRgb(188, 189, 34),  // Болотный
            Color.FromRgb(23, 190, 207)   // Голубой
        };

        private int _nextIndex;

        public Color Next()
        {
            Color color = Colors[_nextIndex % Colors.Length];
            _nextIndex++;
            return color;
        }

        public void Reset() => _nextIndex = 0;
    }
}
