using Microsoft.Extensions.DependencyInjection;

namespace WpfDR.ViewModels
{
    internal class ViewModelLocator
    {
        public MainWindowViewModel MainWindowModel => App.Services.GetRequiredService<MainWindowViewModel>();
        public FileListWindowViewModel FileListWindowModel => App.Services.GetRequiredService<FileListWindowViewModel>();
        public FileRepackWindowViewModel FileRepackWindowModel => App.Services.GetRequiredService<FileRepackWindowViewModel>();
        public SqlImporterViewModel SqlImporterModel => App.Services.GetRequiredService<SqlImporterViewModel>();

    }
}
