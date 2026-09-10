
using System;

namespace Causeless3t.UI
{
    public interface IBinder
    {
        void Bind();
        bool HasKey(string key);
    }
 
    public interface IPropertyBinder<T> 
    {
        void SetProperty(string key, T value);
        T GetProperty(string key);
    }
    
    public interface IEventBinder
    {
        void AddListener(string key, Delegate action);
        void RemoveListener(string key, Delegate action);
    }
    
    public interface ICommandBinder<T>
    {
        void InvokeMethod(string key, T param);
    }

    public interface IBinderManager
    {
        void RegisterBinder(IBinder dataBinder);
        void UnregisterBinder(IBinder binder);
        void SearchBinders();
        void RegisterUIEvents();
        void UnRegisterUIEvents();
    }
}
