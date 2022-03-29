using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WpfDR.Commands;
using WpfDR.Model;
using WpfDR.Service;
using WpfDR.View;
using WpfDR.ViewModels.Base;

namespace WpfDR.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {

        private ParserService _Parser;
        private FileListWindowViewModel _FlWin;
        private IRepository<MailItemDb> _repository;
        private readonly SqlImporterViewModel _sqlImport;

        private ICollectionView _MailItemsListView { get; set; }
        private ICollectionView _SqlMailItemsListView { get; set; }

        public ObservableCollection<MailItem> MailItems { get; } = new();
        private Nullable<MailItem> _BrokeMaill { get; set; }
        public ObservableCollection<MailItemDb> SqlMailItems { get; } = new();

        private int totalLoaded = 0;
        private int notifyDelivery = 0;

        #region Notifications

        private string _Status = ".";
        public string Status { get => _Status; set => Set(ref _Status, value); }

        private string _SearchPhrazeSender = "";
        public string SearchPhrazeSender { get => _SearchPhrazeSender; set => Set(ref _SearchPhrazeSender, value); }

        private string _SearchPhrazeSubject = "";
        public string SearchPhrazeSubject { get => _SearchPhrazeSubject; set => Set(ref _SearchPhrazeSubject, value); }

        private string _SearchPhrazeContent = "";
        public string SearchPhrazeContent { get => _SearchPhrazeContent; set => Set(ref _SearchPhrazeContent, value); }

        private bool _ShowProgressBar = false;
        public bool ShowProgressBar { get => _ShowProgressBar; set => Set(ref _ShowProgressBar, value); }

        private double _ParseProgress;
        public double ParseProgress { get => _ParseProgress; set => Set(ref _ParseProgress, value); }

        private MailItem? _SelectedMail;
        public MailItem? SelectedMail { get => _SelectedMail; set => Set(ref _SelectedMail, value); }

        private MailItemDb _SelectedSqlMail;
        public MailItemDb SelectedSqlMail { get => _SelectedSqlMail; set => Set(ref _SelectedSqlMail, value); }

        private bool _ReadingFile = false;
        public bool ReadingFile { get => _ReadingFile; set => Set(ref _ReadingFile, value); }

        private byte _SelectedTabIndex = 0;
        public byte SelectedTabIndex { get => _SelectedTabIndex; set => Set(ref _SelectedTabIndex, value); }
        #endregion

        #region Commands

        private ICommand _loadFileCommand;
        private bool CanLoadFileCommand(object o) => true;
        public ICommand LoadFileCommand => _loadFileCommand ??= new LambdaCommand(OnLoadFile, CanLoadFileCommand);

        private ICommand _searchCommand;
        private bool CanSearchCommand(object o) => (MailItems.Count() > 0 && _SelectedTabIndex == 0) || (SqlMailItems.Count() > 0 && _SelectedTabIndex == 1);
        public ICommand SearchCommand => _searchCommand ??= new LambdaCommand(OnSearching, CanSearchCommand);

        private ICommand _cancelSearchCommand;
        private bool CanCancelSearchCommand(object o) => (MailItems.Count() > 0 && _SelectedTabIndex == 0) || (SqlMailItems.Count() > 0 && _SelectedTabIndex == 1);

        public ICommand CancelSearchCommand => _cancelSearchCommand ??= new LambdaCommand(OnCancelSearching, CanCancelSearchCommand);

        private ICommand _ClearListCommand;
        private bool CanClearListCommand(object o) => false;// MailItems.Count() > 0;

        public ICommand ClearListCommand => _ClearListCommand ??= new LambdaCommand(OnClearList, CanClearListCommand);


        private ICommand _ClearSqlListCommand;
        private bool CanClearSqlListCommand(object o) => false;

        public ICommand ClearSqlListCommand => _ClearSqlListCommand ??= new LambdaCommand(OnClearSqlList, CanClearSqlListCommand);

        private void OnClearSqlList(object obj)
        {
            SqlMailItems.Clear();
            SelectedSqlMail = null;
            Status = ".";
        }

        private ICommand _ShowFilerListCommand;
        private bool CanShowFilerListCommand(object o) => true;

