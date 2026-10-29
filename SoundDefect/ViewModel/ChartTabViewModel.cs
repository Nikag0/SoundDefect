using SoundDefect.Application.Import;
using SoundDefect.Application.Laser;
using SoundDefect.Application.Normalization;
using SoundDefect.Application.Workspace;
using SoundDefect.Dialogs;
using SoundDefect.Domain.Laser;
using SoundDefect.Domain.Signals;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;

namespace SoundDefect.ViewModel
{
    /// <summary>
    /// Одна вкладка с графиком: свой набор сигналов, свой лазер, свой режим отображения и статус.
    /// </summary>
    public class ChartTabViewModel : ViewModelBase
    {
        private static readonly Color LaserColor = Color.FromRgb(29, 158, 117); // Зелёный

        private readonly ImportSignalsFromFile _importSignals;
        private readonly ImportLaserFromFile _importLaser;
        private readonly LaserActivityDetector _laserDetector;
        private readonly ISignalWorkspace _workspace;
        private readonly IFileDialogService _fileDialog;
        private readonly IConfirmationDialogService _confirmationDialog;
        private readonly SignalColorPalette _palette = new();

        private string _title;
        private bool _isSelected;
        private string _statusMessage = "Готов к загрузке файла";
        private bool _isLoading;
        private bool _isNormalized;
        private INormalizationMethod _selectedNormalizationMethod;

        private SignalItemViewModel? _laser;
        private double _laserThreshold;
        private double _laserMinDuration = LaserDetectionSettings.DefaultMinDuration;
        private LaserActivity? _laserActivity;
        private LaserFilter _laserFilter = LaserFilter.All;
        private bool _highlightLaserIntervals = true;

        public ChartTabViewModel(
            string title,
            ImportSignalsFromFile importSignals,
            ImportLaserFromFile importLaser,
            LaserActivityDetector laserDetector,
            ISignalWorkspace workspace,
            IFileDialogService fileDialog,
            IConfirmationDialogService confirmationDialog,
            IEnumerable<INormalizationMethod> normalizationMethods)
        {
            _title = title;
            _importSignals = importSignals;
            _importLaser = importLaser;
            _laserDetector = laserDetector;
            _workspace = workspace;
            _fileDialog = fileDialog;
            _confirmationDialog = confirmationDialog;

            NormalizationMethods = normalizationMethods.ToList();
            if (NormalizationMethods.Count == 0)
                throw new ArgumentException("Не зарегистрировано ни одного способа нормализации.", nameof(normalizationMethods));
            _selectedNormalizationMethod = NormalizationMethods[0];

            _workspace.Changed += OnWorkspaceChanged;

            LoadFileCommand = new AsyncRelayCommand(LoadFileAsync, () => !IsLoading);
            ClearCommand = new RelayCommand(ClearSignals, () => !IsLoading && Signals.Count > 0);
            RemoveSignalCommand = new RelayCommand<SignalItemViewModel>(RemoveSignal, _ => !IsLoading);
            LoadLaserCommand = new AsyncRelayCommand(LoadLaserAsync, () => !IsLoading);
            RemoveLaserCommand = new RelayCommand(RemoveLaser, () => !IsLoading && HasLaser);
        }

        public string Title
        {
            get => _title;
            set => SetField(ref _title, value);
        }

        /// <summary>
        /// Выбрана ли вкладка сейчас. Управляется из MainViewModel, используется для показа содержимого.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value);
        }

        public ICommand LoadFileCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand RemoveSignalCommand { get; }
        public ICommand LoadLaserCommand { get; }
        public ICommand RemoveLaserCommand { get; }

        /// <summary>Масштаб графика: границы осей и их фиксация.</summary>
        public PlotScaleViewModel Scale { get; } = new();

        /// <summary>Обычные сигналы — для списка в левой панели.</summary>
        public ObservableCollection<SignalItemViewModel> Signals { get; } = new();

        /// <summary>Всё, что может быть нарисовано на графике: обычные сигналы и лазер.</summary>
        public ObservableCollection<SignalItemViewModel> PlotSignals { get; } = new();

        public IReadOnlyList<INormalizationMethod> NormalizationMethods { get; }

        #region Отображение и нормализация

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

        #endregion

        #region Лазер

