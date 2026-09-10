using UnityEngine;

namespace Causeless3t.UI
{ 
    public abstract class DataBinder<T> : MonoBehaviour, IBinder, IDataBinder<T> where T : class
    {
        [SerializeField][Tooltip("기본 컴포넌트 Getter의 키")]
        protected string getterKey;
        
        /// <summary>
        /// 연결할 컴포넌트
        /// </summary>
        protected T Target;
        
        private IBinderManager _binderManager;

        protected virtual void Awake()
        {
            Target = FindTarget();
        }

        protected virtual void OnEnable()
        {
            Bind();
            LoadData();
        }

        protected virtual void OnDestroy()
        {
            Unbind();
            Target = null;
        }
        
        public void Bind()
        {
            var manager = GetComponentInParent<IBinderManager>(true);

            // 이미 동일 Manager에 등록되어 있음
            if (ReferenceEquals(_binderManager, manager))
                return;

            // 부모가 변경된 경우 기존 Manager에서 제거
            _binderManager?.UnregisterBinder(this);

            _binderManager = manager;
            _binderManager?.RegisterBinder(this);
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

        public virtual bool HasKey(string key) => IsGetterKey(key);

        /// <summary>
        /// 컴포넌트에 Serialize된 정보를 내부 Dictionary로 옮겨담습니다.
        /// </summary>
        protected virtual void LoadData() { }
        
        public virtual string[] GetKeyList() { return null; }

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
