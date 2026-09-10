using System;
using System.Collections.Generic;

namespace Causeless3t.UI
{
    internal sealed class BinderRegistry
    {
        private readonly HashSet<IBinder> _binders = new();
        private readonly Dictionary<(string key, Type type), IBinder> _cachedBinders = new();

        public bool Register(IBinder binder)
        {
            if (binder == null)
                return false;

            return _binders.Add(binder);
        }
        
        public bool Unregister(IBinder binder)
        {
            if (binder == null)
                return false;

            var removed = _binders.Remove(binder);

            if (removed)
                RemoveCacheFor(binder);

            return removed;
        }

        public void Clear()
        {
            _binders.Clear();
            _cachedBinders.Clear();
        }

        public T Find<T>(string key) where T : class
        {
            var cacheKey = (key, typeof(T));

            if (_cachedBinders.TryGetValue(cacheKey, out var cachedBinder))
            {
                return cachedBinder as T;
            }

            foreach (var binder in _binders)
            {
                if (!binder.HasKey(key))
                    continue;
                
                if (binder is not T targetBinder)
                    continue;

                

                _cachedBinders[cacheKey] = binder;
                return targetBinder;
            }

            return null;
        }
        
        private void RemoveCacheFor(IBinder binder)
        {
            var removeKeys = new List<(string key, Type type)>();

            foreach (var pair in _cachedBinders)
            {
                if (ReferenceEquals(pair.Value, binder))
                    removeKeys.Add(pair.Key);
            }

            foreach (var key in removeKeys)
                _cachedBinders.Remove(key);
        }
    }
}