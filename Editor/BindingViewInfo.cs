using UnityEngine;

namespace Causeless3t.UI.Editor
{
    internal enum BindingViewKind
    {
        Getter,
        Binding,
        EventHandler
    }

    internal sealed class BindingViewInfo
    {
        public string Key { get; }
        public string OwnerName { get; }
        public string Description { get; }
        public BindingViewKind Kind { get; }
        public Object Target { get; }

        public BindingViewInfo(
            string key,
            string ownerName,
            string description,
            BindingViewKind kind,
            Object target)
        {
            Key = key;
            OwnerName = ownerName;
            Description = description;
            Kind = kind;
            Target = target;
        }
    }
}