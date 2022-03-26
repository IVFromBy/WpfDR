using System.ComponentModel.DataAnnotations.Schema;

namespace WpfDR.Model
{
    [Table("MAIL_MESSAGES")]
    public class MailItemDb : Entity
    {
        public MailItemDb(string Mid, string DateCreate, string Subject,string FromAbonent
            ,string ReplayTo, string ToAbonent, string Content, bool IsBroken = false)
        {
            this.Mid = Mid;
            this.DateCreate = DateCreate;
            this.Subject = Subject;
            this.FromAbonent = FromAbonent;
            this.ReplayTo = ReplayTo;
            this.ToAbonent = ToAbonent;
            this.Content = Content;
            this.IsBroken = IsBroken;
        }
        public MailItemDb()
        {

        }
        public string Mid { get; set; }
        [Column("DATE_CREATE")]
        public string DateCreate { get; set; }
        public string Subject { get; set; }
        [Column("FROM_ABONENT")]
        public string FromAbonent { get; set; }
        [Column("REPLY_TO")]
        public string ReplayTo { get; set; }
        [Column("TO_ABONENT")]
        public string ToAbonent { get; set; }
        public string Content { get; set; }
        [NotMapped]
        public bool IsBroken { get; set; }
    }
}