        /// <summary>Лазер как сигнал: его кривую можно показать на графике. null, если лазер не загружен.</summary>
        public SignalItemViewModel? Laser
        {
            get => _laser;
            private set
            {
                if (!SetField(ref _laser, value)) return;
                OnPropertyChanged(nameof(HasLaser));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool HasLaser => Laser != null;

        /// <summary>Значение сигнала лазера, начиная с которого он считается включённым.</summary>
        public double LaserThreshold
        {
            get => _laserThreshold;
            set
            {
                if (!SetField(ref _laserThreshold, value)) return;
                DetectLaserActivity();
            }
        }

        /// <summary>Включения и выключения короче этого значения (по X) считаются скачками и отбрасываются.</summary>
        public double LaserMinDuration
        {
            get => _laserMinDuration;
            set
            {
                if (value < 0) value = 0;
                if (!SetField(ref _laserMinDuration, value)) return;
                DetectLaserActivity();
            }
        }

        /// <summary>Распознанные интервалы работы лазера. null, если лазер не загружен.</summary>
        public LaserActivity? LaserActivity
        {
            get => _laserActivity;
            private set
            {
                if (!SetField(ref _laserActivity, value)) return;
                OnPropertyChanged(nameof(LaserSummary));
            }
        }

        public string LaserSummary => LaserActivity == null
            ? string.Empty
            : $"Найдено включений: {LaserActivity.OnIntervals.Count}, отброшено скачков: {LaserActivity.DiscardedSpikes}";

        public LaserFilter LaserFilter
        {
            get => _laserFilter;
            set
            {
                if (!SetField(ref _laserFilter, value)) return;
                OnPropertyChanged(nameof(ShowAllData));
                OnPropertyChanged(nameof(ShowLaserOn));
                OnPropertyChanged(nameof(ShowLaserOff));
                ApplyDisplayMode();
            }
        }

        // Свойства для привязки переключателей: снятие отметки игнорируется, его делает соседний переключатель
        public bool ShowAllData
        {
            get => LaserFilter == LaserFilter.All;
            set { if (value) LaserFilter = LaserFilter.All; }
        }

        public bool ShowLaserOn
        {
            get => LaserFilter == LaserFilter.LaserOn;
            set { if (value) LaserFilter = LaserFilter.LaserOn; }
        }

        public bool ShowLaserOff
        {
            get => LaserFilter == LaserFilter.LaserOff;
            set { if (value) LaserFilter = LaserFilter.LaserOff; }
        }

        /// <summary>Подсвечивать ли на графике интервалы, когда лазер включён.</summary>
        public bool HighlightLaserIntervals
        {
            get => _highlightLaserIntervals;
            set => SetField(ref _highlightLaserIntervals, value);
        }

        #endregion

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

        private async Task LoadLaserAsync()
        {
            string? filePath = _fileDialog.OpenSignalFile();
            if (filePath == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Чтение файла лазера...";

                await _importLaser.ExecuteAsync(filePath);

                StatusMessage = $"Лазер «{Path.GetFileName(filePath)}» загружен. {LaserSummary}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при чтении файла лазера: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ClearSignals()
        {
            bool confirmed = _confirmationDialog.Confirm(
                $"Удалить все сигналы ({Signals.Count}) со вкладки «{Title}»?",
                "Удаление сигналов");
            if (!confirmed) return;

            _workspace.Clear();
            StatusMessage = "Графики удалены";
        }

        private void RemoveSignal(SignalItemViewModel item)
        {
            if (_workspace.Remove(item.Id))
                StatusMessage = $"Сигнал «{item.Name}» удалён";
        }

        private void RemoveLaser()
        {
            if (_workspace.RemoveLaser())
                StatusMessage = "Лазер удалён";
        }

        private void OnWorkspaceChanged(object? sender, SignalWorkspaceChangedEventArgs e)
        {
            switch (e.Change)
            {
                case SignalWorkspaceChange.Added:
                    foreach (var signal in e.Signals)
                    {
                        var item = new SignalItemViewModel(signal, _palette.Next());
                        Signals.Add(item);
                        AddToPlot(item);
                    }
                    break;

                case SignalWorkspaceChange.Removed:
                    foreach (var signal in e.Signals)
                    {
                        var item = Signals.FirstOrDefault(s => s.Id == signal.Id);
                        if (item == null) continue;

                        Signals.Remove(item);
                        RemoveFromPlot(item);
                    }
                    break;

                case SignalWorkspaceChange.Cleared:
                    foreach (var item in Signals)
                        RemoveFromPlot(item);
                    Signals.Clear();
                    _palette.Reset();
                    break;

                case SignalWorkspaceChange.LaserSet:
                    SetLaser(e.Signals[0]);
                    break;

                case SignalWorkspaceChange.LaserRemoved:
                    if (Laser != null) RemoveFromPlot(Laser);
                    Laser = null;
                    LaserActivity = null;
                    break;
            }

            ApplyDisplayMode();
        }

        private void SetLaser(Signal laser)
        {
            if (Laser != null) RemoveFromPlot(Laser);

            // Кривая лазера по умолчанию скрыта: его значения на порядки больше и растянули бы ось Y
            var item = new SignalItemViewModel(laser, LaserColor, isVisible: false);
            Laser = item;
            AddToPlot(item);

            // Порог задаём напрямую в поле, чтобы распознавание не запускалось дважды
            _laserThreshold = _laserDetector.SuggestThreshold(laser);
            OnPropertyChanged(nameof(LaserThreshold));
            DetectLaserActivity();
        }

        private void AddToPlot(SignalItemViewModel item)
        {
            item.PropertyChanged += OnPlotSignalPropertyChanged;
            PlotSignals.Add(item);
        }

        private void RemoveFromPlot(SignalItemViewModel item)
        {
            item.PropertyChanged -= OnPlotSignalPropertyChanged;
            PlotSignals.Remove(item);
        }

        private void OnPlotSignalPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Нормализация считается по видимым сигналам, поэтому смена видимости меняет и остальные кривые
            if (e.PropertyName == nameof(SignalItemViewModel.IsVisible) && IsNormalized)
                ApplyDisplayMode();
        }

        private void DetectLaserActivity()
        {
            if (_workspace.Laser == null) return;

            LaserActivity = _laserDetector.Detect(_workspace.Laser, new LaserDetectionSettings(LaserThreshold, LaserMinDuration));

            if (LaserFilter != LaserFilter.All || IsNormalized)
                ApplyDisplayMode();
        }

        /// <summary>
        /// Пересчитывает, что рисуется на графике:
        /// 1. Фильтр по лазеру оставляет только участки, где лазер включён или выключен.
        /// 2. Нормализация (если включена) считается только по тому, что видно на графике:
        ///    по видимым сигналам и только по участкам, прошедшим фильтр.
        /// </summary>
        private void ApplyDisplayMode()
        {
            var ranges = PlotSignals.ToDictionary(item => item, FindDisplayedRanges);

            IReadOnlyDictionary<SignalId, double[]>? normalized = null;
            if (IsNormalized)
            {
                var displayedSignals = PlotSignals
                    .Where(item => item.IsVisible && ranges[item].Count > 0)
                    .Select(item => Signal.Create(
                        item.Id,
                        item.Name,
                        Concat(item.OriginalXs, ranges[item]),
                        Concat(item.OriginalYs, ranges[item])))
                    .ToList();

                normalized = SelectedNormalizationMethod.Normalize(displayedSignals);
            }

            foreach (var item in PlotSignals)
            {
                // Нормализованные значения идут подряд по всем участкам, поэтому разрезаем их так же, как исходные
                double[]? normalizedYs = null;
                normalized?.TryGetValue(item.Id, out normalizedYs);

                var segments = new List<SignalSegment>(ranges[item].Count);
                int offset = 0;

                foreach (var (start, count) in ranges[item])
                {
                    double[] xs = Slice(item.OriginalXs, start, count);
                    double[] ys = normalizedYs != null
                        ? Slice(normalizedYs, offset, count)
                        : Slice(item.OriginalYs, start, count);

                    segments.Add(new SignalSegment(xs, ys));
                    offset += count;
                }

                item.ShowSegments(segments);
            }
        }

        /// <summary>
        /// Диапазоны точек сигнала, которые проходят фильтр по лазеру.
        /// Время сигналов и лазера не выравнивается: состояние лазера берётся по X точки сигнала как есть.
        /// </summary>
        private IReadOnlyList<(int Start, int Count)> FindDisplayedRanges(SignalItemViewModel item)
        {
            if (LaserFilter == LaserFilter.All || LaserActivity == null)
                return new[] { (0, item.OriginalXs.Length) };

            var state = LaserFilter == LaserFilter.LaserOn ? LaserState.On : LaserState.Off;
            return LaserActivity.FindRanges(item.OriginalXs, state);
        }

        private static double[] Concat(double[] source, IReadOnlyList<(int Start, int Count)> ranges)
        {
            var result = new double[ranges.Sum(r => r.Count)];
            int offset = 0;

            foreach (var (start, count) in ranges)
            {
                Array.Copy(source, start, result, offset, count);
                offset += count;
            }

            return result;
        }

        private static double[] Slice(double[] source, int start, int count)
        {
            if (start == 0 && count == source.Length) return source;

            var result = new double[count];
            Array.Copy(source, start, result, 0, count);
            return result;
        }
    }
}
