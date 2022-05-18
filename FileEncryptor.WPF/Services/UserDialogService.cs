using System;
using FileEncryptor.WPF.Services.Interfaces;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using FileEncryptor.WPF.Views.Windows;

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

        public bool SaveFile(string title, out string selectedFile, string defaultFileName = null, 
            string filter = "Все файлы (*.*)|*.*")
        {
            var fileDialog = new SaveFileDialog
            {
                Title = title,
                Filter = filter
            };

            if (!string.IsNullOrWhiteSpace(defaultFileName))
            {
                fileDialog.FileName = defaultFileName;
            }

            if (fileDialog.ShowDialog() != true)
            {
                selectedFile = null;
                return false;
            }

            selectedFile = fileDialog.FileName;

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
