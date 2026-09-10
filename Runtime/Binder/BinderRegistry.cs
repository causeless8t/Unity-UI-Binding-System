using System;
using System.Collections.Generic;

namespace Causeless3t.UI
{
    internal sealed class BinderRegistry
    {
        private readonly List<IBinder> _binders = new();
        private readonly HashSet<IBinder> _binderSet = new();
        private readonly Dictionary<(string key, Type type), IBinder> _cachedBinder = new();

        public bool Register(IBinder binder)
        {
            if (binder == null)
                return false;
            
            if (!_binderSet.Add(binder))
                return false;
            
            _binders.Add(binder);

            return true;
        }
        
        public bool Unregister(IBinder binder)
        {
            if (binder == null)
                return false;
            
            if (!_binderSet.Remove(binder))
                return false;

            _binders.Remove(binder);
            RemoveCache(binder);

            return true;
        }

        public void Clear()
        {
            _binders.Clear();
            _binderSet.Clear();
            _cachedBinder.Clear();
        }

        public T FindFirst<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key))
                return null;
            
            var cacheKey = (key, typeof(T));

            if (_cachedBinder.TryGetValue(cacheKey, out var cachedBinder))
            {
                if (cachedBinder is T typedBinder)
                    return typedBinder;

                _cachedBinder.Remove(cacheKey);
            }

            foreach (var binder in _binders)
            {
                if (!binder.HasKey(key))
                    continue;
                
                if (binder is not T typedBinder)
                    continue;
                
                _cachedBinder[cacheKey] = binder;
                return typedBinder;
            }

            return null;
        }
        
        public IEnumerable<T> FindAll<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key))
                yield break;

            foreach (var binder in _binders)
            {
                if (!binder.HasKey(key))
                    continue;

                if (binder is T typedBinder)
                    yield return typedBinder;
            }
        }
        
        private void RemoveCache(IBinder binder)
        {
            var removeKeys = new List<(string key, Type type)>();

            foreach (var pair in _cachedBinder)
            {
                if (ReferenceEquals(pair.Value, binder))
                    removeKeys.Add(pair.Key);
            }

            foreach (var key in removeKeys)
                _cachedBinder.Remove(key);
        }
    }
}