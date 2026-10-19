using SoundDeffect.Domain;
using SoundDeffect.Service;
using SoundDeffect.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SoundDeffect.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly FileReader _fileReader;
        private string _statusMessage = "Готов к загрузке файла";
        private Dataset? _currentDataset;
        private bool _isLoading;

        public event Action<Dataset, int[]>? RequestPlotUpdate;
        public event Action<Dataset>? DatasetLoaded;
        public event Action? ChartsCleared;
        public MainViewModel(FileReader fileReader)
        {
            _fileReader = fileReader;
            LoadFileCommand = new RelayCommand(async () => await LoadFileAsync(), () => !IsLoading);
            ClearCommand = new RelayCommand(ClearCharts, () => !IsLoading);
        }

        public ICommand LoadFileCommand { get; }
        public ICommand ClearCommand { get; }

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

        public ObservableCollection<ChannelToggleViewModel> ChannelToggles { get; } = new();

        private async Task LoadFileAsync()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Выберите текстовый файл с данными графиков",
                Filter = "Текстовые файлы (*.txt)|*.txt|CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    IsLoading = true;
                    StatusMessage = "Чтение и парсинг файла...";
                    ClearToggles();

                    _currentDataset = await _fileReader.ParseFileAsync(openFileDialog.FileName);

                    foreach (var channelId in _currentDataset.Channels.Keys)
                    {
                        var toggle = new ChannelToggleViewModel(channelId);
                        toggle.IsVisibleChanged += UpdateActiveCharts;
                        ChannelToggles.Add(toggle);
                    }

                    StatusMessage = $"Файл успешно загружен. Найдено каналов: {_currentDataset.Channels.Count}";

                    DatasetLoaded?.Invoke(_currentDataset);
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
        }

        private void ClearCharts()
        {
            _currentDataset = null;
            ClearToggles();
            StatusMessage = "График очищен";
            RequestPlotUpdate?.Invoke(new Dataset(), Array.Empty<int>());
        }

        private void UpdateActiveCharts()
        {
            if (_currentDataset == null) return;

            int[] activeIds = ChannelToggles
                .Where(t => t.IsVisible)
                .Select(t => t.ChannelId)
                .ToArray();

            RequestPlotUpdate?.Invoke(_currentDataset, activeIds);
        }

        private void ClearToggles()
        {
            foreach (var toggle in ChannelToggles)
            {
                toggle.IsVisibleChanged -= UpdateActiveCharts;
            }
            ChannelToggles.Clear();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
