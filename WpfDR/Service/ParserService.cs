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
        private string regExpPattern = @"(([\d\w]{4,})+\t\d{1,6}\t(\d{2}.\d{2}.\d{4}))";
        private string utf8Cod = @"<meta http-equiv='Content-Type' content='text/html;charset=UTF-8'>";
        private int tailLength = 12;

        public async Task<List<MailItem>> ParseTextFileAsync(string textFile, IProgress<double> Progress = null,
            CancellationToken Cancel = default, Nullable<MailItem> brokenMail = null)
        {
            Regex regex = new Regex(regExpPattern, RegexOptions.Singleline);
            var matches = regex.Matches(textFile);
            int totalMatches = matches.Count;
            bool IsNeedFixBrokenMail = brokenMail is not null;
            var resList = new List<MailItem>();
            int i = 0;
            try
            {


                foreach (Match m in matches)
                { // находим следующее совпадение
                    Match nextV = m.NextMatch();

                    if (IsNeedFixBrokenMail)
                    {
                        resList.Add(await Task.Run(() => ParseBrokenBody(textFile.Substring(0, m.Index), brokenMail)).ConfigureAwait(false));
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
            catch (Exception e)
            {

                MessageBox.Show($"Произошла на этапе парсинга тела! \n Ошибка:{e.Message}"
                    , "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return resList;
            }
        }

        private MailItem ParseBody(string inStr)
        {
            // ищем начало поля "content"
            int contentPos = inStr.IndexOf("<span") == -1 ? inStr.IndexOf("<html") : inStr.IndexOf("<span");

            // отделяем начало записи от тела контента
            string[] data = inStr.Substring(0, contentPos).Split('\t');
            int bodyLength = inStr.Length;
            int num = 0;
            int msgCategory = 0;
            string contentBody;
            bool isEndOfFile = false;
            // получаем кусочек окончания тела
            var endofTheBody = inStr.Substring(bodyLength - tailLength, tailLength);
            // Добавление кодировки только для UTF-8 в тело письма.            

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

                return new MailItem
                {
                    MID = data[0],
                    IdFolder = Convert.ToInt32(data[1]),
                    DateCreate = DateTime.ParseExact(data[2], "dd.MM.yyyy", null),
                    Subject = data[3],
                    FromAbonent = data[4] ?? "",
                    ReplyTo = data[5],
                    ToAbonent = data[6],
                    DateRecive = data[7] == "?" ? null : DateTime.ParseExact(data[7], "dd.MM.yyyy", null),
                    DateRead = data[8] == "?" ? null : DateTime.ParseExact(data[8], "dd.MM.yyyy", null),
                    PA = Convert.ToInt32(data[9]),
                    Receipt = Convert.ToInt32(data[10]),
                    DateReceipt = data[11] == "?" ? null : DateTime.ParseExact(data[11], "dd.MM.yyyy", null),
                    IdReceipt = data[12] == "?" ? null : Convert.ToInt32(data[12]),
                    TypeMessage = Convert.ToInt32(data[13]),
                    DateSend = data[14] == "?" ? null : DateTime.ParseExact(data[14], "dd.MM.yyyy", null),
                    IdAbonent = Convert.ToInt32(data[15]),
                    Priority = Convert.ToInt32(data[16]),
                    IsRead = Convert.ToInt32(data[17]),
                    Content = String.Concat(utf8Cod, contentBody),
                    Num = num,
                    MsgCategory = msgCategory,
                    IsEndOfFile = isEndOfFile
                };
            }
            catch (Exception e)
            {
                MessageBox.Show($"Произошла на этапе парсинга тела! \n Ошибка:{e.Message}"
                    , "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new MailItem();
            }


        }

        private MailItem ParseBrokenBody(string inStr, Nullable<MailItem> brokenMail)
        {
            try
            {

                int bodyLength = inStr.Length;

                var endofTheBody = inStr.Substring(bodyLength - tailLength, tailLength);

                string[] endValues = endofTheBody.Split('\t');

                Nullable<MailItem> fixedMail = new MailItem
                (brokenMail.Value.MID,
                    idFolder: brokenMail.Value.IdFolder,
                    dateCreate: brokenMail.Value.DateCreate,
                    subject: brokenMail.Value.Subject,
                    fromAbonent: brokenMail.Value.FromAbonent,
                    replyTo: brokenMail.Value.ReplyTo,
                    toAbonent: brokenMail.Value.ToAbonent,
                    dateRecive: brokenMail.Value.DateRecive,
                    dateRead: brokenMail.Value.DateRead,
                    pA: brokenMail.Value.PA,
                    receipt: brokenMail.Value.Receipt,
                    dateReceipt: brokenMail.Value.DateReceipt,
                    idReceipt: brokenMail.Value.IdReceipt,
                    typeMessage: brokenMail.Value.TypeMessage,
                    dateSend: brokenMail.Value.DateSend,
                    idAbonent: brokenMail.Value.IdAbonent,
                    priority: brokenMail.Value.Priority,
                    isRead: brokenMail.Value.IsRead,
                    content: string.Concat(brokenMail.Value.Content, inStr.Substring(0, bodyLength - (endofTheBody.Length - endofTheBody.IndexOf('\t')))),
                    num: Convert.ToInt32(endValues[1]),
                    msgCategory: Convert.ToInt32(endValues[2]),
                    isEndOfFile: false
                );
                return fixedMail.Value;
            }
            catch (Exception e)
            {

                MessageBox.Show($"Произошла на этапе парсинга сломанного тела! \n Ошибка:{e.Message}"
    , "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new MailItem();
            }
        }


    }
}
