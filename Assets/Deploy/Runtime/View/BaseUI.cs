
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Causeless3t.UI
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class BaseUIEventRegisterAttribute : System.Attribute
    {
        public Type EventType { get; }

        public BaseUIEventRegisterAttribute(System.Type type)
        {
            EventType = type;
        }
    }

    public class BaseUI : MonoBehaviour, IBinderManager
    {
        private readonly struct UIEventCallback
        {
            public string Key { get; }
            public Delegate Callback { get; }

            public UIEventCallback(string key, Delegate callback)
            {
                Key = key;
                Callback = callback;
            }
        }
        
        private readonly BinderRegistry _binderRegistry = new();
        private readonly List<UIEventCallback> _uiEventCallbacks = new();

        private bool _isUIEventsRegistered;
        private bool _isUIEventCallbacksInitialized;
        
        public bool IsInitializedBinder { get; private set; }
        
        public RectTransform RectTransform => this.transform as RectTransform;
        /// <summary>
        /// 씬 자체에 이미 있는 Ui인지 여부
        /// </summary>
        public bool IsOriginUi { get; set; }

        /// <summary>
        /// Ui가 오픈된 후 불리는 Event
        /// </summary>
        public event Action<BaseUI> Opened;
        /// <summary>
        /// Ui가 닫히기 직전 불리는 Event
        /// </summary>
        public event Action<BaseUI> Closing;

        #region member method

        protected virtual void Awake()
        {
            SearchBinders();
        }

        protected virtual void OnEnable()
        {
            RegisterUIEvents();
        }

        protected virtual void OnDisable()
        {
            UnRegisterUIEvents();
        }

        protected virtual void OnDestroy()
        {
            _binderRegistry.Clear();
        }
        
        
        public virtual void Open()
        {
            gameObject.SetActive(true);

            OnOpened();
            
            Opened?.Invoke(this);
            Opened = null;
        }
        
        protected virtual void OnOpened()
        {
        }

        public virtual void Close()
        {
            OnClosing();
            
            Closing?.Invoke(this);
            Closing = null;
            
            gameObject.SetActive(false);
        }
        
        protected virtual void OnClosing()
        {
        }

        #endregion
        
        #region Property Binding
        
        /// <summary>
        /// 프로퍼티의 Setter에서 호출하여 변경사항을 키와 연결된 Ui에 적용합니다.  
        /// </summary>
        /// <param name="key">Ui에 연결된 키</param>
        /// <param name="type">프로퍼티의 타입</param>
        /// <param name="value">프로퍼티의 값</param>
        public void BroadcastSetProperty<T>(string key, T value)
        {
            foreach (var binder in _binderRegistry.FindAll<IPropertyBinder<T>>(key))
            {
                binder.SetProperty(key, value);
            }
        }

        /// <summary>
        /// 키와 연결된 Ui에서 값을 가져옵니다.
        /// </summary>
        /// <param name="key">Ui에 연결된 키</param>
        /// <returns>Ui의 값</returns>
        public T BroadcastGetProperty<T>(string key)
        {
            var binder = _binderRegistry.FindFirst<IPropertyBinder<T>>(key);

            return binder != null ? binder.GetProperty(key) : default;
        }
        
        #endregion

        #region Command Binding
        
        /// <summary>
        /// 키와 연결된 Ui의 메소드를 호출합니다.
        /// </summary>
        /// <param name="key">Ui에 연결된 키</param>
        /// <param name="param">파라미터의 값</param>
        // BaseUi -> UI Component (ex: GameObject.SetActive(bool))
        public void BroadcastInvokeMethod<T>(string key, T param)
        {
            foreach (var binder in _binderRegistry.FindAll<ICommandBinder<T>>(key))
            {
                binder.InvokeMethod(key, param);
            }
        }
        
        #endregion
        
        #region Binder Management
        
        /// <summary>
        /// 바인더를 이 Ui의 리스너로 등록합니다.
        /// </summary>
        /// <param name="dataBinder">등록할 바인더</param>
        public void RegisterBinder(IBinder binder)
        {
            if (!_binderRegistry.Register(binder))
                return;

            // OnEnable 이후 동적으로 생성된 Binder라면
            // 해당 Binder의 이벤트도 즉시 등록한다.
            if (_isUIEventsRegistered)
            {
                RegisterUIEventsToBinder(binder);
            }
        }
        
        public void UnregisterBinder(IBinder binder)
        {
            _binderRegistry.Unregister(binder);
        }

        /// <summary>
        /// 하위에 있는 Binder 요소를 탐색한 후 바인딩한다.
        /// 동적으로 생성되는 객체의 경우 따로 호출해줘야 정상적으로 Binder들이 등록됩니다.
        /// </summary>
        public void SearchBinders()
        { 
            var restoreUIEvents = _isUIEventsRegistered;

            if (restoreUIEvents)
            {
                UnRegisterUIEvents();
            }

            _binderRegistry.Clear();

            var binders = GetComponentsInChildren<IBinder>(true);

            foreach (var binder in binders)
            {
                binder.Bind();
            }

            IsInitializedBinder = true;

            if (restoreUIEvents)
            {
                RegisterUIEvents();
            }
        }
        
        #endregion
        
        #region Event Binding
        
        /// <summary>
        /// UI 이벤트의 콜백을 등록하기 위한 메소드
        /// </summary>
        public void RegisterUIEvents()
        {
            if (_isUIEventsRegistered)
                return;

            EnsureUIEventCallbacks();
            
            foreach (var eventCallback in _uiEventCallbacks)
            {
                RegisterUIEvent(
                    eventCallback.Key,
                    eventCallback.Callback);
            }

            _isUIEventsRegistered = true;
        }

        /// <summary>
        /// UI 이벤트의 콜백을 등록해제하기 위한 메소드
        /// </summary>
        public void UnRegisterUIEvents()
        {
            if (!_isUIEventsRegistered)
                return;

            foreach (var eventCallback in _uiEventCallbacks)
            {
                UnregisterUIEvent(
                    eventCallback.Key,
                    eventCallback.Callback);
            }

            _isUIEventsRegistered = false;
        }
        
        protected void RegisterUIEvent(string key, Delegate action)
        {
            foreach (var binder in _binderRegistry.FindAll<IEventBinder>(key))
            {
                binder.AddListener(key, action);
            }
        }
        
        protected void UnregisterUIEvent(string key, Delegate action)
        {
            foreach (var binder in _binderRegistry.FindAll<IEventBinder>(key))
            {
                binder.RemoveListener(key, action);
            }
        }
        
        private void EnsureUIEventCallbacks()
        {
            if (_isUIEventCallbacksInitialized)
                return;

            var bindings = UIEventBindingRegistry.GetBindings(GetType());

            foreach (var binding in bindings)
            {
                try
                {
                    var callback = Delegate.CreateDelegate(
                        binding.DelegateType,
                        this,
                        binding.Method);

                    _uiEventCallbacks.Add(
                        new UIEventCallback(
                            binding.Key,
                            callback));
                }
                catch (ArgumentException e)
                {
                    Debug.LogError(
                        $"Failed to create UI event delegate '{binding.Key}' " +
                        $"for {GetType().Name}.{binding.Method.Name}\n{e.Message}",
                        this);
                }
            }
            
            _isUIEventCallbacksInitialized = true;
        }
        
        private void RegisterUIEventsToBinder(IBinder binder)
        {
            if (binder is not IEventBinder eventBinder)
                return;

            EnsureUIEventCallbacks();
            
            foreach (var eventCallback in _uiEventCallbacks)
            {
                if (!binder.HasKey(eventCallback.Key))
                    continue;

                eventBinder.AddListener(eventCallback.Key, eventCallback.Callback);
            }
        }
        
        #endregion
    }
}