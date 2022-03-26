using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WpfDR.Model;

namespace WpfDR.Data
{
    public class WpfDb : DbContext
    {
        public WpfDb(DbContextOptions<WpfDb> options):base(options)
        {
        }

        public DbSet<MailItemDb> MailItemDbs { get; set; }
    }
}
