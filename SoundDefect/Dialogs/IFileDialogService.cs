namespace SoundDefect.Dialogs
{
    /// <summary>
    /// Диалоги выбора файлов. Отделены от ViewModel, чтобы её можно было тестировать без окон.
    /// </summary>
    public interface IFileDialogService
    {
        /// <returns>Путь к выбранному файлу или null, если пользователь отменил выбор.</returns>
        string? OpenSignalFile();
    }
}
