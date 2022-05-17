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

        private ICommand _selectFileCommand;

        public ICommand SelectFileCommand => _selectFileCommand ??= 
            new RelayCommand(OnSelectFileCommandExecuted);

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

        public MainWindowViewModel(IUserDialog userDialog)
        {
            _userDialog = userDialog;
        }
    }
}
