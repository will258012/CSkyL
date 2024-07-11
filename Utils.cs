namespace CSkyL
{
    using ColossalFramework.Math;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;

    public static class Lang
    {
        public class _Object<Target>
        {
            public _Object(Target target) => _target = target;
            public Field Get<Field>(string fieldName, Type type = null)
            {
                type = type ?? typeof(Target);
                if (type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Static |
                                             BindingFlags.Public | BindingFlags.NonPublic)
                            is FieldInfo field)
                    return (Field) field.GetValue(_target);

                if (type.BaseType is Type baseType && baseType != typeof(object))
                    return Get<Field>(fieldName, baseType);
                else Log.Warn($"GetField fails: <{fieldName}> not of <{typeof(Target).Name}>" +
                              $"(up to <{type.Name}>)");
                return default;
            }
            public void Set<Field>(string fieldName, Field value, Type type = null)
            {
                type = type ?? typeof(Target);
                if (type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Static |
                                             BindingFlags.Public | BindingFlags.NonPublic)
                            is FieldInfo field)
                    field.SetValue(_target, value);
                else if (type.BaseType is Type baseType && baseType != typeof(object))
                    Set(fieldName, value, baseType);
                else Log.Warn($"SetField fails: <{fieldName}> not of <{typeof(Target).Name}>" +
                              $"(up to <{type.Name}>)");
            }
            public void Invoke(string methodName, params object[] args)
                => Invoke(methodName, typeof(Target), args: args);
            public void Invoke(string methodName, Type type, params object[] args)
            {
                if (type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static |
                                               BindingFlags.Public | BindingFlags.NonPublic)
                            is MethodInfo method)
                    method.Invoke(_target, args);
                else if (type.BaseType is Type baseType && baseType != typeof(object))
                    Invoke(methodName, baseType, args: args);
                else Log.Warn($"Invoke fails: <{methodName}> not of <{typeof(Target).Name}>" +
                              $"(up to <{type.Name}>)");
            }
            private readonly Target _target;
        }
        public static _Object<Target> In<Target>(Target target)
            => new _Object<Target>(target);

        public static bool WithAtrribute<Attr>(this FieldInfo fieldInfo) where Attr : Attribute
            => (fieldInfo.GetCustomAttributes(typeof(Attr), false) as Attr[])?.Any() ?? false;

        /* -------- Field Name Attribute ------------------------------------------------------- */

        [AttributeUsage(AttributeTargets.Field)]
        public class FieldNameAttribute : Attribute
        {
            public readonly string name;
            public FieldNameAttribute(string name) { this.name = name; }
        }

        public interface IFieldWithName
        {
            void _set(string name);
            string Name { get; }
        }

        public delegate void AttrLoader<in Attr>(IFieldWithName field, Attr attribute)
                                where Attr : FieldNameAttribute;

        public static void LoadFieldNameAttribute<Attr>(object obj, AttrLoader<Attr> attributeLoader)
                                    where Attr : FieldNameAttribute
        {
            foreach (var fieldInfo in obj.GetType().GetFields(
                                            BindingFlags.Instance | BindingFlags.Public)) {
                if (fieldInfo.GetValue(obj) is IFieldWithName field) {
                    var attrs = fieldInfo.GetCustomAttributes(typeof(Attr), false) as Attr[];
                    if (attrs?.Any() ?? false) attributeLoader(field, attrs[0]);
                }
            }
        }
        public static void LoadFieldNameAttribute<Attr, T>(AttrLoader<Attr> attributeLoader)
            where Attr : FieldNameAttribute
        {
            foreach (var fieldInfo in typeof(T).GetFields(
                                            BindingFlags.Static | BindingFlags.Public)) {
                if (fieldInfo.GetValue(null) is IFieldWithName field) {
                    var attrs = fieldInfo.GetCustomAttributes(typeof(Attr), false) as Attr[];
                    if (attrs?.Any() ?? false) attributeLoader(field, attrs[0]);
                }
            }
        }
        public static void LoadFieldNameAttribute<T>(T obj)
            => LoadFieldNameAttribute(obj,
                        (IFieldWithName field, FieldNameAttribute attr) => field._set(attr.name));
        public static void LoadFieldNameAttribute<T>()
            => LoadFieldNameAttribute<FieldNameAttribute, T>(
                        (IFieldWithName field, FieldNameAttribute attr) => field._set(attr.name));
    }

    public static class Math
    {
        public static bool AlmostEquals(this float a, float b, float error = 1 / 32f)
            => System.Math.Abs(b - a) < error;

        public static T AdvanceToTarget<T>(this T value, T target,
                                float advanceRatio, Range rangeOfChange,
                                Diff<T> difference, LinearInterpolation<T> interpolation)
        {
            Log.Assert(rangeOfChange.min >= 0, "AdvanceToTarget: rangeOfChange must >= 0");

            var diff = difference(value, target);
            if (diff < rangeOfChange.min) return target;

            advanceRatio = diff * advanceRatio > rangeOfChange.max ? rangeOfChange.max / diff :
                            diff * advanceRatio < rangeOfChange.min ? rangeOfChange.min / diff :
                            advanceRatio;
            return interpolation(value, target, advanceRatio);
        }
        public delegate float Diff<in T>(T a, T b);
        public delegate T LinearInterpolation<T>(T a, T b, float t);

        public static float AdvanceToTarget(this float value, float target,
            float advanceRatio, Range rangeOfChange)
            => AdvanceToTarget(value, target, advanceRatio, rangeOfChange,
                 (a, b) => System.Math.Abs(a - b), (a, b, t) => a + (b - a) * t);

        public struct Vec2D
        {
            public float x { get => _x; set => _x = value; }
            public float y { get => _y; set => _y = value; }

            public float width {
                get => _x < 0f ? 0f : _x;
                set => _x = value < 0f ? 0f : value;
            }
            public float height {
                get => _y < 0f ? 0f : _y;
                set => _y = value < 0f ? 0f : value;
            }

            public static Vec2D Position(float x, float y)
                => new Vec2D { x = x, y = y };
            public static Vec2D Size(float width, float height)
                => new Vec2D { width = width, height = height };

            internal Vector2 _AsVec2 => new Vector2(_x, _y);
            internal static Vec2D _FromVec2(Vector2 v)
                => new Vec2D { _x = v.x, _y = v.y };

            public override string ToString() => $"({_x}, {_y})";

            private float _x, _y;
        }

        public struct Range
        {
            public Range(float min = float.MinValue, float max = float.MaxValue)
            {
                this.min = float.IsNaN(min) ? float.MinValue : min;
                this.max = float.IsNaN(max) ? float.MaxValue :
                                              max < min ? min : max;
            }

            public float min, max;
            public float Size => max - min;
        }

        public static bool InRange(this float value, Range range)
            => value >= range.min && value <= range.max;

        public static float Clamp(this float value, float min, float max)
            => value.Clamp(new Range(min, max));
        public static float Clamp(this float value, Range clampRange)
            => value < clampRange.min ? clampRange.min :
               value > clampRange.max ? clampRange.max : value;

        public static float Modulus(this float value, Range modulusRange)
        {
            Log.Assert(modulusRange.max > modulusRange.min,
                       "Modulus: modulus range cannot be empty.");

            var range = modulusRange.max - modulusRange.min;
            value = (value - modulusRange.min) % range;
            if (value < 0f) value += range;

            return value + modulusRange.min;
        }

        public static T GetRandomOne<T>(this IEnumerable<T> enumerable)
        {
            var list = enumerable.ToList();
            return list.Any() ? list[_random.Next(list.Count)] : default;
        }

        public static bool RandomTrue(double probability = .5)
            => _random.NextDouble() < probability;

        private static System.Random _random {
            get => __random ?? (__random = new System.Random(Environment.TickCount));
        }
        private static System.Random __random;


        /// <summary>
        /// Creates Bezier using start/end pos/dir
        /// </summary>
        /// <param name="startSmooth">true for middle nodes and transitions</param>
        /// <param name="endSmooth">true for middle nodes and transitions</param>
        /// <param name="startDir">should be going toward the end of the bezier.</param>
        /// <param name="endDir">should be going toward the start of the  bezier.</param>
        public static Bezier3 Bezier3ByDir(Vector3 startPos, Vector3 startDir,
                                           Vector3 endPos, Vector3 endDir,
                                           bool startSmooth = false, bool endSmooth = false)
        {
            NetSegment.CalculateMiddlePoints(
                startPos, startDir,
                endPos, endDir,
                startSmooth, endSmooth,
                out Vector3 MiddlePoint1, out Vector3 MiddlePoint2);
            return new Bezier3
            {
                a = startPos,
                d = endPos,
                b = MiddlePoint1,
                c = MiddlePoint2,
            };
        }

        /// <summary>
        /// Calculates length of the Bezier arc by subdividing it into several <paramref name="step"/>s.
        /// </summary>
        public static float ArcLength(this Bezier3 beizer, float step = 0.1f)
        {
            float ret = 0;
            float t;
            for (t = step; t < 1f; t += step) {
                float len = (beizer.Position(t) - beizer.Position(t - step)).magnitude;
                ret += len;
            }
            {
                float len = (beizer.d - beizer.Position(t - step)).magnitude;
                ret += len;
            }
            return ret;
        }

        /// <summary>
        /// Accurately travels a certain distance on <paramref name="beizer"/> starting from <code>bezier.a</code>
        /// if <paramref name="distance"/> is larger than bezier length then method returns 1.
        /// </summary>
        /// <param name="distance">must be positive.</param>
        /// <param name="step">precision of calculation</param>
        /// <returns>bezier offset that is <paramref name="distance"/> away from start point</returns>
        public static float ArcTravel(this Bezier3 beizer, float distance, float step = 0.1f)
        {
            float accDistance = 0;
            float t;
            for (t = step; ; t += step) {
                if (t > 1f) t = 1f;
                float len = (beizer.Position(t) - beizer.Position(t - step)).magnitude;
                accDistance += len;
                if (accDistance >= distance) {
                    // travel backward to correct position.
                    t = beizer.Travel(t, distance - accDistance);
                    return t;
                }
                if (t >= 1f)
                    return 1;
            }
        }
    }
}
