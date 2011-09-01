using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Astral.Plane.Container
{
    class RefCountContainer : IContainer
    {
        class CacheContainer
        {
            public int Count=1;
            public IContainer Container;
        }
        private static Dictionary<string, CacheContainer> _cache = new Dictionary<string, CacheContainer>();
        private string _key;
        bool _disposed = false;

        public RefCountContainer(string key, Func<IContainer> create)
        {
            lock (_cache)
            {
                _key = key;
                if (GetCache() == null)
                {
                    _cache[key] = new CacheContainer() { Container = create() };
                }
                else
                {
                    AddRef();
                }
            }
        }

        private IContainer TheContainer
        {
            get
            {
                var val = GetCache();
                if (val == null || _disposed)
                    throw new ObjectDisposedException("IContainer");
                return val.Container;
            }
        }

        private CacheContainer GetCache()
        {
            lock (_cache)
            {
                CacheContainer value;
                if (_cache.TryGetValue(_key, out value))
                {
                    return value;
                }
                return null;
            }
        }

        private void AddRef()
        {
            Interlocked.Increment(ref GetCache().Count);
        }

        private void Release()
        {
            var cacheEntry = GetCache();
            if (cacheEntry != null)
            {
                int count = Interlocked.Decrement(ref cacheEntry.Count);
                if (count == 0)
                {
                    cacheEntry.Container.Dispose();
                    _cache.Remove(_key);
                }
            }
        }

        public bool ContainsFile(string filename)
        {
            return TheContainer.ContainsFile(filename);
        }

        public System.IO.Stream GetFileStream(string filename, bool create = true)
        {
            return TheContainer.GetFileStream(filename, create);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                Release();
            }
        }
    }
}
