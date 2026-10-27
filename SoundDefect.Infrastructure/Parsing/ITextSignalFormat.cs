using SoundDefect.Domain.Signals;

namespace SoundDefect.Infrastructure.Parsing
{
    /// <summary>
    /// Один из поддерживаемых форматов текстового файла с сигналами.
    /// </summary>
    public interface ITextSignalFormat
    {
        /// <summary>
        /// Подходит ли формат файлу, у которого в строках данных столько столбцов.
        /// </summary>
        bool CanRead(int columnCount);

        IReadOnlyList<Signal> Read(IReadOnlyList<DataRow> rows, string baseName);
    }
}
