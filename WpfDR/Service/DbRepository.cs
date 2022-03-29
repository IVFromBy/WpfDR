using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WpfDR.Data;
using WpfDR.Model;

namespace WpfDR.Service
{
    public class DbRepository<T> : IRepository<T> where T : Entity
    {
        private readonly WpfDb db;
        private DbSet<T> Set { get; }
        public DbRepository(WpfDb db)
        {
            this.db = db;
            Set = db.Set<T>();
        }

        public int Add(T item)
        {

            db.Entry(item).State = EntityState.Added;
            db.SaveChanges();
            return item.id;
        }

        public IEnumerable<T> GetAll() => Set;


        public T GetById(int id) => Set.Find(id);

        public bool Remove(int id)
        {
            var item = GetById(id);
            if (item is null)
                return false;

            Set.Remove(item);

            return true;

        }

        public void Update(T item)
        {
            db.Entry(item).State = EntityState.Modified;
            db.SaveChanges();
        }

        public async Task<bool> AddRange(IEnumerable<T> itemList)
        {
            await db.AddRangeAsync(itemList);
            await db.SaveChangesAsync();
            return true;
        }

        public bool DeleteAll()
        {
            db.RemoveRange(Set);
            db.SaveChanges();
            return true;
        }
    }
}
