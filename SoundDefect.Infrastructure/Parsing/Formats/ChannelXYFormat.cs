using SoundDefect.Domain.Signals;

namespace SoundDefect.Infrastructure.Parsing.Formats
{
    /// <summary>
    /// Несколько сигналов в одном файле: столбцы «канал, x, y».
    /// Каждый канал становится отдельным сигналом.
    /// </summary>
    public class ChannelXYFormat : ITextSignalFormat
    {
        private const int ColumnCount = 3;

        public bool CanRead(int columnCount) => columnCount == ColumnCount;

        public IReadOnlyList<Signal> Read(IReadOnlyList<DataRow> rows, string baseName)
        {
            var channels = new SortedDictionary<int, (List<double> Xs, List<double> Ys)>();

            foreach (var row in rows)
            {
                double rawChannel = row.Values[0];
                if (rawChannel != Math.Floor(rawChannel))
                    throw new FormatException($"Строка {row.LineNumber}: номер канала должен быть целым числом, получено {rawChannel}.");

                int channel = (int)rawChannel;

                if (!channels.TryGetValue(channel, out var points))
                {
                    points = (new List<double>(), new List<double>());
                    channels.Add(channel, points);
                }

                points.Xs.Add(row.Values[1]);
                points.Ys.Add(row.Values[2]);
            }

            return channels
                .Select(kvp => Signal.Create($"{baseName}, канал {kvp.Key}", kvp.Value.Xs, kvp.Value.Ys))
                .ToList();
        }
    }
}
