using Microsoft.Extensions.DependencyInjection;

namespace FileEncryptor.WPF.ViewModels.Locator
{
    internal class ViewModelLocator
    {
        public MainWindowViewModel MainWindowModel => App.Services
            .GetRequiredService<MainWindowViewModel>();
    }
}
