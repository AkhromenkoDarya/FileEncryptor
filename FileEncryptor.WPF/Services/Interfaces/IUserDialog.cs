using System;
using System.Collections.Generic;
using System.Threading;

namespace FileEncryptor.WPF.Services.Interfaces
{
    internal interface IUserDialog
    {
        bool OpenFile(string title, out string selectedFile, string filter = "Все файлы (*.*)|*.*");

        bool OpenFiles(string title, out IEnumerable<string> selectedFiles, string filter = 
            "Все файлы (*.*)|*.*");

        bool SaveFile(string title, out string selectedFile, string defaultFileName = null, 
            string filter = "Все файлы (*.*)|*.*");

        void Information(string title, string message);

        void Warning(string title, string message);

        void Error(string title, string message);

        (IProgress<double> progress, IProgress<string> status, CancellationToken token, 
            Action close) ShowProgress(string title);
    }
}
