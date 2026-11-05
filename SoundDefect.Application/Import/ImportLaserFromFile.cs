using SoundDefect.Application.Workspace;
using SoundDefect.Domain.Signals;

namespace SoundDefect.Application.Import
{
    /// <summary>
    /// Сценарий: прочитать сигнал лазера из файла и установить его в рабочую область.
    /// Файл разбирается тем же парсером, что и обычные сигналы, но должен содержать ровно один сигнал.
    /// </summary>
    public class ImportLaserFromFile
    {
        private readonly ISignalFileParser _parser;
        private readonly ISignalWorkspace _workspace;

        public ImportLaserFromFile(ISignalFileParser parser, ISignalWorkspace workspace)
        {
            _parser = parser;
            _workspace = workspace;
        }

        public async Task<Signal> ExecuteAsync(string filePath, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Signal> signals = await _parser.ParseAsync(filePath, cancellationToken);

            if (signals.Count != 1)
                throw new InvalidOperationException($"Файл лазера должен содержать один сигнал, а в нём найдено: {signals.Count}.");

            _workspace.SetLaser(signals[0]);

            return signals[0];
        }
    }
}