        public ICommand RepackFile => _ShowFilerListCommand ??= new LambdaCommand(OnShowRepackFile, CanShowFilerListCommand);

        private ICommand _ShowImportSqlCommand;
        public ICommand ImportInToSqlCommand => _ShowImportSqlCommand ??= new LambdaCommand(OnShowSqlImport, CanShowFilerListCommand);

        private ICommand _loadSqlCommand;
        private bool CanLoadSqlCommand(object o) => true;
        public ICommand LoadSqlCommand => _loadSqlCommand ??= new LambdaCommand(OnLoadSql, CanLoadSqlCommand);
        
        private ICommand _SqlDeleteAllCommand;
        private bool CanSqlDeleteAllCommand(object o) => true;
        public ICommand SqlDeleteAllCommand => _SqlDeleteAllCommand ??= new LambdaCommand(OnDeleteAllSql, CanSqlDeleteAllCommand);
        
        private ICommand _SqlAddTestRowCommand;
        private bool CanSqlAddTestRowCommand(object o) => true;
        public ICommand SqlAddTestRowCommand => _SqlAddTestRowCommand ??= new LambdaCommand(OnAddTestRowSql, CanSqlAddTestRowCommand);

        private void OnAddTestRowSql(object obj)
        {
            var testRow = new MailItemDb { Mid = "test",
                Content="<html> <d3>test row</d3></html>",
                FromAbonent="test",
                Subject="test",
                IsBroken=false,
                ToAbonent="to test",
                ReplayTo="test",
                DateCreate="01.01.1991"
            };

            _repository.Add(testRow);
            Status = "Добавлена тестовая запись";
        }

        private void OnDeleteAllSql(object obj)
        {
            Status = "Начал очищать базу";
            _repository.DeleteAll();
            SqlMailItems.Clear();
            Status = "База очищена";
        }

        private void OnShowRepackFile(object obj)
        {
            FileRepackWindow frWindow = new FileRepackWindow();
            frWindow.ShowDialog();
        }

