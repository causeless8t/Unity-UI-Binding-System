using UnityEngine;

namespace Causeless3t.UI
{ 
    public abstract class BinderBase : MonoBehaviour, IBinder
    {
        [SerializeField][Tooltip("기본 컴포넌트 Getter의 키")]
        protected string getterKey;

        public abstract void Bind();
        public abstract bool HasKey(string key);
    }
    
    public abstract class ComponentBinder<T> : BinderBase, IPropertyBinder<T> where T : class
    {
        /// <summary>
        /// 연결할 컴포넌트
        /// </summary>
        protected T Target;
        
        private IBinderManager _binderManager;
        private bool _isInitialized;

        protected virtual void Awake()
        {
            EnsureInitialized();
        }

        protected virtual void OnEnable()
        {
            Bind();
        }

        protected virtual void OnDestroy()
        {
            Unbind();
            Target = null;
        }
        
        public override void Bind()
        {
            // 부모 BaseUI의 Awake에서 자식 Awake보다 먼저 호출될 수 있다.
            // Registry에 노출하기 전에 Target과 Key lookup을 준비한다.
            EnsureInitialized();

            var manager = GetComponentInParent<IBinderManager>(true);

            if (!ReferenceEquals(_binderManager, manager))
            {
                _binderManager?.UnregisterBinder(this);
                _binderManager = manager;
            }
            
            _binderManager?.RegisterBinder(this);
        }
        
        private void EnsureInitialized()
        {
            if (_isInitialized)
                return;

            Target = FindTarget();
            BuildBindings();
            _isInitialized = true;
        }

        private void Unbind()
        {
            if (_binderManager == null)
                return;

            _binderManager.UnregisterBinder(this);
            _binderManager = null;
        }
        
        protected bool IsGetterKey(string key)
        {
            return !string.IsNullOrEmpty(getterKey) && getterKey == key;
        }

        public override bool HasKey(string key) => IsGetterKey(key);

        /// <summary>
        /// 컴포넌트에 Serialize된 정보를 내부 Dictionary로 옮겨담습니다.
        /// </summary>
        protected virtual void BuildBindings() { }
        
        protected T GetTarget()
        {
            if (IsTargetNull())
                Target = FindTarget();
            return Target;
        }
        
        private bool IsTargetNull()
        {
            if (Target == null)
                return true;

            if (Target is Object unityObject)
                return unityObject == null;

            return false;
        }

        public void SetProperty(string key, T value)
        {
            // Base DataBinder는 기본 컴포넌트 Getter 용도로만 사용
        }

        /// <summary>
        /// 기본 컴포넌트의 Getter
        /// </summary>
        /// <param name="key">기본 Getter의 키</param>
        /// <returns>컴포넌트</returns>
        public T GetProperty(string key)
        {
            if (string.IsNullOrEmpty(getterKey)) return default;
            if (!getterKey.Equals(key)) return default;
            Target ??= FindTarget();
            return Target;
        }
        
        protected virtual T FindTarget()
        {
            if (typeof(T) == typeof(GameObject))
                return gameObject as T;

            if (typeof(T) == typeof(Transform))
                return transform as T;

            return GetComponent<T>();
        }
    }
}
