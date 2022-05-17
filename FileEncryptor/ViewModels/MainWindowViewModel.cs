using FileEncryptor.ViewModels.Base;

namespace FileEncryptor.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
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
    }
}
