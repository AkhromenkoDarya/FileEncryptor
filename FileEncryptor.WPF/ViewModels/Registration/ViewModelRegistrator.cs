using Microsoft.Extensions.DependencyInjection;

namespace FileEncryptor.WPF.ViewModels.Registration
{
    internal static class ViewModelRegistrator
    {
        public static IServiceCollection AddViewModels(this IServiceCollection services) => services
            .AddSingleton<MainWindowViewModel>();
    }
}
