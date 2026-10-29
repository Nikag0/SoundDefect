namespace SoundDefect.Dialogs
{
    /// <summary>
    /// Запрос подтверждения у пользователя. Отделён от ViewModel, чтобы её можно было тестировать без окон.
    /// </summary>
    public interface IConfirmationDialogService
    {
        /// <returns>true, если пользователь подтвердил действие.</returns>
        bool Confirm(string message, string title);
    }
}
