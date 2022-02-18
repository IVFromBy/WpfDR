using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using WpfDR.Commands;
using WpfDR.Model;
using WpfDR.Service;
using WpfDR.ViewModels.Base;
//using System.Windows.Data.Binding;

namespace WpfDR.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {

        private ParserService _Parser;
        private ICollectionView _MailItensListView { get; set; }

        public ObservableCollection<MailItem> MailItems { get; } = new();

        private int totalLoaded = 0;
        private int notifyDelivery = 0;

        #region Notifications

        private string _Status = ".";
        public string Status { get => _Status; set => Set(ref _Status, value); }

        private string _SearchPhrazeSender = default;
        public string SearchPhrazeSender { get => _SearchPhrazeSender; set => Set(ref _SearchPhrazeSender, value); }

        private string _SearchPhrazeSubject = default;
        public string SearchPhrazeSubject { get => _SearchPhrazeSubject; set => Set(ref _SearchPhrazeSubject, value); }

        private string _SearchPhrazeContent = default;
        public string SearchPhrazeContent { get => _SearchPhrazeContent; set => Set(ref _SearchPhrazeContent, value); }

        private bool _ShowProgressBar = false;
        public bool ShowProgressBar { get => _ShowProgressBar; set => Set(ref _ShowProgressBar, value); }

        private double _ParseProgress;
        public double ParseProgress { get => _ParseProgress; set => Set(ref _ParseProgress, value); }

        private MailItem _SelectedMail;
        public MailItem SelectedMail { get => _SelectedMail; set => Set(ref _SelectedMail, value); }

        #endregion

        #region Commands

        private ICommand _loadFileCommand;
        private bool CanLoadFileCommand(object o) => true;
        public ICommand LoadFileCommand => _loadFileCommand ??= new LambdaCommand(OnLoadFile, CanLoadFileCommand);

        private ICommand _searchCommand;
        private bool CanSearchCommand(object o) => MailItems.Count() > 0;
        public ICommand SearchCommand => _searchCommand ??= new LambdaCommand(OnSearching, CanSearchCommand);

        private ICommand _cancelSearchCommand;
        private bool CanCancelSearchCommand(object o) => MailItems.Count() > 0;

        public ICommand CancelSearchCommand => _cancelSearchCommand ??= new LambdaCommand(OnCancelSearching, CanCancelSearchCommand);

        private ICommand _ClearListCommand;
        private bool CanClearListCommand(object o) => MailItems.Count() > 0;

        public ICommand ClearListCommand => _ClearListCommand ??= new LambdaCommand(OnClearList, CanClearListCommand);

        #endregion


        private void OnClearList(object obj)
        {
            totalLoaded = 0;
            notifyDelivery = 0;
            MailItems.Clear();
            Status = ".";
        }

        private void OnCancelSearching(object obj)
        {
            SearchPhrazeContent = default;
            SearchPhrazeSender = default;
            SearchPhrazeSubject = default;
            _MailItensListView.Refresh();
        }

        private void OnSearching(object obj)
        {
            _MailItensListView.Refresh();
        }
        private bool MailItemFilter(object o)
        {
            MailItem mail = o as MailItem;

            return mail.FromAbonent.ToLower().Contains(_SearchPhrazeSender?.ToLower() ?? mail.FromAbonent.ToLower())
                     && mail.Subject.ToLower().Contains(_SearchPhrazeSubject?.ToLower() ?? mail.Subject.ToLower())
                     && mail.Content.ToLower().Contains(_SearchPhrazeContent?.ToLower() ?? mail.Content.ToLower())
                     ;
        }

        public MainWindowViewModel(ParserService parser)
        {
            _Parser = parser;
            _MailItensListView = System.Windows.Data.CollectionViewSource.GetDefaultView(MailItems);
            _MailItensListView.Filter = MailItemFilter;
        }

        private async void OnLoadFile(object o)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "файлы|*.csv;*.txt";
            Status = "Загрузуа";
            ShowProgressBar = true;

            if (openFileDialog.ShowDialog() == true)
            {
                int FileCount = 1;
                foreach (var fileName in openFileDialog.FileNames)
                {
                    if (openFileDialog.FileNames.Count() > 1)
                        Status = $"Загрузка файла {FileCount} из {openFileDialog.FileNames.Count()}";

                    using (FileStream fstream = File.OpenRead(fileName))
                    {
                        // преобразуем строку в байты
                        byte[] array = new byte[fstream.Length];
                        // считываем данные
                        await fstream.ReadAsync(array, 0, array.Length).ConfigureAwait(false);
                        // декодируем байты в строку файл строго UTF8
                        string textFromFile = System.Text.Encoding.UTF8.GetString(array);

                        var progress = new Progress<double>(p => ParseProgress = p);

                        var res = await _Parser.ParseTextFileAsync(textFromFile, progress);

                        totalLoaded += res.Count();

                        foreach (MailItem item in res.GroupBy(g => new { g.FromAbonent, g.Subject, g.DateCreate, g.Content }).Select(g => g.First()))
                        {
                            if (item.Subject == "Уведомление о доставке")
                                notifyDelivery++;
                            else
                            {
                                App.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    MailItems.Add(item);
                                });

                            }
                        }
                        SelectedMail = MailItems.FirstOrDefault();
                    }
                    FileCount++;
                }
            }

            ParseProgress = 0;
            ShowProgressBar = false;

            Status = $"Обработано {totalLoaded}; Удалено уведомлений о доставке {notifyDelivery}; Дубликаты: {totalLoaded - notifyDelivery - MailItems.Count}; Показано {MailItems.Count}";
        }

    }
}