        private void OnShowFilerList(object obj)
        {
            FileListWindow flWindow = new FileListWindow();
            flWindow.ShowDialog();
        }
        private void OnShowSqlImport(object obj)
        {
            try
            {
                SqlImporterWindow flWindow = new SqlImporterWindow();
                flWindow.ShowDialog();
            }
            catch (Exception e)
            {
                MessageBox.Show($" Ошибка:{e.Message}"
                            , "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        #endregion


        private void OnClearList(object obj)
        {            
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                totalLoaded = 0;
                notifyDelivery = 0;
                _MailItemsListView = null;
                MailItems.Clear();
                SelectedMail = null;
                Status = ".";
                _BrokeMaill = null;
            });

        }

        private void OnCancelSearching(object obj)
        {
            SearchPhrazeContent = "";
            SearchPhrazeSender = "";
            SearchPhrazeSubject = "";
            if (SelectedTabIndex == 0)
                _MailItemsListView.Refresh();
            else
                _SqlMailItemsListView.Refresh();


        }

        private void OnSearching(object obj)
        {
            if (_SelectedTabIndex == 0)
            {
                _MailItemsListView.Refresh();
            }
            else
            {
                _SqlMailItemsListView.Refresh();
            }
        }
        private bool MailItemFilter(object o)
        {
            if (_SearchPhrazeSender.Length != 0 || _SearchPhrazeSubject.Length != 0 || _SearchPhrazeContent.Length != 0)
            {
                if (_SelectedTabIndex == 0)
                {
                    Nullable<MailItem> mail = o as Nullable<MailItem>;

                    return mail.Value.FromAbonent.ToLower().Contains(_SearchPhrazeSender?.ToLower() ?? mail.Value.FromAbonent.ToLower())
                             && mail.Value.Subject.ToLower().Contains(_SearchPhrazeSubject?.ToLower() ?? mail.Value.Subject.ToLower())
                             && mail.Value.Content.ToLower().Contains(_SearchPhrazeContent?.ToLower() ?? mail.Value.Content.ToLower())
                             ;
                }
                else
                {
                    MailItemDb mail = o as MailItemDb;

                    return mail.FromAbonent.ToLower().Contains(_SearchPhrazeSender.ToLower() ?? mail.FromAbonent.ToLower())
                             && mail.Subject.ToLower().Contains(_SearchPhrazeSubject.ToLower() ?? mail.Subject.ToLower())
                             && mail.Content.ToLower().Contains(_SearchPhrazeContent.ToLower() ?? mail.Content.ToLower())
                             ;
                }
            }
            else
                return true;
        }

        public MainWindowViewModel(ParserService parser
            , FileListWindowViewModel flWin
            , IRepository<MailItemDb> repository
            , SqlImporterViewModel sqlImport
            )
        {
            _Parser = parser;
            _FlWin = flWin;
            _repository = repository;
            _sqlImport = sqlImport;
            _MailItemsListView = System.Windows.Data.CollectionViewSource.GetDefaultView(MailItems);
            _MailItemsListView.Filter = MailItemFilter;
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
                _FlWin.FileList.Clear();
                _FlWin.ModalResult = false;

                foreach (var fileName in openFileDialog.FileNames)
                    _FlWin.FileList.Add(fileName);

                if (openFileDialog.FileNames.Count() > 1)
                {
                    OnShowFilerList(o);

                    if (!_FlWin.ModalResult)
                        return;
                }

                foreach (var fileName in _FlWin.FileList)
                {
                    if (_FlWin.FileList.Count() > 1)
                        Status = $"Загрузка файла {FileCount} из {_FlWin.FileList.Count()}";

                    try
                    {
                        using (StreamReader fstream = new StreamReader(new BufferedStream(File.OpenRead(fileName), (1024 * 1024) * 7)))
                        {

                            var progress = new Progress<double>(p => ParseProgress = p);
                            Status = $"Читаю файл - {fileName}";
                            ReadingFile = true;
                            IProgress<double> prog = progress;
                            string textFromFile = await Task.Run(() => fstream.ReadToEndAsync()).ConfigureAwait(false);
                            fstream.Close();
                            ReadingFile = false;
                            Status = "Обрабатываю";
                            var res = await _Parser.ParseTextFileAsync(textFromFile, progress, brokenMail: _BrokeMaill);

                            _BrokeMaill = null;

                            totalLoaded += res.Count();

                            var GrouptedRes = res.GroupBy(g => new { g.FromAbonent, g.Subject, g.DateCreate, g.Content }).Select(g => g.First());

                            int grpRes = GrouptedRes.Count();
                            int i = 0;
                            Status = "Формирую список для отображения";

                            foreach (MailItem item in GrouptedRes)
                            {
                                if (item.Subject == "Уведомление о доставке")
                                    notifyDelivery++;
                                else
                                {
                                    App.Current.Dispatcher.Invoke((Action)delegate
                                    {
                                        if (item.IsEndOfFile)
                                        {
                                            _BrokeMaill = item;
                                            totalLoaded--;
                                        }
                                        else
                                            MailItems.Add(item);
                                    });

                                }
                                prog.Report((double)i / grpRes);

                                i++;
                            }
                            SelectedMail = MailItems.FirstOrDefault();

                        }
                        FileCount++;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($" Ошибка:{e.Message}"
                            , "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            OnClearList(o);
                        });
                    }
                }

            }

            ParseProgress = 0;
            ShowProgressBar = false;

            Status = $"Обработано {totalLoaded}; Удалено уведомлений о доставке {notifyDelivery}; Дубликаты: {totalLoaded - notifyDelivery - MailItems.Count}; Показано {MailItems.Count}";
        }

        private void OnLoadSql(object obj)
        {            
            //_SqlMailItemsListView = System.Windows.Data.CollectionViewSource.GetDefaultView(null);
            SqlMailItems.Clear();

            foreach (var mail in _repository.GetAll())
                SqlMailItems.Add(mail);

            SelectedSqlMail = SqlMailItems.FirstOrDefault();
            _SqlMailItemsListView = System.Windows.Data.CollectionViewSource.GetDefaultView(SqlMailItems);
            _SqlMailItemsListView.Filter = MailItemFilter;
            Status = $"Прочитано {SqlMailItems.Count()} писем из базы";
        }

    }
}
