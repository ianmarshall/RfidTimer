using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaceTimer.Common
{
    public class LazyConcurrentDictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, Lazy<TValue>> concurrentDictionary;

        public LazyConcurrentDictionary()
        {
            this.concurrentDictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>();
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            var lazyResult = this.concurrentDictionary.GetOrAdd(key, k => new Lazy<TValue>(() => valueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication));

            return lazyResult.Value;
        }

        public bool ContainsKey(TKey key)
        {
            return this.concurrentDictionary.ContainsKey(key);

        }
    }
}
