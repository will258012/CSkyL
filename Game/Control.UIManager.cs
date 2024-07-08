using System.Collections;
using ToggleItManager = ToggleIt.Managers.ToggleManager;
using ColossalFramework;
namespace CSkyL.Game
{
    public static partial class Control
    {
        public static class UIManager
        {
            private class UIState
            {
                internal bool NotificationsVisible { get; set; }
                internal bool BordersVisible { get; set; }
                internal bool DirectNamesVisible { get; set; }
                internal bool RoadNamesVisible { get; set; }
                internal bool ContoursVisible { get; set; }
            }

            private static UIState savedState;

            public static IEnumerator ToggleUI(bool visibility)
            {
                if (ModSupport.IsToggleItFoundandEnabled) {
                    try {
                        SetUIVisibilityByToggleIt(visibility);
                    }
                    catch (System.Exception e) {
                        Log.Err($"ModSupport: Failed to toggle UI using \"Toggle It!\": {e}. Falling back to the vanilla way.");
                        SetUIVisibilityDirectly(visibility);
                    }
                }
                else {
                    SetUIVisibilityDirectly(visibility);
                }

                yield break;
            }

            private static void SaveState()
            {
                savedState = new UIState
                {
                    NotificationsVisible = ToggleItManager.Instance.GetById(1).On,
                    RoadNamesVisible = ToggleItManager.Instance.GetById(2).On,
                    BordersVisible = ToggleItManager.Instance.GetById(4).On,
                    ContoursVisible = ToggleItManager.Instance.GetById(5).On,
                    DirectNamesVisible = ToggleItManager.Instance.GetById(10).On,
                };
                Log.Msg($"ModSupport: Saved UI state from \"Toggle It!\":\n" +
                         $"  NotificationIcons = {savedState.NotificationsVisible}\n" +
                         $"  BorderLines = {savedState.BordersVisible}\n" +
                         $"  ContourLines = {savedState.ContoursVisible}\n" +
                         $"  RoadNames = {savedState.RoadNamesVisible}\n" +
                         $"  DistrictNames = {savedState.DirectNamesVisible}");
            }

            private static void RestoreState()
            {
                if (savedState != null) {
                    ToggleItManager.Instance.Apply(1, savedState.NotificationsVisible);
                    ToggleItManager.Instance.Apply(2, savedState.RoadNamesVisible);
                    ToggleItManager.Instance.Apply(4, savedState.BordersVisible);
                    ToggleItManager.Instance.Apply(5, savedState.ContoursVisible);
                    ToggleItManager.Instance.Apply(10, savedState.DirectNamesVisible);
                    Log.Msg("ModSupport: Restored saved UI state using \"Toggle It!\"");
                }
            }

            private static void SetUIVisibilityByToggleIt(bool visibility)
            {
                ColossalFramework.UI.UIView.Show(visibility);
                if (!visibility) {
                    SaveState();
                    ToggleItManager.Instance.Apply(1, false);
                    ToggleItManager.Instance.Apply(2, false);
                    ToggleItManager.Instance.Apply(4, false);
                    ToggleItManager.Instance.Apply(5, false);
                    ToggleItManager.Instance.Apply(10, false);
                    Log.Msg("ModSupport: Hid UI using \"Toggle It!\"");
                }
                else {
                    RestoreState();
                }
                Singleton<PropManager>.instance.MarkersVisible = visibility;
                Singleton<GuideManager>.instance.TutorialDisabled = !visibility;
                Singleton<DisasterManager>.instance.MarkersVisible = visibility;
            }

            private static void SetUIVisibilityDirectly(bool visibility)
            {
                ColossalFramework.UI.UIView.Show(visibility);
                Singleton<NotificationManager>.instance.NotificationsVisible = visibility;
                Singleton<GameAreaManager>.instance.BordersVisible = visibility;
                Singleton<DistrictManager>.instance.NamesVisible = visibility;
                Singleton<PropManager>.instance.MarkersVisible = visibility;
                Singleton<GuideManager>.instance.TutorialDisabled = !visibility;
                Singleton<DisasterManager>.instance.MarkersVisible = visibility;
                Singleton<NetManager>.instance.RoadNamesVisible = visibility;
            }
        }
    }
}

