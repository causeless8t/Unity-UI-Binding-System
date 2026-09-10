using System;
using System.Collections.Generic;

namespace Causeless3t.UI
{
    internal sealed class BinderRegistry
    {
        private readonly HashSet<IBinder> _binders = new();
        private readonly Dictionary<(string key, Type type), IBinder> _cachedBinders = new();

        public void Register(IBinder binder)
        {
            if (binder == null)
                return;

            _binders.Add(binder);
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
    }
}