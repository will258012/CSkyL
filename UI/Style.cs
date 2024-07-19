namespace CSkyL.UI
{
    using UnityEngine;

    public struct Style
    {
        public static Style Current = default;

        public string namePrefix;
        public CSkyLColor color {
            get => CSkyLColor._From32(_color);
            set => _color = value.ToColor32();
        }
        public CSkyLColor textColor {
            get => CSkyLColor._From32(_textColor);
            set => _textColor = value.ToColor32();
        }
        public CSkyLColor bgColor {
            get => CSkyLColor._From32(_bgColor);
            set => _bgColor = value.ToColor32();
        }
        public CSkyLColor colorDisabled {
            get => CSkyLColor._From32(_colorDisabled);
            set => _colorDisabled = value.ToColor32();
        }
        public CSkyLColor textColorDisabled {
            get => CSkyLColor._From32(_textColorDisabled);
            set => _textColorDisabled = value.ToColor32();
        }

        public float scale { get => _scale; set => _scale = value; }
        public int padding { get => _padding; set => _padding = value; }

        // internal
        internal Color32 _color;
        internal Color32 _textColor;
        internal Color32 _bgColor;
        internal Color32 _colorDisabled;
        internal Color32 _textColorDisabled;
        internal float _scale;
        internal int _padding;

        public struct CSkyLColor
        {
            public byte r, g, b, a;

            public static CSkyLColor RGBA(byte r, byte g, byte b, byte a) => new CSkyLColor(r, g, b, a);
            public static CSkyLColor RGB(byte r, byte g, byte b) => new CSkyLColor(r, g, b);

            public static CSkyLColor White => RGB(255, 255, 255);
            public static CSkyLColor None => RGBA(0, 0, 0, 0);

            private CSkyLColor(byte r, byte g, byte b, byte a = 255)
            { this.r = r; this.g = g; this.b = b; this.a = a; }
            public static CSkyLColor _From32(Color32 c) => CSkyLColor.RGBA(c.r, c.g, c.b, c.a);
            public Color32 ToColor32() => new Color32(r, g, b, a);
        }
    }
}
