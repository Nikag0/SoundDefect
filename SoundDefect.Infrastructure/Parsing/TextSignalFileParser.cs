using SoundDefect.Application.Import;
using SoundDefect.Domain.Signals;
using System.Globalization;

namespace SoundDefect.Infrastructure.Parsing
{
    /// <summary>
    /// Читает текстовый файл с числовыми столбцами и выбирает формат по количеству столбцов.
    /// Пустые строки и строки до первой числовой (заголовки) пропускаются.
    /// </summary>
    public class TextSignalFileParser : ISignalFileParser
    {
        private static readonly char[] Separators = { ' ', '\t', ';' };

        private readonly IReadOnlyList<ITextSignalFormat> _formats;

        public TextSignalFileParser(IEnumerable<ITextSignalFormat> formats)
        {
            _formats = formats.ToList();
        }

        public async Task<IReadOnlyList<Signal>> ParseAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл не найден: {filePath}", filePath);

            var rows = new List<DataRow>();
            int? columnCount = null;
            int lineNumber = 0;

            using var reader = new StreamReader(filePath);

            while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is string line)
            {
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
                double[]? values = TryParseNumbers(parts);

                if (values == null)
                {
                    if (columnCount == null) continue; // заголовок перед данными

                    throw new FormatException($"Строка {lineNumber}: не удалось разобрать числа: \"{line}\".");
                }

                columnCount ??= values.Length;

                if (values.Length != columnCount)
                    throw new FormatException($"Строка {lineNumber}: ожидалось столбцов: {columnCount}, найдено: {values.Length}.");

                rows.Add(new DataRow(lineNumber, values));
            }

            if (columnCount == null)
                throw new FormatException("В файле нет строк с данными.");

            ITextSignalFormat format = _formats.FirstOrDefault(f => f.CanRead(columnCount.Value))
                ?? throw new FormatException($"Неизвестный формат файла: строки содержат {columnCount} столбцов.");

            return format.Read(rows, Path.GetFileNameWithoutExtension(filePath));
        }

        private static double[]? TryParseNumbers(string[] parts)
        {
            var values = new double[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                if (!double.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out values[i]))
                    return null;
            }

            return values;
        }
    }
}
