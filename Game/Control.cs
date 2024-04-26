namespace CSkyL.Game
{
    using UnityEngine;

    public static class Control
    {
        public static float MouseMoveHori => Input.GetAxis("Mouse X");  // +/-: right/left
        public static float MouseMoveVert => Input.GetAxis("Mouse Y");  // +/-: up/down
        public static float MouseScroll => Input.GetAxisRaw("Mouse ScrollWheel");  // +/i: up/down

        public static bool MouseTriggered(MouseButton btn)
            => Input.GetMouseButtonDown((int) btn);
        public static bool MousePressed(MouseButton btn)
            => Input.GetMouseButton((int) btn);
        public static bool KeyTriggered(KeyCode key) => Input.GetKeyDown(key);
        public static bool KeyPressed(KeyCode key) => Input.GetKey(key);

        public enum MouseButton : int { Primary = 0, Secondary = 1, Middle = 2 }

        public static void ShowCursor(bool visibility = true)
            => Cursor.visible = visibility;
        public static void HideCursor() => ShowCursor(false);

        public static class UIManager
        {
            private class UIState
            {
                internal bool NotificationsVisible { get; set; }
                internal bool BordersVisible { get; set; }
                internal bool NamesVisible { get; set; }
                internal bool MarkersVisible { get; set; }
                internal bool TutorialDisabled { get; set; }
                internal bool DisasterMarkersVisible { get; set; }
                internal bool RoadNamesVisible { get; set; }
            }

            private static UIState savedState;

            public static void ShowUI(bool IsShow = true)
            {
                if (!ModSupport.ToggleIt.IsToggleItFoundandEnbled) {
                    SetUIVisibility(IsShow);
                }
                else {
                    if (IsShow) {
                        RestoreState();
                        ColossalFramework.UI.UIView.Show(true);
                    }
                    else {
                        SaveState();
                        SetUIVisibility(false);
                    }
                }
            }

            private static void SaveState()
            {
                savedState = new UIState
                {
                    NotificationsVisible = NotificationManager.instance.NotificationsVisible,
                    BordersVisible = GameAreaManager.instance.BordersVisible,
                    NamesVisible = DistrictManager.instance.NamesVisible,
                    MarkersVisible = PropManager.instance.MarkersVisible,
                    TutorialDisabled = GuideManager.instance.TutorialDisabled,
                    DisasterMarkersVisible = DisasterManager.instance.MarkersVisible,
                    RoadNamesVisible = NetManager.instance.RoadNamesVisible
                };
            }

            private static void RestoreState()
            {
                if (savedState != null) {
                    NotificationManager.instance.NotificationsVisible = savedState.NotificationsVisible;
                    GameAreaManager.instance.BordersVisible = savedState.BordersVisible;
                    DistrictManager.instance.NamesVisible = savedState.NamesVisible;
                    PropManager.instance.MarkersVisible = savedState.MarkersVisible;
                    GuideManager.instance.TutorialDisabled = savedState.TutorialDisabled;
                    DisasterManager.instance.MarkersVisible = savedState.DisasterMarkersVisible;
                    NetManager.instance.RoadNamesVisible = savedState.RoadNamesVisible;
                }
            }

            private static void SetUIVisibility(bool isVisible)
            {
                ColossalFramework.UI.UIView.Show(isVisible);
                NotificationManager.instance.NotificationsVisible = isVisible;
                GameAreaManager.instance.BordersVisible = isVisible;
                DistrictManager.instance.NamesVisible = isVisible;
                PropManager.instance.MarkersVisible = isVisible;
                GuideManager.instance.TutorialDisabled = isVisible;
                DisasterManager.instance.MarkersVisible = isVisible;
                NetManager.instance.RoadNamesVisible = isVisible;
            }
        }
    }
}
