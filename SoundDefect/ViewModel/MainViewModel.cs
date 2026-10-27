using SoundDefect.Application.Import;
using SoundDefect.Application.Normalization;
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
        private bool _isNormalized;
        private INormalizationMethod _selectedNormalizationMethod;

        public MainViewModel(
            ImportSignalsFromFile importSignals,
            ISignalWorkspace workspace,
            IFileDialogService fileDialog,
            IEnumerable<INormalizationMethod> normalizationMethods)
        {
            _importSignals = importSignals;
            _workspace = workspace;
            _fileDialog = fileDialog;

            NormalizationMethods = normalizationMethods.ToList();
            if (NormalizationMethods.Count == 0)
                throw new ArgumentException("Не зарегистрировано ни одного способа нормализации.", nameof(normalizationMethods));
            _selectedNormalizationMethod = NormalizationMethods[0];

            _workspace.Changed += OnWorkspaceChanged;

            LoadFileCommand = new AsyncRelayCommand(LoadFileAsync, () => !IsLoading);
            ClearCommand = new RelayCommand(ClearSignals, () => !IsLoading && Signals.Count > 0);
        }

        public ICommand LoadFileCommand { get; }
        public ICommand ClearCommand { get; }

        public ObservableCollection<SignalItemViewModel> Signals { get; } = new();

        public IReadOnlyList<INormalizationMethod> NormalizationMethods { get; }

        /// <summary>
        /// true — на графике нормализованные сигналы, false — исходные.
        /// </summary>
        public bool IsNormalized
        {
            get => _isNormalized;
            set
            {
                if (!SetField(ref _isNormalized, value)) return;
                OnPropertyChanged(nameof(ShowOriginal));
                ApplyDisplayMode();
            }
        }

        /// <summary>
        /// Обратное к IsNormalized, для привязки переключателя «Исходные».
        /// </summary>
        public bool ShowOriginal
        {
            get => !IsNormalized;
            set => IsNormalized = !value;
        }

        public INormalizationMethod SelectedNormalizationMethod
        {
            get => _selectedNormalizationMethod;
            set
            {
                if (value == null || !SetField(ref _selectedNormalizationMethod, value)) return;
                if (IsNormalized) ApplyDisplayMode();
            }
        }

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

                    // Набор изменился — общие характеристики для нормализации тоже
                    if (IsNormalized) ApplyDisplayMode();
                    break;

                case SignalWorkspaceChange.Removed:
                    foreach (var signal in e.Signals)
                    {
                        var item = Signals.FirstOrDefault(s => s.Id == signal.Id);
                        if (item != null) Signals.Remove(item);
                    }

                    if (IsNormalized) ApplyDisplayMode();
                    break;

                case SignalWorkspaceChange.Cleared:
                    Signals.Clear();
                    _palette.Reset();
                    break;
            }
        }

        /// <summary>
        /// Показывает на графике исходные или нормализованные значения в зависимости от режима.
        /// Нормализация считается по всем загруженным сигналам, а не только по видимым,
        /// чтобы скрытие сигнала не меняло масштаб остальных.
        /// </summary>
        private void ApplyDisplayMode()
        {
            if (!IsNormalized)
            {
                foreach (var item in Signals)
                    item.ShowOriginal();
                return;
            }

            var normalized = SelectedNormalizationMethod.Normalize(_workspace.Signals);

            foreach (var item in Signals)
            {
                if (normalized.TryGetValue(item.Id, out var ys))
                    item.ShowValues(ys);
            }
        }
    }
}
