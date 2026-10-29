using System.Windows.Input;

namespace SoundDefect.ViewModel
{
    /// <summary>
    /// Команда с параметром из CommandParameter. Если параметр не типа T, команда недоступна.
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;
        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null) => (_execute, _canExecute) = (execute, canExecute);
        public bool CanExecute(object? parameter) => parameter is T value && (_canExecute == null || _canExecute(value));
        public void Execute(object? parameter)
        {
            if (parameter is T value) _execute(value);
        }
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
