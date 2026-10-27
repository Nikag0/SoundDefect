using SoundDefect.Domain.Signals;

namespace SoundDefect.Infrastructure.Parsing.Formats
{
    /// <summary>
    /// Один сигнал в файле из 17 столбцов: «X, Y1, Y2, ..., Y16» (файлы «Лазер»).
    /// Используются только столбцы X и Y1.
    /// </summary>
    public class MultiColumnFirstSignalFormat : ITextSignalFormat
    {
        private const int ColumnCount = 17;

        private const int XColumnIndex = 0;

        // Индекс столбца со значениями Y: столбец Yn имеет индекс n.
        // Чтобы брать Y9 вместо Y1, замените значение на 9.
        private const int YColumnIndex = 1;

        // Сбойное значение: в начале записи Y1 иногда резко падает ровно до 10
        // (в примерах 7–21 точка подряд), остальные значения порядка 20 000–39 000.
        private const double InvalidYValue = 10;

        public bool CanRead(int columnCount) => columnCount == ColumnCount;

        public IReadOnlyList<Signal> Read(IReadOnlyList<DataRow> rows, string baseName)
        {
            var xs = new double[rows.Count];
            var ys = new double[rows.Count];

            for (int i = 0; i < rows.Count; i++)
            {
                xs[i] = rows[i].Values[XColumnIndex];
                ys[i] = rows[i].Values[YColumnIndex];
            }

            ReplaceInvalidValues(ys);

            return new[] { Signal.Create(baseName, xs, ys) };
        }

        /// <summary>
        /// Заменяет сбойные значения Y на минимум или максимум среди корректных значений сигнала.
        /// </summary>
        private static void ReplaceInvalidValues(double[] ys)
        {
            var validYs = ys.Where(y => y != InvalidYValue).ToList();
            if (validYs.Count == 0 || validYs.Count == ys.Length) return;

            // Способ замены: оставьте раскомментированной одну из строк.
            double replacement = validYs.Min();   // Вариант 1: минимальное корректное значение
            //double replacement = validYs.Max(); // Вариант 2: максимальное корректное значение

            for (int i = 0; i < ys.Length; i++)
            {
                if (ys[i] == InvalidYValue)
                    ys[i] = replacement;
            }
        }
    }
}
