using System.Windows;

namespace WpfDR.View
{

    public partial class FileListWindow : Window
    {
        public FileListWindow()
        {
            InitializeComponent();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
