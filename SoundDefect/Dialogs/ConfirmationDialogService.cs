using System.Windows;

namespace SoundDefect.Dialogs
{
    public class ConfirmationDialogService : IConfirmationDialogService
    {
        public bool Confirm(string message, string title)
        {
            // По умолчанию выбрано «Нет», чтобы случайное нажатие Enter ничего не удалило
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            return result == MessageBoxResult.Yes;
        }
    }
}
