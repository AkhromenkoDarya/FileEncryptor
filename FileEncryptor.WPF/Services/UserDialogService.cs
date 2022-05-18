using FileEncryptor.WPF.Services.Interfaces;
using FileEncryptor.WPF.Views.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace FileEncryptor.WPF.Services
{
    internal class UserDialogService : IUserDialog
    {
        public bool OpenFile(string title, out string selectedFilePath, string filter = 
            "Все файлы (*.*)|*.*")
        {
            var fileDialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter
            };

            if (fileDialog.ShowDialog() != true)
            {
                selectedFilePath = null;
                return false;
            }

            selectedFilePath = fileDialog.FileName;

            return true;
        }

        public bool OpenFiles(string title, out IEnumerable<string> selectedFilePaths, 
            string filter = "Все файлы (*.*)|*.*")
        {
            var fileDialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter
            };

            if (fileDialog.ShowDialog() != true)
            {
                selectedFilePaths = Enumerable.Empty<string>();
                return false;
            }

            selectedFilePaths = fileDialog.FileNames;

            return true;
        }

        public bool SaveFile(string title, out string selectedFilePath, string defaultFilePath = 
                null, string filter = "Все файлы (*.*)|*.*")
        {
            var fileDialog = new SaveFileDialog
            {
                Title = title,
                Filter = filter
            };

            if (!string.IsNullOrWhiteSpace(defaultFilePath))
            {
                fileDialog.FileName = defaultFilePath;
            }

            if (fileDialog.ShowDialog() != true)
            {
                selectedFilePath = null;
                return false;
            }

            selectedFilePath = fileDialog.FileName;

            return true;
        }

        public void Information(string title, string message) => MessageBox.Show(message, title, 
            MessageBoxButton.OK, MessageBoxImage.Information);

        public void Warning(string title, string message) => MessageBox.Show(message, title, 
            MessageBoxButton.OK, MessageBoxImage.Warning);

        public void Error(string title, string message) => MessageBox.Show(message, title, 
            MessageBoxButton.OK, MessageBoxImage.Error);

        public (IProgress<double> progress, IProgress<string> status, CancellationToken token, 
            Action close) ShowProgress(string title)
        {
            var progressWindow = new ProgressWindow 
            { 
                Title = title, 
                Owner = App.FocusedWindow, 
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            
            progressWindow.Show();

            return (progressWindow.ProgressInformation, progressWindow.StatusInformation, 
                progressWindow.CancellationToken, progressWindow.Close);
        }
    }
}
