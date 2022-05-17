using FileEncryptor.WPF.Infrastructure.Commands;
using FileEncryptor.WPF.Services.Interfaces;
using FileEncryptor.WPF.ViewModels.Base;
using System.IO;
using System.Windows.Input;

namespace FileEncryptor.WPF.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        private readonly IUserDialog _userDialog;

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
        private void OnEncryptCommandExecuted(object p)
        {
            FileInfo file = p as FileInfo ?? SelectedFile;

            if (file is null)
            {
                return;
            }
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
        private void OnDecryptCommandExecuted(object p)
        {
            FileInfo file = p as FileInfo ?? SelectedFile;

            if (file is null)
            {
                return;
            }
        }

        #endregion

        #endregion

        public MainWindowViewModel(IUserDialog userDialog)
        {
            _userDialog = userDialog;
        }
    }
}
