using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using WpfDR.Commands;
using WpfDR.Service;
using WpfDR.ViewModels.Base;

namespace WpfDR.ViewModels
{
    public class SqlImporterViewModel : ViewModel
    {
        private string _SourceFilePath;
        public string SourceFilePath { get => _SourceFilePath; set => Set(ref _SourceFilePath, value); }


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

        private bool CanStartImport(object o) => _SourceFilePath != default;
        public ICommand StartImportCommand => _StartImportCommand ??= new LambdaCommand(OnStartImport, CanStartImport);

        private void OnStartImport(object obj)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
