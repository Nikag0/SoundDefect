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

            return new[] { Signal.Create(baseName, xs, ys) };
        }
    }
}
