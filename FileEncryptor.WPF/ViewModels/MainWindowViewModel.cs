using FileEncryptor.WPF.Infrastructure.Commands;
using FileEncryptor.WPF.Infrastructure.Commands.Base;
using FileEncryptor.WPF.Services.Interfaces;
using FileEncryptor.WPF.ViewModels.Base;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileEncryptor.WPF.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        private const string EncryptedFileMark = "(encrypted)";

        private const string DecryptedFileMark = "(decrypted)";

        private readonly IUserDialog _userDialog;

        private readonly IEncryptor _encryptor;

        private CancellationTokenSource _processCancellation;

        #region Title : string - Заголовок окна

        /// <summary>Заголовок окна.</summary>
        private string _title = "Encryptor";

        /// <summary>Заголовок окна.</summary>
        public string Title
        {
            get => _title; 
            
            set => Set(ref _title, value);
        }

        #endregion

        #region Password : string - Пароль

        /// <summary>
        /// Пароль.
        /// </summary>
        private string _password = "123";

        /// <summary>
        /// Пароль.
        /// </summary>
        public string Password
        {
            get => _password; 
            
            set => Set(ref _password, value);
        }

        #endregion

        #region SelectedFile : FileInfo - Выбранный файл

        /// <summary>
        /// Выбранный файл.
        /// </summary>
        private FileInfo _selectedFile;

        /// <summary>
        /// Выбранный файл.
        /// </summary>
        public FileInfo SelectedFile
        { 
            get => _selectedFile; 

            set => Set(ref _selectedFile, value);
        }

        #endregion

        #region ProgressValue : double - Значение прогресса операции

        /// <summary>
        /// Значение прогресса операции.
        /// </summary>
        private double _progressValue;

        /// <summary>
        /// Значение прогресса операции.
        /// </summary>
        public double ProgressValue
        {
            get => _progressValue;

            set => Set(ref _progressValue, value);
        }

        #endregion

        #region Команды

        #region SelectFileCommand - Команда выбора файла

        /// <summary>
        /// Команда выбора файла.
        /// </summary>
        private ICommand _selectFileCommand;

        /// <summary>
        /// Проверка возможности выполнения - Команда выбора файла.
        /// </summary>
        public ICommand SelectFileCommand => _selectFileCommand ??= 
            new RelayCommand(OnSelectFileCommandExecuted);

        /// <summary>
        /// Логика выполнения - Команда выбора файла.
        /// </summary>
        private void OnSelectFileCommandExecuted()
        {
            if (!_userDialog.OpenFile("Selecting a file", out string filePath))
            {
                return;
            }

            var selectedFile = new FileInfo(filePath);
            SelectedFile = selectedFile.Exists ? selectedFile : null;
        }

        #endregion

        #region Command EncryptCommand - Команда шифрования файла

        /// <summary>
        /// Команда шифрования файла.
        /// </summary>
        private ICommand _encryptCommand;

        /// <summary>
        /// Команда шифрования файла.
        /// </summary>
        public ICommand EncryptCommand => _encryptCommand ??=
            new RelayCommand(OnEncryptCommandExecuted, CanEncryptCommandExecute);

        /// <summary>
        /// Проверка возможности выполнения - Команда шифрования файла.
        /// </summary>
        private bool CanEncryptCommandExecute(object p) => (p is FileInfo { Exists: true } || 
            SelectedFile != null) && !string.IsNullOrWhiteSpace(Password);

        /// <summary>
        /// Логика выполнения - Команда шифрования файла.
        /// </summary>
        private async void OnEncryptCommandExecuted(object p)
        {
            FileInfo file = p as FileInfo ?? SelectedFile;

            if (file is null)
            {
                return;
            }

            string defaultFileName = file.FullName.Contains(DecryptedFileMark)
                ? file.FullName.Replace(DecryptedFileMark, EncryptedFileMark)
                : file.FullName.Insert(file.FullName.IndexOf('.'), " " + EncryptedFileMark);

            if (!_userDialog.SaveFile("Selecting a file for save", out string destinationPath,
                    defaultFileName))
            {
                return;
            }

            var timer = Stopwatch.StartNew();

            (IProgress<double> progressInfo, IProgress<string> statusInfo, 
                CancellationToken operationCancellation, Action closeWindow) = _userDialog
                .ShowProgress("File Encryption");
            statusInfo.Report($"{file.Name} encryption");

            _processCancellation = new CancellationTokenSource();
            CancellationToken cancellationToken = _processCancellation.Token;

            var combineCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, operationCancellation);

            ((Command)EncryptCommand).Executable = false;
            ((Command)DecryptCommand).Executable = false;

            try
            {
                await _encryptor.EncryptAsync(file.FullName, destinationPath, Password,
                    progress: progressInfo, cancellationToken: combineCancellation.Token);

                _userDialog.Information("Encryption", "File encryption completed " +
                                                      $"successfully in {timer.Elapsed.TotalSeconds:0.##} с");
            }
            catch (OperationCanceledException exc) when (exc.CancellationToken == combineCancellation
                                                             .Token)
            {
                _userDialog.Warning("Encryption Cancellation", "Encryption operation canceled " +
                                                                   "successfully");
            }
            finally
            {
                _processCancellation.Dispose();
                _processCancellation = null;
                closeWindow();
            }

            timer.Stop();

            ((Command)EncryptCommand).Executable = true;
            ((Command)DecryptCommand).Executable = true;
        }

        #endregion

        #region Command DecryptCommand - Команда дешифрования файла

        /// <summary>
        /// Команда дешифрования файла.
        /// </summary>
        private ICommand _decryptCommand;

        /// <summary>
        /// Команда дешифрования файла.
        /// </summary>
        public ICommand DecryptCommand => _decryptCommand ??=
            new RelayCommand(OnDecryptCommandExecuted, CanDecryptCommandExecute);

        /// <summary>
        /// Проверка возможности выполнения - Команда дешифрования файла.
        /// </summary>
        private bool CanDecryptCommandExecute(object p) => (p is FileInfo { Exists: true } || 
            SelectedFile != null) && !string.IsNullOrWhiteSpace(Password);

        /// <summary>
        /// Логика выполнения - Команда дешифрования файла.
        /// </summary>
        private async void OnDecryptCommandExecuted(object p)
        {
            FileInfo file = p as FileInfo ?? SelectedFile;

            if (file is null)
            {
                return;
            }

            string defaultFileName = file.FullName.Contains(EncryptedFileMark)
                ? file.FullName.Replace(EncryptedFileMark, DecryptedFileMark)
                : file.FullName.Insert(file.FullName.IndexOf('.'), " " + DecryptedFileMark);

            if (!_userDialog.SaveFile("Selecting a file to save", out string destinationPath, 
                    defaultFileName))
            {
                return;
            }

            var timer = Stopwatch.StartNew();
            
            _processCancellation = new CancellationTokenSource();
            CancellationToken cancellationToken = _processCancellation.Token;

            (IProgress<double> progressInfo, IProgress<string> statusInfo,
                CancellationToken operationCancellation, Action closeWindow) = _userDialog
                .ShowProgress("File Decryption");
            statusInfo.Report($"{file.Name} decryption");

            var combineCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, operationCancellation);

            ((Command)EncryptCommand).Executable = false;
            ((Command)DecryptCommand).Executable = false;

            try
            {
                Task<bool> decryptionTask = _encryptor.DecryptAsync(file.FullName, destinationPath,
                    Password, progress: progressInfo, cancellationToken: combineCancellation.Token);

                // Дополнительный код, выполняемый параллельно процессу дешифрования.

                bool success = await decryptionTask;

                if (success)
                {
                    _userDialog.Information("Decryption", "File decryption completed " +
                                                          $"successfully in {timer.Elapsed.TotalSeconds:0.##} с");
                }
                else
                {
                    _userDialog.Error("Decryption", $"{file.Name} decryption failed: invalid password");
                }
            }
            catch (OperationCanceledException exc) when (exc.CancellationToken == combineCancellation
                                                             .Token)
            {
                _userDialog.Warning("Decryption Cancellation", "Decryption operation canceled " +
                                                                   "successfully");
            }
            finally
            {
                _processCancellation.Dispose();
                _processCancellation = null;
                closeWindow();
            }

            timer.Stop();

            ((Command)EncryptCommand).Executable = true;
            ((Command)DecryptCommand).Executable = true;
        }

        #endregion

        #region Command CancelCommand - Команда отмены операции

        /// <summary>
        /// Команда отмены операции.
        /// </summary>
        private ICommand _cancelCommand;

        /// <summary>
        /// Команда отмены операции.
        /// </summary>
        public ICommand CancelCommand => _cancelCommand ??=
            new RelayCommand(OnCancelCommandExecuted, CanCancelCommandExecute);

        /// <summary>
        /// Проверка возможности выполнения - Команда отмены операции.
        /// </summary>
        private bool CanCancelCommandExecute(object p) => _processCancellation is 
            { IsCancellationRequested: false };

        /// <summary>
        /// Логика выполнения - Команда отмены операции.
        /// </summary>
        private void OnCancelCommandExecuted(object p) => _processCancellation.Cancel();

        #endregion

        #endregion

        public MainWindowViewModel(IUserDialog userDialog, IEncryptor encryptor)
        {
            _userDialog = userDialog;
            _encryptor = encryptor;
        }
    }
}
