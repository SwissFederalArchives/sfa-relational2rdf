using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.DataSources.Siard.Utils
{
    public class PooledList<T> where T : class, new()
    {
        private readonly List<T> _list;
        private readonly DefaultObjectPool<T> _pool;

        public IEnumerable<T> List => _list;

        public PooledList(int initialCapacity = 0, IPooledObjectPolicy<T> policy = null)
        {
            _list = new List<T>(initialCapacity);
            _pool = new DefaultObjectPool<T>(policy ?? new DefaultPooledObjectPolicy<T>(), 128);
            for (int i = 0; i < initialCapacity; i++)
                _pool.Get();
        }

        public void Clear()
        {
            _list.ForEach(_pool.Return);
            _list.Clear();
        }

        public T GetNext()
        {
            var item = _pool.Get();
            _list.Add(item);
            return item;
        }
    }
}
