using SoundDefect.Application.Import;
using SoundDefect.Application.Laser;
using SoundDefect.Application.Normalization;
using SoundDefect.Application.Workspace;
using SoundDefect.Dialogs;

namespace SoundDefect.ViewModel
{
    /// <summary>
    /// Создаёт вкладки с графиком. У каждой вкладки своя рабочая область,
    /// поэтому сигналы разных вкладок друг от друга не зависят.
    /// </summary>
    public class ChartTabFactory
    {
        private readonly ISignalFileParser _parser;
        private readonly LaserActivityDetector _laserDetector;
        private readonly IFileDialogService _fileDialog;
        private readonly IConfirmationDialogService _confirmationDialog;
        private readonly IReadOnlyList<INormalizationMethod> _normalizationMethods;

        public ChartTabFactory(
            ISignalFileParser parser,
            LaserActivityDetector laserDetector,
            IFileDialogService fileDialog,
            IConfirmationDialogService confirmationDialog,
            IEnumerable<INormalizationMethod> normalizationMethods)
        {
            _parser = parser;
            _laserDetector = laserDetector;
            _fileDialog = fileDialog;
            _confirmationDialog = confirmationDialog;
            _normalizationMethods = normalizationMethods.ToList();
        }

        public ChartTabViewModel Create(string title)
        {
            var workspace = new SignalWorkspace();
            var importSignals = new ImportSignalsFromFile(_parser, workspace);
            var importLaser = new ImportLaserFromFile(_parser, workspace);

            return new ChartTabViewModel(
                title, importSignals, importLaser, _laserDetector, workspace,
                _fileDialog, _confirmationDialog, _normalizationMethods);
        }
    }
}
