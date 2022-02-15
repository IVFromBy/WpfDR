using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using WpfDR.Model;

namespace WpfDR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private async void miLoad_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV файл|*.csv";

            if (openFileDialog.ShowDialog() == true)
            {
                //string[] lines = await File.ReadAllLinesAsync(openFileDialog.FileName);
                ListViewPeople.ItemsSource = await ReadCSV2(openFileDialog.FileName);

            }
        }

        private async Task<IEnumerable<MailItem>> ReadCSV2(string fileName)
        {
            using (TextFieldParser parser = new TextFieldParser(fileName))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters("\t");

                var testHeader = parser.ReadLine().Split(',').ToArray();
                try
                {
                    var ResultList = new List<MailItem>();

                    if (testHeader.Length != 21)
                        throw new ArgumentOutOfRangeException($"Количество ячеек={testHeader.Length}", "Заголовок csv файла не совподает с шаблоном!");


                    while (!parser.EndOfData)
                    {
                        //Process row
                        string[] data = parser.ReadFields();

                        ResultList.Add(new MailItem(
                            mID: data[0],
                            idFolder: Convert.ToInt32(data[1]),
                            dateCreate: Convert.ToDateTime(data[2]),
                            subject: data[3],
                            fromAbonent: data[4],
                            replyTo: data[5],
                            toAbonent: data[6],
                            dateRecive: data[7] == "?" ? null : Convert.ToDateTime(data[7]),
                            dateRead: data[8] == "?" ? null : Convert.ToDateTime(data[8]),
                            pA: Convert.ToInt32(data[9]),
                            receipt: Convert.ToInt32(data[10]),
                            dateReceipt: data[11] == "?" ? null : Convert.ToDateTime(data[11]),
                            idReceipt: data[12] == "?" ? null : Convert.ToInt32(data[12]),
                            typeMessage: Convert.ToInt32(data[13]),
                            dateSend: data[14] == "?" ? null : Convert.ToDateTime(data[14]),
                            idAbonent: Convert.ToInt32(data[15]),
                            priority: Convert.ToInt32(data[16]),
                            isRead: Convert.ToInt32(data[17]),
                            content: data[18],
                            num: Convert.ToInt32(data[19]),
                            msgCategory: Convert.ToInt32(data[20])
                            ));
                    }
                    return ResultList;
                }
                catch (Exception e)
                {

                    MessageBox.Show("Ошибка: " + e.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return new List<MailItem>();
                }
                finally
                {
                    parser.Close();
                }



            }

            /*
            string[] lines = await File.ReadAllLinesAsync(Path.ChangeExtension(fileName, ".csv"));

            var testHeader = lines.Take(1).Select(l => { string[] data = l.Split(','); return data; }).ToArray();

            if (testHeader[0].Length != 21)
                throw new ArgumentOutOfRangeException("Заголовок csv файла не совподает с шаблоном!", "Заголовок csv файла не совподает с шаблоном!");

            return lines.Skip(1).Select(line =>
            {
                string[] data = line.Split('\t');

                return new MailItem(
                    mID: data[0],
                    idFolder: Convert.ToInt32(data[1]),
                    dateCreate: Convert.ToDateTime(data[2]),
                    subject: data[3],
                    fromAbonent: data[4],
                    replyTo: data[5],
                    toAbonent: data[6],
                    dateRecive: data[7] == "?" ? null : Convert.ToDateTime(data[7]),
                    dateRead: data[8] == "?" ? null : Convert.ToDateTime(data[8]),
                    pA: Convert.ToInt32(data[9]),
                    receipt: Convert.ToInt32(data[10]),
                    dateReceipt: data[11] == "?" ? null : Convert.ToDateTime(data[11]),
                    idReceipt: data[12] == "?" ? null : Convert.ToInt32(data[12]),
                    typeMessage: Convert.ToInt32(data[13]),
                    dateSend: data[14] == "?" ? null : Convert.ToDateTime(data[14]),
                    idAbonent: Convert.ToInt32(data[15]),
                    priority: Convert.ToInt32(data[16]),
                    isRead: Convert.ToInt32(data[17]),
                    content: data[18],
                    num: Convert.ToInt32(data[19]),
                    msgCategory: Convert.ToInt32(data[20])
                    );
            });*/
        }

        private async Task<IEnumerable<MailItem>> ReadCSV(string fileName)
        {
            string[] lines = await File.ReadAllLinesAsync(Path.ChangeExtension(fileName, ".csv"));

            var testHeader = lines.Take(1).Select(l => { string[] data = l.Split(','); return data; }).ToArray();

            if (testHeader[0].Length != 21)
                throw new ArgumentOutOfRangeException("Заголовок csv файла не совподает с шаблоном!", "Заголовок csv файла не совподает с шаблоном!");

            return lines.Skip(1).Select(line =>
            {
                string[] data = line.Split('\t');

                return new MailItem(
                    mID: data[0],
                    idFolder: Convert.ToInt32(data[1]),
                    dateCreate: Convert.ToDateTime(data[2]),
                    subject: data[3],
                    fromAbonent: data[4],
                    replyTo: data[5],
                    toAbonent: data[6],
                    dateRecive: data[7] == "?" ? null : Convert.ToDateTime(data[7]),
                    dateRead: data[8] == "?" ? null : Convert.ToDateTime(data[8]),
                    pA: Convert.ToInt32(data[9]),
                    receipt: Convert.ToInt32(data[10]),
                    dateReceipt: data[11] == "?" ? null : Convert.ToDateTime(data[11]),
                    idReceipt: data[12] == "?" ? null : Convert.ToInt32(data[12]),
                    typeMessage: Convert.ToInt32(data[13]),
                    dateSend: data[14] == "?" ? null : Convert.ToDateTime(data[14]),
                    idAbonent: Convert.ToInt32(data[15]),
                    priority: Convert.ToInt32(data[16]),
                    isRead: Convert.ToInt32(data[17]),
                    content: data[18],
                    num: Convert.ToInt32(data[19]),
                    msgCategory: Convert.ToInt32(data[20])
                    );
            });
        }

    }
}
