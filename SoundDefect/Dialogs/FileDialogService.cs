using Microsoft.Win32;

namespace SoundDefect.Dialogs
{
    public class FileDialogService : IFileDialogService
    {
        public string? OpenSignalFile()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите текстовый файл с данными графиков",
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                Multiselect = false
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
