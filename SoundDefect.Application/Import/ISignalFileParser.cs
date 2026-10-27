using SoundDefect.Domain.Signals;

namespace SoundDefect.Application.Import
{
    /// <summary>
    /// Читает сигналы из файла. Формат файла определяет реализация.
    /// </summary>
    public interface ISignalFileParser
    {
        Task<IReadOnlyList<Signal>> ParseAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
