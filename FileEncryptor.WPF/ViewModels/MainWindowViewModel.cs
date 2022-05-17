using System;
using FileEncryptor.WPF.Infrastructure.Commands;
using FileEncryptor.WPF.Services.Interfaces;
using FileEncryptor.WPF.ViewModels.Base;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using FileEncryptor.WPF.Infrastructure.Commands.Base;

namespace FileEncryptor.WPF.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        private const string EncryptedFileSuffix = ".encrypted";

        private readonly IUserDialog _userDialog;

        private readonly IEncryptor _encryptor;

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

            string defaultFileName = file.FullName + EncryptedFileSuffix;

            if (!_userDialog.SaveFile("Selecting a file for save", out string destinationPath,
                    defaultFileName))
            {
                return;
            }

            var timer = Stopwatch.StartNew();
            ((Command)EncryptCommand).Executable = false;
            ((Command)DecryptCommand).Executable = false;

            try
            {
                await _encryptor.EncryptAsync(file.FullName, destinationPath, Password);
            }
            catch (OperationCanceledException)
            {

            }

            ((Command)EncryptCommand).Executable = true;
            ((Command)DecryptCommand).Executable = true;
            timer.Stop();

            _userDialog.Information("Encryption", "File encryption completed successfully in " +
                                                  $"{timer.Elapsed.TotalSeconds:0.##} sec");
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

            string defaultFileName = file.FullName.EndsWith(EncryptedFileSuffix)
                ? file.FullName[..^EncryptedFileSuffix.Length]
                : file.FullName;

            if (!_userDialog.SaveFile("Selecting a file to save", out string destinationPath, 
                    defaultFileName))
            {
                return;
            }

            var timer = Stopwatch.StartNew();
            ((Command)EncryptCommand).Executable = false;
            ((Command)DecryptCommand).Executable = false;

            Task<bool> decryptionTask = _encryptor.DecryptAsync(file.FullName, destinationPath, 
                Password);
            // Дополнительный код, выполняемый параллельно процессу дешифрования.

            var success = false;

            try
            {
                success = await decryptionTask;
            }
            catch (OperationCanceledException)
            {

            }

            ((Command)EncryptCommand).Executable = true;
            ((Command)DecryptCommand).Executable = true;
            timer.Stop();

            if (success)
            {
                _userDialog.Information("Decryption", "File decryption completed successfully in " +
                                                      $"{timer.Elapsed.TotalSeconds:0.##} с");
                return;
            }

            _userDialog.Warning("Decryption", "File decryption error: invalid password");
        }

        #endregion

        #endregion

        public MainWindowViewModel(IUserDialog userDialog, IEncryptor encryptor)
        {
            _userDialog = userDialog;
            _encryptor = encryptor;
        }
    }
}
