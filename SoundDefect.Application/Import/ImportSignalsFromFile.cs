using SoundDefect.Application.Workspace;
using SoundDefect.Domain.Signals;

namespace SoundDefect.Application.Import
{
    /// <summary>
    /// Сценарий: прочитать сигналы из файла и добавить их в рабочую область.
    /// </summary>
    public class ImportSignalsFromFile
    {
        private readonly ISignalFileParser _parser;
        private readonly ISignalWorkspace _workspace;

        public ImportSignalsFromFile(ISignalFileParser parser, ISignalWorkspace workspace)
        {
            _parser = parser;
            _workspace = workspace;
        }

        public async Task<IReadOnlyList<Signal>> ExecuteAsync(string filePath, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Signal> signals = await _parser.ParseAsync(filePath, cancellationToken);

            _workspace.Add(signals);

            return signals;
        }
    }
}
