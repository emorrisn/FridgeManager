using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FridgeManager
{
    public class Cache
    {
        private Dictionary<string, object> cache;

        public Cache()
        {
            cache = new Dictionary<string, object>();
        }

        public T GetOrAdd<T>(string key, Func<T> valueFactory)
        {
            if (cache.TryGetValue(key, out object cachedValue))
            {
                return (T)cachedValue;
            }

            T newValue = valueFactory();
            cache[key] = newValue;
            return newValue;
        }

        public void Remove(string key)
        {
            if (cache.ContainsKey(key))
            {
                cache.Remove(key);
            }
        }

        public void Clear()
        {
            cache.Clear();
        }
    }
}
