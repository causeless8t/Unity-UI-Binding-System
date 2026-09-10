
using System;
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
        private readonly BinderRegistry _binderRegistry = new();

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
        
        /// <summary>
        /// 프로퍼티의 Setter에서 호출하여 변경사항을 키와 연결된 Ui에 적용합니다.  
        /// </summary>
        /// <param name="key">Ui에 연결된 키</param>
        /// <param name="type">프로퍼티의 타입</param>
        /// <param name="value">프로퍼티의 값</param>
        public void BroadcastSetProperty<T>(string key, T value)
        {
            _binderRegistry.Find<IDataBinder<T>>(key)?.SetProperty(key, value);
        }

        /// <summary>
        /// 키와 연결된 Ui에서 값을 가져옵니다.
        /// </summary>
        /// <param name="key">Ui에 연결된 키</param>
        /// <returns>Ui의 값</returns>
        public T BroadcastGetProperty<T>(string key)
        {
            var binder = _binderRegistry.Find<IDataBinder<T>>(key);

            return binder != null ? binder.GetProperty(key) : default;
        }
        
        /// <summary>
        /// 키와 연결된 Ui의 메소드를 호출합니다.
        /// </summary>
        /// <param name="key">Ui에 연결된 키</param>
        /// <param name="param">파라미터의 값</param>
        // BaseUi -> UI Component (ex: GameObject.SetActive(bool))
        public void BroadcastInvokeMethod<T>(string key, T param)
        {
            _binderRegistry.Find<ICommandBinder<T>>(key)?.InvokeMethod(key, param);
        }
        
        protected void RegisterUIEvent(string key, Delegate action)
        {
            _binderRegistry.Find<IUIEventBinder>(key)?.AddListener(key, action);
        }
        
        protected void UnregisterUIEvent(string key, Delegate action)
        {
            _binderRegistry.Find<IUIEventBinder>(key)?.RemoveListener(key, action);
        }

        /// <summary>
        /// 바인더를 이 Ui의 리스너로 등록합니다.
        /// </summary>
        /// <param name="dataBinder">등록할 바인더</param>
        public void RegisterBinder(IBinder binder)
        {
            _binderRegistry.Register(binder);
        }

        /// <summary>
        /// 하위에 있는 Binder 요소를 탐색한 후 바인딩한다.
        /// 동적으로 생성되는 객체의 경우 따로 호출해줘야 정상적으로 Binder들이 등록됩니다.
        /// </summary>
        public void SearchBinders()
        { 
            _binderRegistry.Clear();
            var binders = GetComponentsInChildren<IBinder>(true);

            
            foreach (var binder in binders)
                binder.Bind(); // 하위로부터 바인딩하는 형태로 만든 이유는 부모가 active되지 않을 경우 등록되지 않는 경우가 생기기 때문

            IsInitializedBinder = true;
        }

        /// <summary>
        /// UI 이벤트의 콜백을 등록하기 위한 메소드
        /// </summary>
        public void RegisterUIEvents()
        {
            SetUIEventsRegistered(true);
        }

        /// <summary>
        /// UI 이벤트의 콜백을 등록해제하기 위한 메소드
        /// </summary>
        public void UnRegisterUIEvents()
        {
            SetUIEventsRegistered(false);
        }
        
        private void SetUIEventsRegistered(bool register)
        {
            foreach (var binding in UIEventBindingRegistry.GetBindings(GetType()))
            {
                try
                {
                    var callback = Delegate.CreateDelegate(binding.DelegateType, 
                        this,
                        binding.Method);

                    if (register)
                        RegisterUIEvent(binding.Key, callback);
                    else
                        UnregisterUIEvent(binding.Key, callback);
                }
                catch (TargetParameterCountException)
                {
                    Debug.LogError(
                        $"Invalid UI event binding: {binding.Method.Name}");
                }
            }
        }
    }
}