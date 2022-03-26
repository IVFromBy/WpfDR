using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Win32;
using WpfDR.Commands;
using WpfDR.Model;
using WpfDR.Service;
using WpfDR.ViewModels.Base;

namespace WpfDR.ViewModels
{
    class FileRepackWindowViewModel : ViewModel
    {
        public List<MailItem> _MailItems { get; } = new();
        private Nullable<MailItem> _BrokeMaill { get; set; }

        private string _SourceFilePath;
        public string SourceFilePath { get => _SourceFilePath; set => Set(ref _SourceFilePath, value); }

        private string _ResultFilePath;
        public string ResultFilePath { get => _ResultFilePath; set => Set(ref _ResultFilePath, value); }

        private string _ReadRowCount = "1000000";
        public string ReadRowCount { get => _ReadRowCount; set => Set(ref _ReadRowCount, value); }

        private double _ParseProgress;
        public double ParseProgress { get => _ParseProgress; set => Set(ref _ParseProgress, value); }

        private bool _TotalProgress = false;
        public bool TotalProgress { get => _TotalProgress; set => Set(ref _TotalProgress, value); }

        private bool _EnableBtn = true;
        public bool btnEnable { get => _EnableBtn; set => Set(ref _EnableBtn, value); }

        #region comands
        private ICommand _SetSourceFilePath;
        private bool CanSetSourceFilePath(object o) => true;
        public ICommand SetSourceFilePath => _SetSourceFilePath ??= new LambdaCommand(OnSetSourceFilePath, CanSetSourceFilePath);

        private void OnSetSourceFilePath(object obj)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "файлы|*.csv;*.txt";
            if (openFileDialog.ShowDialog() == true) SourceFilePath = openFileDialog.FileName;
        }

        private ICommand _SetResultFilePath;
        private bool CanSetResultFilePath(object o) => true;
        public ICommand SetResulFilePath => _SetResultFilePath ??= new LambdaCommand(OnSetResultFilePath, CanSetResultFilePath);

        private void OnSetResultFilePath(object obj)
        {
            var openFileDialog = new SaveFileDialog();
            openFileDialog.Filter = "файлы|*.csv;*.txt";
            if (openFileDialog.ShowDialog() == true)
                ResultFilePath = openFileDialog.FileName;
        }

        private ICommand _StartRepack;

        private bool CanStartRepack(object o) => _ResultFilePath != default && _SourceFilePath != default;
        public ICommand StartRepack => _StartRepack ??= new LambdaCommand(OnStartRepack, CanStartRepack);

        #endregion

        private readonly ParserService _Parser;

        public FileRepackWindowViewModel(ParserService parser)
        {
            _Parser = parser;
        }

        private async void OnStartRepack(object obj)
        {
            int readRowCount;
            int.TryParse(_ReadRowCount, out readRowCount);
            btnEnable = false;

            var Lines = new string[readRowCount];
            try
            {
                TotalProgress = true;
                using (StreamReader r = new StreamReader(new BufferedStream(File.OpenRead(_SourceFilePath), 1024 * 1024)))
                {
                    int count = 0;


                    while (r.EndOfStream != true)
                    {
                        Lines[count] = r.ReadLine();
                        count++;
                        if (count == readRowCount)
                        {
                            count = 0;
                            await RepackBlock(Lines).ConfigureAwait(false);
                            Array.Clear(Lines, 0, readRowCount);

                        }

                    }
                    if (count > 0 && count <= readRowCount)
                    {
                        count = 0;
                        await RepackBlock(Lines).ConfigureAwait(false);
                        Array.Clear(Lines, 0, readRowCount);
                    }

                }

                TotalProgress = false;
                MessageBox.Show($"Преобразование окончено", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show($" Ошибка:{e.Message}", "Ошибка преобразования", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnEnable = true;
                _MailItems.Clear();
                _BrokeMaill = null;
                SourceFilePath = "";
                TotalProgress = false;
                ParseProgress = 0;
            }
        }

        private async Task RepackBlock(string[] lines)
        {
            string text = string.Join("\n", lines);

            var progress = new Progress<double>(p => ParseProgress = p);
            try
            {

                var res = await _Parser.ParseTextFileAsync(text, progress, brokenMail: _BrokeMaill).ConfigureAwait(false);
                _BrokeMaill = null;

                foreach (MailItem item in res.GroupBy(g => new { g.FromAbonent, g.Subject, g.DateCreate, g.Content }).Select(g => g.First()))
                {
                    if (item.Subject != "Уведомление о доставке")
                    {
                        if (item.IsEndOfFile)
                            _BrokeMaill = item;
                        else
                            _MailItems.Add(item);
                    }
                }
                SaveRepackBlock();
            }
            catch (Exception e)
            {

                MessageBox.Show($" Ошибка:{e.Message}", "Ошибка преобразования R", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SaveRepackBlock()
        {
            using (var stream = File.Open(_ResultFilePath, FileMode.Append))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(new CultureInfo("ru-RU"))
            {
                HasHeaderRecord = false,
                Delimiter = "\t",
            }))
            {
                csv.WriteRecords(_MailItems);
                _MailItems.Clear();
            }
        }
    }
}
