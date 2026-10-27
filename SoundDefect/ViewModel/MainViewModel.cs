using SoundDefect.Application.Import;
using SoundDefect.Application.Workspace;
using SoundDefect.Dialogs;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace SoundDefect.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ImportSignalsFromFile _importSignals;
        private readonly ISignalWorkspace _workspace;
        private readonly IFileDialogService _fileDialog;
        private readonly SignalColorPalette _palette = new();

        private string _statusMessage = "Готов к загрузке файла";
        private bool _isLoading;

        public MainViewModel(ImportSignalsFromFile importSignals, ISignalWorkspace workspace, IFileDialogService fileDialog)
        {
            _importSignals = importSignals;
            _workspace = workspace;
            _fileDialog = fileDialog;

            _workspace.Changed += OnWorkspaceChanged;

            LoadFileCommand = new AsyncRelayCommand(LoadFileAsync, () => !IsLoading);
            ClearCommand = new RelayCommand(ClearSignals, () => !IsLoading && Signals.Count > 0);
        }

        public ICommand LoadFileCommand { get; }
        public ICommand ClearCommand { get; }

        public ObservableCollection<SignalItemViewModel> Signals { get; } = new();

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetField(ref _isLoading, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private async Task LoadFileAsync()
        {
            string? filePath = _fileDialog.OpenSignalFile();
            if (filePath == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Чтение и парсинг файла...";

                var signals = await _importSignals.ExecuteAsync(filePath);

                StatusMessage = $"Файл «{Path.GetFileName(filePath)}» загружен. Сигналов: {signals.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при чтении файла: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ClearSignals()
        {
            _workspace.Clear();
            StatusMessage = "Графики удалены";
        }

        private void OnWorkspaceChanged(object? sender, SignalWorkspaceChangedEventArgs e)
        {
            switch (e.Change)
            {
                case SignalWorkspaceChange.Added:
                    foreach (var signal in e.Signals)
                        Signals.Add(new SignalItemViewModel(signal, _palette.Next()));
                    break;

                case SignalWorkspaceChange.Removed:
                    foreach (var signal in e.Signals)
                    {
                        var item = Signals.FirstOrDefault(s => s.Id == signal.Id);
                        if (item != null) Signals.Remove(item);
                    }
                    break;

                case SignalWorkspaceChange.Cleared:
                    Signals.Clear();
                    _palette.Reset();
                    break;
            }
        }
    }
}
