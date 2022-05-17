using FileEncryptor.WPF.Services.Interfaces;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;

namespace FileEncryptor.WPF.Services
{
    internal class UserDialogService : IUserDialog
    {
        public bool OpenFile(string title, out string selectedFile, string filter = 
            "Все файлы (*.*)|*.*")
        {
            var fileDialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter
            };

            if (fileDialog.ShowDialog() != true)
            {
                selectedFile = null;
                return false;
            }

            selectedFile = fileDialog.FileName;

            return true;
        }

        public bool OpenFiles(string title, out IEnumerable<string> selectedFiles, string filter = 
            "Все файлы (*.*)|*.*")
        {
            var fileDialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter
            };

            if (fileDialog.ShowDialog() != true)
            {
                selectedFiles = Enumerable.Empty<string>();
                return false;
            }

            selectedFiles = fileDialog.FileNames;

            return true;
        }
    }
}
