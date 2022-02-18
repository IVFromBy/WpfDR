using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfDR.Model;

namespace WpfDR.Service
{
    public class ParserService
    {  //Шаблон регулярки, которая находин уникальную свяку трёх первых полей записи
        private string regExpPattern = @"(([\d\w]{4,})+\t\d\t(\d{2}.\d{2}.\d{4}))";

        public async Task<List<MailItem>> ParseTextFileAsync(string textFile, IProgress<double> Progress = null,
            CancellationToken Cancel = default, MailItem brokenMail = null)
        {
            Regex regex = new Regex(regExpPattern, RegexOptions.Singleline);
            var matches = regex.Matches(textFile);
            int totalMatches = matches.Count;
            bool IsNeedFixBrokenMail = brokenMail is not null;
            var resList = new List<MailItem>();
            int i = 0;

            foreach (Match m in matches)
            { // находим следующее совпадение
                Match nextV = m.NextMatch();

                if (IsNeedFixBrokenMail)
                {
                    resList.Add(await Task.Run(() => ParseBrokenBody(textFile.Substring(0,m.Index),brokenMail)).ConfigureAwait(false));
                    IsNeedFixBrokenMail = false;
                }

                Progress?.Report((double)i / totalMatches);
                // передаём на детальный разбор парсером одного сообщения
                resList.Add(await Task.Run(() => ParseBody(textFile.Substring(m.Index, nextV.Success ? nextV.Index - m.Index : textFile.Length - m.Index))).ConfigureAwait(false));
                i++;
                Cancel.ThrowIfCancellationRequested();
            }
            Progress?.Report(1);
            return resList;
        }

        private MailItem ParseBody(string inStr)
        {
            // ищем начало поля "content"
            int contentPos = inStr.IndexOf('<');
            // отделяем начало записи от тела контента
            string[] data = inStr.Substring(0, contentPos).Split('\t');
            int bodyLength = inStr.Length;
            int num = 0;
            int msgCategory = 0;
            string contentBody;
            bool isEndOfFile = false;
            // получаем кусочек окончания тела
            var endofTheBody = inStr.Substring(bodyLength - 8, 8);
            // Добавление кодировки только для UTF-8 в тело письма.
            string utf8Cod = @"<meta http-equiv='Content-Type' content='text/html;charset=UTF-8'>";

            try
            {
                if (!endofTheBody.Contains("\t"))
                { // если в окончании отсутствует табуляция, то это "обрывок" файла
                    isEndOfFile = true;
                    contentBody = inStr[contentPos..bodyLength];
                }
                else
                { // парсим окончание тела, получая значение последних полей
                    string[] endValues = endofTheBody.Split('\t');
                    num = Convert.ToInt32(endValues[1]);
                    msgCategory = Convert.ToInt32(endValues[2]);
                    // формируем полное тело "контента"
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
                      content: String.Concat(utf8Cod, contentBody),
                      num: num,
                      msgCategory: msgCategory,
                      isEndOfFile: isEndOfFile
                      );
            }
            catch (Exception e)
            {
                MessageBox.Show($"Произошла на этапе парсинга тела! \n Ошибка:{e.Message}"
                    , "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new MailItem();
            }


        }

        private MailItem ParseBrokenBody(string inStr, MailItem brokenMail)
        {
            int bodyLength = inStr.Length;

            var endofTheBody = inStr.Substring(bodyLength - 8, 8);

            string[] endValues = endofTheBody.Split('\t');

            MailItem fixedMail = new MailItem
            {
                MID = brokenMail.MID,
                IdFolder = brokenMail.IdFolder,
                DateCreate = brokenMail.DateCreate,
                Subject = brokenMail.Subject,
                FromAbonent = brokenMail.FromAbonent,
                ReplyTo = brokenMail.ReplyTo,
                ToAbonent = brokenMail.ToAbonent,
                DateRecive = brokenMail.DateRecive,
                DateRead = brokenMail.DateRead,
                PA = brokenMail.PA,
                Receipt = brokenMail.Receipt,
                DateReceipt = brokenMail.DateReceipt,
                IdReceipt = brokenMail.IdReceipt,
                TypeMessage = brokenMail.TypeMessage,
                DateSend = brokenMail.DateSend,
                IdAbonent = brokenMail.IdAbonent,
                Priority = brokenMail.Priority,
                IsRead = brokenMail.IsRead,
                Content = string.Concat(brokenMail.Content, inStr.Substring(0, bodyLength - (endofTheBody.Length - endofTheBody.IndexOf('\t')))),
                Num = Convert.ToInt32(endValues[1]),
                MsgCategory = Convert.ToInt32(endValues[2]),
                IsEndOfFile = false,
            };
            return fixedMail;
        }


    }
}
