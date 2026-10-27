using System.Windows.Input;

namespace SoundDefect.ViewModel
{
    /// <summary>
    /// Команда для асинхронного действия. Пока действие выполняется, повторный запуск недоступен.
    /// Исключения действие должно обрабатывать само.
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private bool _isRunning;

        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
            => (_execute, _canExecute) = (execute, canExecute);

        public bool CanExecute(object? parameter) => !_isRunning && (_canExecute == null || _canExecute());

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            _isRunning = true;
            CommandManager.InvalidateRequerySuggested();
            try
            {
                await _execute();
            }
            finally
            {
                _isRunning = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
