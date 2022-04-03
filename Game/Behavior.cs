namespace CSkyL.Game
{
    using System;
    using BFlags = System.Reflection.BindingFlags;

    public abstract class Behavior : UnityEngine.MonoBehaviour
    {
        public void Destroy() => Destroy(this);

        private void Awake() { _Init(); }
        protected abstract void _Init();

        private void Start() { _SetUp(); }
        protected virtual void _SetUp() { }

        private void Update() { _Update(); }
        protected virtual void _Update() { }

        private void LateUpdate() { _UpdateLate(); }
        protected virtual void _UpdateLate() { }

        private void OnDestroy() { _Destroy(); }
        protected virtual void _Destroy()
        {
            Log.Msg($"Destroying - <{GetType().Name}>");
            var stack = new System.Collections.Generic.Stack<_DestroyTask>();
            stack.Push(new _DestroyTask { obj = this, type = GetType() });

            while (stack.Count != 0) {
                var task = stack.Pop();
                foreach (var field in task.type.GetFields(BFlags.Public | BFlags.NonPublic |
                                                          BFlags.Instance | BFlags.DeclaredOnly)) {
                    if (!field.WithAtrribute<RequireDestructionAttribute>()) continue;

                    switch (field.GetValue(task.obj)) {
                    case UnityEngine.Object uobj:
                        Destroy(uobj); break;
                    case IDestruction any:
                        if (any is ICustomDestruction custom) custom._Destruct();
                        stack.Push(new _DestroyTask { obj = any, type = any.GetType() }); break;
                    case System.Collections.IEnumerable enumerable:
                        foreach (var item in enumerable)
                            stack.Push(new _DestroyTask { obj = item, type = item.GetType() });
                        break;
                    default:
                        Log.Warn($" -- field <{field.Name}> of type <{field.FieldType.Name}> " +
                                 $"does not implement <IDestruction>");
                        break;
                    }
                }
                if (task.type.BaseType is Type baseType &&
                            typeof(IDestruction).IsAssignableFrom(baseType))
                    stack.Push(new _DestroyTask { obj = task.obj, type = baseType });
            }
        }

        private struct _DestroyTask
        {
            public object obj;
            public Type type;
        }
    }

    public abstract class UnityGUI : Behavior
    {
        private void OnGUI() { _UnityGUI(); }
        protected abstract void _UnityGUI();
    }

    public interface IDestruction { }
    public interface ICustomDestruction : IDestruction
    { void _Destruct(); }

    [AttributeUsage(AttributeTargets.Field)]
    public class RequireDestructionAttribute : Attribute { }
}
