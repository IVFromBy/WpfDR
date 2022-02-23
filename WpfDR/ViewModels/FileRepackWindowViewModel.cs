using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using WpfDR.Commands;
using WpfDR.ViewModels.Base;

namespace WpfDR.ViewModels
{
    class FileRepackWindowViewModel : ViewModel
    {
        private string _SourceFilePath;
        public string SourceFilePath { get => _SourceFilePath; set => Set(ref _SourceFilePath, value); }

        private string _ResultFilePath;
        public string ResultFilePath { get => _ResultFilePath; set => Set(ref _ResultFilePath, value); }

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
        #endregion


    }
}
