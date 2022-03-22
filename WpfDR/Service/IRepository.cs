using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfDR.Model;

namespace WpfDR.Service
{
    public interface IRepository<T> where T: Entity
    {
        IEnumerable<T> GetAll();
        T GetById(int id);
        int Add(T item);
        void Update(T item);
        bool Remove(int id);
        //bool AddRange(IEnumerable<T> itemList);
    }
}
