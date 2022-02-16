using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;
using WpfDR.Model;

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
                var resList = new List<MailItem>();

                foreach (Match m in matches)
                {
                    // Console.WriteLine("Часть " + counter + " :");
                    var nextV = m.NextMatch();
                    //Console.WriteLine("{0}| размер {1}| позиция {2}| charSize {3} ", m.Value, m.Length, m.Index, nextV.Index - m.Index);
                    //Console.WriteLine("Кусок для копирования:");

                    //Console.WriteLine(textFromFile.Substring(m.Index, nextV.Success ? nextV.Index - m.Index : textFromFile.Length - m.Index));
                    resList.Add(ParseBody(textFromFile.Substring(m.Index, nextV.Success ? nextV.Index - m.Index : textFromFile.Length - m.Index)));

                    counter++;
                }

            }

        }

        private static MailItem ParseBody(string inStr)
        {

            int contentPos = inStr.IndexOf('<');

            var data = inStr.Substring(0, contentPos).Split('\t');
            int bodyLength = inStr.Length;
            int num = 0;
            int msgCategory = 0;
            string contentBody;
            bool isEndofFile = false;

            var endofTheBody = inStr.Substring(bodyLength - 8, 8);

            if (!endofTheBody.Contains("\t"))
            {
                isEndofFile = true;
                contentBody = inStr[contentPos..bodyLength];
            }
            else
            {
                string[] endValues = endofTheBody.Split('\t');

                num = Convert.ToInt32(endValues[1]);
                msgCategory = Convert.ToInt32(endValues[2]);

                contentBody = inStr.Substring(contentPos, bodyLength - contentPos - (endofTheBody.Length - endofTheBody.IndexOf('\t')));
            }

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
                                        content: contentBody,
                                        num: num,
                                        msgCategory: msgCategory,
                                        isEndOfFile:isEndofFile
                                        );

        }


    }
}