using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WpfDR.Commands;
using WpfDR.Model;
using WpfDR.Service;
using WpfDR.ViewModels.Base;

namespace WpfDR.ViewModels
{
    public class SqlImporterViewModel : ViewModel
    {
        public SqlImporterViewModel(ParserService parser, IRepository<MailItemDb> repository)
        {
            _parser = parser;
            _repository = repository;
        }
        private MailItemDb _BrokeMaill { get; set; }
        public List<MailItemDb> _MailItems { get; } = new();

        private string _SourceFilePath;
        public string SourceFilePath { get => _SourceFilePath; set => Set(ref _SourceFilePath, value); }

        private string _ImportStatus;
        public string ImportStatus { get => _ImportStatus; set => Set(ref _ImportStatus, value); }

        private double _ParseProgress;
        public double ParseProgress { get => _ParseProgress; set => Set(ref _ParseProgress, value); }

        private bool _EnableBtn = true;
        public bool btnEnable { get => _EnableBtn; set => Set(ref _EnableBtn, value); }

        #region comands
        private ICommand _SetSourceFilePath;
        private bool CanSetSourceFilePath(object o) => true;
        public ICommand SetSourceFilePathCommand => _SetSourceFilePath ??= new LambdaCommand(OnSetSourceFilePath, CanSetSourceFilePath);

        private void OnSetSourceFilePath(object obj)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "файлы|*.csv;*.txt";
            if (openFileDialog.ShowDialog() == true) SourceFilePath = openFileDialog.FileName;
        }

        private ICommand _StartImportCommand;
        private readonly ParserService _parser;

        private bool CanStartImport(object o) => true;
        public ICommand StartImportCommand => _StartImportCommand ??= new LambdaCommand(OnStartImport, CanStartImport);

        public IRepository<MailItemDb> _repository { get; }

        #endregion

        private async void OnStartImport(object obj)
        {
            const int readRowCount = 1000000;
            const int fileBuff = 1024 * 1024;
            btnEnable = false;

            var Lines = new string[readRowCount];
            try
            {
                using (StreamReader r = new StreamReader(new BufferedStream(File.OpenRead(_SourceFilePath), fileBuff)))
                {
                    int count = 0;
                    ImportStatus = "Считываю кусок данных из файла";
                    while (r.EndOfStream != true)
                    {

                        Lines[count] = r.ReadLine();
                        count++;
                        if (count == readRowCount)
                        {
                            count = 0;
                            await ProcessFileBlock(Lines).ConfigureAwait(false);
                            Array.Clear(Lines, 0, readRowCount);

                        }

                    }
                    if (count > 0 && count <= readRowCount)
                    {
                        count = 0;
                        await ProcessFileBlock(Lines).ConfigureAwait(false);
                        Array.Clear(Lines, 0, readRowCount);
                    }

                }

                MessageBox.Show($"Преобразование окончено", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                ImportStatus = "";
            }
            catch (Exception e)
            {
                MessageBox.Show($" Ошибка:{e.Message}", "Ошибка преобразования", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnEnable = true;
                SourceFilePath = "";
                ParseProgress = 0;
            }

        }

        private async Task ProcessFileBlock(string[] arrayList)
        {
            ImportStatus = "Обрабатываю кусок данных";

            string text = string.Join("\n", arrayList);

            var progress = new Progress<double>(p => ParseProgress = p);
            try
            {

                var res = await _parser.ParseTextFileSqlAsync(text, progress, brokenMail: _BrokeMaill).ConfigureAwait(false);
                _BrokeMaill = null;

                foreach (MailItemDb item in res.GroupBy(g => new { g.FromAbonent, g.Subject, g.DateCreate, g.Content }).Select(g => g.First()))
                {
                    if (item.Subject != "Уведомление о доставке")
                    {
                        if (item.IsBroken)
                            _BrokeMaill = item;
                        else
                            _MailItems.Add(item);
                    }
                }
                SaveBlock();
            }
            catch (Exception e)
            {

                MessageBox.Show($" Ошибка:{e.Message}", "Ошибка преобразования R", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveBlock()
        {
            ImportStatus = "Сохраняю в базу";
            _repository.AddRange(_MailItems);
            _MailItems.Clear();
        }
    }
}
