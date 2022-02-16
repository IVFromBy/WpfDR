using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using WpfDR.Commands;
using WpfDR.Model;
using WpfDR.Service;
using WpfDR.ViewModels.Base;

namespace WpfDR.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {

        private string _Status = ".";

        private ParserService _Parser;
        public ObservableCollection<MailItem> MailItems { get; } = new();

        private double _parseProgress;
        public double ParseProgress { get => _parseProgress; set => Set(ref _parseProgress, value); }

        public MainWindowViewModel(ParserService parser)
        {
            _Parser = parser;
        }

        private MailItem _SelectedMail;

        public MailItem SelectedMail { get => _SelectedMail; set => Set(ref _SelectedMail, value); }

        private async void OnLoadFile(object o)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV файл|*.csv";
            Status = "Загрузуа";
            if (openFileDialog.ShowDialog() == true)
            {
                MailItems.Clear();

                using (FileStream fstream = File.OpenRead(openFileDialog.FileName))
                {
                    // преобразуем строку в байты
                    byte[] array = new byte[fstream.Length];
                    // считываем данные
                    fstream.Read(array, 0, array.Length);
                    // декодируем байты в строку файл строго UTF8
                    string textFromFile = System.Text.Encoding.UTF8.GetString(array);

                    var progress = new Progress<double>(p => ParseProgress = p);

                    var res = await _Parser.ParseTextFileAsync(textFromFile, progress);
                    foreach (MailItem item in res)
                        MailItems.Add(item);
                    SelectedMail = MailItems.FirstOrDefault();
                }
            }
            
            ParseProgress = 0;

            Status = $"загружено {MailItems.Count}";
        }

        private ICommand _loadFile;
        private bool CanLoadFile(object o) => true;
        public ICommand LoadFile => _loadFile ??= new LambdaCommand(OnLoadFile, CanLoadFile);

        public string Status { get => _Status; set => Set(ref _Status, value); }

        private ICommand _refreshBrowser;
        private bool CanRefrashBrowser(object o) => SelectedMail.Content is null ? false : true;
        public ICommand RefreshBrowser => _refreshBrowser ??= new LambdaCommand(OnRefreshBrowser, CanRefrashBrowser);

        private void OnRefreshBrowser(object o)
        {

        }

    }
}
