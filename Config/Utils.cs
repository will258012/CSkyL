namespace CSkyL.Config
{
    using System;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.Field)]
    public class ConfigAttribute : Lang.FieldNameAttribute
    {
        public readonly string description;
        public readonly string detail;
        public ConfigAttribute(string name, string TranslateDescription_Key, string TranslateDetail_Key = "") : base(name)
        { this.description = Translation.Translations.Translate(TranslateDescription_Key); this.detail = Translation.Translations.Translate(TranslateDetail_Key); }
    }
    public interface IConfigData : Lang.IFieldWithName
    {
        bool AssignByParsing(string str);
        void Assign(object objOfSameType);
        void _set(string name, string description, string detail);
        string Description { get; }
        string Detail { get; }
    }
    public class ConfigData<T> : IConfigData
    {
        public static implicit operator T(ConfigData<T> data) => data._data;
        public ConfigData(T a) { this._data = a; }

        public virtual void Assign(object other)
        {
            if (other is ConfigData<T> o) Assign(o);
            else Log.Warn($"ConfigData<{typeof(T).Name}> assigned with <{other.GetType().Name}>");
        }
        public virtual T Assign(T data) { return _data = data; }
        public override string ToString() => _data.ToString();
        public virtual bool AssignByParsing(string str)
        {
            try { Assign((T) TypeDescriptor.GetConverter(_data).ConvertFromString(str)); }
            catch {
                CSkyL.Log.Err("Config loading: cannot convert " +
                              $"\"{str}\" to type[{typeof(T).Name}]");
                return false;
            }
            return true;
        }

        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Detail { get; private set; }
        public void _set(string name) => _set(name, "", "");
        public void _set(string name, string description, string detail)
        { Name = name; Description = description; Detail = detail; }

        protected T _data;
    }

    public class CfFloat : ConfigData<float>
    {
        public sealed override float Assign(float num) =>
            _data = num < Min ? Min : num > Max ? Max : num;

        public float Max { get; }
        public float Min { get; }

        public CfFloat(float num, float min = float.MinValue, float max = float.MaxValue) : base(num)
        { Min = min; Max = max; Assign(num); }
    }

    public class OffsetConfig
    {
        public OffsetConfig(CfFloat forward, CfFloat up, CfFloat right,
                            CfFloat yawDegree = null, CfFloat pitchDegree = null)
        {
            this.forward = forward; this.up = up; this.right = right;
            this.yawDegree = yawDegree ?? new CfFloat(0f, -180f, 180f);
            this.pitchDegree = pitchDegree ?? new CfFloat(0f, -90f, 90f);
        }

        public override string ToString() => $"{forward},{up},{right},{yawDegree},{pitchDegree}";
        public readonly CfFloat forward, up, right;
        public readonly CfFloat yawDegree, pitchDegree;
    }
    public class CfOffset : ConfigData<OffsetConfig>
    {
        public CfOffset(CfFloat forward, CfFloat up, CfFloat right,
                        CfFloat yawDegree = null, CfFloat pitchDegree = null)
            : base(new OffsetConfig(forward, up, right, yawDegree, pitchDegree)) { }

        public CfFloat forward => _data.forward;
        public CfFloat up => _data.up;
        public CfFloat right => _data.right;
        public CfFloat yawDegree => _data.yawDegree;
        public CfFloat pitchDegree => _data.pitchDegree;

        public Transform.Offset AsOffSet => new Transform.Offset(
            new Transform.LocalMovement { forward = forward, up = up, right = right },
            new Transform.DeltaAttitude(yawDegree, pitchDegree)
        );

        public void Assign(Transform.Offset offset)
        {
            _data.forward.Assign(offset.movement.forward);
            _data.up.Assign(offset.movement.up);
            _data.right.Assign(offset.movement.right);
            _data.yawDegree.Assign(offset.deltaAttitude.yawDegree);
            _data.pitchDegree.Assign(offset.deltaAttitude.pitchDegree);
        }

        public override OffsetConfig Assign(OffsetConfig data)
        {
            _data.forward.Assign(data.forward);
            _data.up.Assign(data.up);
            _data.right.Assign(data.right);
            _data.yawDegree.Assign(data.yawDegree);
            _data.pitchDegree.Assign(data.pitchDegree);
            return _data;
        }

        public override bool AssignByParsing(string str)
        {
            var strs = str.Split(',');
            if (strs.Length != 5) return false;
            try {
                _data.forward.Assign(float.Parse(strs[0]));
                _data.up.Assign(float.Parse(strs[1]));
                _data.right.Assign(float.Parse(strs[2]));
                _data.yawDegree.Assign(float.Parse(strs[3]));
                _data.pitchDegree.Assign(float.Parse(strs[4]));
            }
            catch { return false; }
            return true;
        }
    }

    public class CfScreenPosition : ConfigData<CSkyL.Math.Vec2D>
    {
        public CfScreenPosition(CSkyL.Math.Vec2D v) : base(v) { }

        public float x => _data.x;
        public float y => _data.y;

        public override string ToString() => $"{x},{y}";

        public override bool AssignByParsing(string str)
        {
            var strs = str.Split(',');
            if (strs.Length != 2) return false;
            try {
                _data.x = float.Parse(strs[0]);
                _data.y = float.Parse(strs[1]);
            }
            catch { return false; }
            return true;
        }
    }
}
