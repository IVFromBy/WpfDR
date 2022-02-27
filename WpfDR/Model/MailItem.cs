using System;
using CsvHelper.Configuration.Attributes;

namespace WpfDR.Model
{
    public struct MailItem
    {
        //public MailItem()
        //{

        //}
        
        public MailItem(string mID, int idFolder, DateTime dateCreate, string subject, string fromAbonent, string replyTo, string toAbonent
            , DateTime? dateRecive, DateTime? dateRead, int pA, int receipt, DateTime? dateReceipt, int? idReceipt, int typeMessage
            , DateTime? dateSend, int idAbonent, int priority, int isRead, string content, int num, int msgCategory, bool isEndOfFile)
        {
            MID = mID;
            IdFolder = idFolder;
            DateCreate = dateCreate;
            Subject = subject;
            FromAbonent = fromAbonent;
            ReplyTo = replyTo;
            ToAbonent = toAbonent;
            DateRecive = dateRecive;
            DateRead = dateRead;
            PA = pA;
            Receipt = receipt;
            DateReceipt = dateReceipt;
            IdReceipt = idReceipt;
            TypeMessage = typeMessage;
            DateSend = dateSend;
            IdAbonent = idAbonent;
            Priority = priority;
            IsRead = isRead;
            Content = content;
            Num = num;
            MsgCategory = msgCategory;
            IsEndOfFile = isEndOfFile;
        }
        

        public string MID { get; init; }
        public int IdFolder { get; init; }
        [Format("dd.MM.yyyy")]
        public DateTime DateCreate { get; init; }
        public string Subject { get; init; }
        public string FromAbonent { get; init; }
        public string ReplyTo { get; init; }
        public string ToAbonent { get; init; }
        [Format("dd.MM.yyyy"), NullValues("?")]
        public DateTime? DateRecive { get; init; }
        [Format("dd.MM.yyyy"), NullValues("?")]
        public DateTime? DateRead { get; init; }
        public int PA { get; init; }
        public int Receipt { get; set; }
        [Format("dd.MM.yyyy"), NullValues("?")]
        public DateTime? DateReceipt { get; init; }
        [NullValues("?")]
        public int? IdReceipt { get; init; }
        public int TypeMessage { get; init; }
        [Format("dd.MM.yyyy"), NullValues("?")]
        public DateTime? DateSend { get; init; }
        public int IdAbonent { get; init; }
        public int Priority { get; init; }
        public int IsRead { get; init; }
        public string Content { get; init; }
        public int Num { get; init; }
        public int MsgCategory { get; init; }
        [Ignore]
        public bool IsEndOfFile { get; set; }
    }
}
