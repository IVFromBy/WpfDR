using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TestParser
{
    class Program
    {
        static void Main(string[] args)
        {

            parser0();

            Console.ReadLine();
        }

        private static void parser0()
        {
            using (FileStream fstream = File.OpenRead(@"D:\_lab\t3.csv"))
            {
                // преобразуем строку в байты
                byte[] array = new byte[fstream.Length];
                // считываем данные
                fstream.Read(array, 0, array.Length);
                // декодируем байты в строку
                string textFromFile = System.Text.Encoding.UTF8.GetString(array);

                Regex regex = new Regex(@"(([\d\w]{4,})+\t\d\t(\d{2}.\d{2}.\d{4}))", RegexOptions.Singleline);
                var matches = regex.Matches(textFromFile);

                int counter = 0;
                foreach (Match m in matches)
                {
                    Console.WriteLine("Часть " + counter + " :");
                    var nextV = m.NextMatch();
                    Console.WriteLine("{0}| размер {1}| позиция {2}| charSize {3} ",m.Value,m.Length ,m.Index, nextV.Index-m.Index);
                    Console.WriteLine("Кусок для копирования:");
                    if (nextV.Success)
                    {                        
                        Console.WriteLine(textFromFile.Substring(m.Index,nextV.Index-m.Index));
                    }
                    else
                    {
                        Console.WriteLine(textFromFile.Substring(m.Index,textFromFile.Length-m.Index));
                    }

                    counter++;
                }

            }

        }


    }
}