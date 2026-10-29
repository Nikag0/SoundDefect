using System.Windows.Input;

namespace SoundDefect.ViewModel
{
    /// <summary>
    /// Масштаб графика вкладки: текущие границы осей и фиксация каждой оси.
    /// Зафиксированная ось не меняется при автоматическом масштабировании
    /// (загрузка сигналов, смена участков лазера, нормализация), но её можно двигать мышью или задать вручную.
    /// </summary>
    public class PlotScaleViewModel : ViewModelBase
    {
        private bool _isXLocked;
        private bool _isYLocked;
        private double _xMin, _xMax = 10, _yMin, _yMax = 10;

        public PlotScaleViewModel()
        {
            FitCommand = new RelayCommand(() => FitRequested?.Invoke(this, EventArgs.Empty));
        }

        /// <summary>Просьба к графику вписать все данные. Выполняет представление, которое знает о графике.</summary>
        public event EventHandler? FitRequested;

        /// <summary>Пользователь вручную ввёл границы оси. Представление применяет их к графику.</summary>
        public event EventHandler? RangeEdited;

        public ICommand FitCommand { get; }

        public bool IsXLocked
        {
            get => _isXLocked;
            set => SetField(ref _isXLocked, value);
        }

        public bool IsYLocked
        {
            get => _isYLocked;
            set => SetField(ref _isYLocked, value);
        }

        // Ручной ввод границы фиксирует ось: иначе следующий автомасштаб сразу отменил бы введённое значение.
        // Значение, при котором начало не меньше конца, не принимается.

        public double XMin
        {
            get => _xMin;
            set => EditRange(ref _xMin, value, value < _xMax, isXAxis: true);
        }

        public double XMax
        {
            get => _xMax;
            set => EditRange(ref _xMax, value, value > _xMin, isXAxis: true);
        }

        public double YMin
        {
            get => _yMin;
            set => EditRange(ref _yMin, value, value < _yMax, isXAxis: false);
        }

        public double YMax
        {
            get => _yMax;
            set => EditRange(ref _yMax, value, value > _yMin, isXAxis: false);
        }

        /// <summary>
        /// Обновляет показанные границы по фактическому состоянию графика (без фиксации осей и без RangeEdited).
        /// </summary>
        public void UpdateFromPlot(double xMin, double xMax, double yMin, double yMax)
        {
            SetField(ref _xMin, xMin, nameof(XMin));
            SetField(ref _xMax, xMax, nameof(XMax));
            SetField(ref _yMin, yMin, nameof(YMin));
            SetField(ref _yMax, yMax, nameof(YMax));
        }

        private void EditRange(ref double field, double value, bool isValid, bool isXAxis)
        {
            if (field == value) return;

            if (double.IsFinite(value) && isValid)
            {
                field = value;

                if (isXAxis) IsXLocked = true;
                else IsYLocked = true;
            }

            // И при принятом, и при отклонённом значении обновляем поля ввода оси:
            // при отказе в поле вернётся прежнее значение
            OnPropertyChanged(isXAxis ? nameof(XMin) : nameof(YMin));
            OnPropertyChanged(isXAxis ? nameof(XMax) : nameof(YMax));

            if (field == value)
                RangeEdited?.Invoke(this, EventArgs.Empty);
        }
    }
}
