using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using WpfDR.Commands;
using WpfDR.ViewModels.Base;

namespace WpfDR.ViewModels
{
    public class FileListWindowViewModel : ViewModel
    {
        public ObservableCollection<string> FileList { get; set; } = new();
        
        private string _SelectedFile;
        public string SelectedFile { get => _SelectedFile; set => Set(ref _SelectedFile, value); }

        private int _SelectedFileIndex;
        public int SelectedFileIndex { get => _SelectedFileIndex; set => Set(ref _SelectedFileIndex, value); }

        private bool _ModalResult = false;
        public bool ModalResult { get => _ModalResult; set => Set(ref _ModalResult, value); }
        

        #region Commands

        private ICommand _MoveFileUpCommand;
        private bool CanMoveFileUpCommand(object o) => true;
        public ICommand MoveFileUpCommand => _MoveFileUpCommand ??= new LambdaCommand(OnMoveFileUp, CanMoveFileUpCommand);

        private ICommand _MoveFileDownCommand;
        private bool CanMoveFileDownCommand(object o) => true;
        public ICommand MoveFileDownCommand => _MoveFileDownCommand ??= new LambdaCommand(OnMoveFileDown, CanMoveFileDownCommand);
        
        private ICommand _ModalResultOkCommand;
        private bool CanModalResultOkCommand(object o) => true;
        public ICommand ModalResultOkCommand => _ModalResultOkCommand ??= new LambdaCommand(OnModalResultOk, CanModalResultOkCommand);

        #endregion

        private void OnModalResultOk(object obj)
        {
            ModalResult = true;
        }

        private void OnMoveFileUp(object obj)
        {
            var selectedIndex = _SelectedFileIndex;

            if (selectedIndex > 0)
            {
                var itemToMoveUp = this.FileList[selectedIndex];
                this.FileList.RemoveAt(selectedIndex);
                this.FileList.Insert(selectedIndex - 1, itemToMoveUp);
                SelectedFileIndex = selectedIndex - 1;
            }
        }

        private void OnMoveFileDown(object obj)
        {
            var selectedIndex = _SelectedFileIndex;

            if (selectedIndex +1 <FileList.Count)
            {
                var itemToMoveDown = FileList[selectedIndex];
                FileList.RemoveAt(selectedIndex);
                FileList.Insert(selectedIndex + 1, itemToMoveDown);
                SelectedFileIndex = selectedIndex + 1;
            }
        }

    }
}
