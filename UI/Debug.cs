#if DEBUG
namespace CSkyL.UI
{
    using System.Collections.Generic;
    using UnityEngine;

    public class Debug : Game.Behavior
    {
        public static Debug Panel {
            get {
                if (_panel == null) _panel = Game.CamController.I.AddComponent<Debug>();
                return _panel;
            }
        }

        protected override void _Init() => enabled = true;

        public static void DestroyPanel()
        { if (_panel != null) Destroy(_panel); }

        public void AppendMessage(string msg)
        {
            if (_msgCount < msgLimit) ++_msgCount;
            else _message = _message.Substring(_message.IndexOf('\n') + 1);
            _message += msg + "\n";
            enabled = true;
        }

        public void RegisterAction(string actionName, System.Action action)
        { _nameList.Add(actionName); _actionList.Add(action); }

        private void OnGUI()
        {
            var boxWidth = Mathf.Min(Screen.width / 5f, 400f);
            var boxHeight = Mathf.Min(Screen.height / 2f, 1000f);

            GUI.Box(new Rect(0f, boxHeight / 2f, boxWidth, boxHeight), "");
            if (GUI.Button(new Rect(boxWidth, boxHeight / 2f, 20f, 20f), "X"))
                enabled = false;

            var style = new GUIStyle
            {
                fontSize = 12,
                normal = { textColor = Color.white }
            };

            const float margin = 5f;
            float curY = margin + boxHeight / 2f, curX = margin;
            boxWidth -= 2f * margin;
            const int btnPerLine = 3;
            float btnW = (boxWidth - margin * 2f) / btnPerLine, btnH = boxHeight / 16f;
            for (int i = 0; i < _nameList.Count; ++i) {
                if (GUI.Button(new Rect(curX, curY, btnW, btnH), _nameList[i]))
                    _actionList[i].Invoke();

                if (i % btnPerLine == btnPerLine - 1) { curX = margin; curY += btnH; }
                else curX += btnW;
            }
            if (curX > margin) curY += btnH;

            GUI.Label(new Rect(margin, curY,
                               boxWidth, boxHeight * (1 + 1 / 2f) - curY),
                      _message);
        }

        private const uint msgLimit = 20u;
        private uint _msgCount = 0u;
        private string _message = "";

        private readonly List<string> _nameList = new List<string>();
        private readonly List<System.Action> _actionList = new List<System.Action>();

        private static Debug _panel;
    }
}
#endif
