using ColossalFramework.Plugins;
using System.Collections.Generic;
using static ColossalFramework.Plugins.PluginManager;
namespace CSkyL
{
    public class ModSupport
    {
        public static bool IsToggleItFoundandEnbled { get; private set; }
        public static bool IsTrainDisplayFoundandEnbled { get; private set; }
        public static ushort FollowVehicleID { get; set; }
        internal static void Initialize()
        {
            ModChecker.CheckToggleIt();
            ModChecker.CheckTrainDisplay();
        }
        private class ModChecker
        {
            internal static void CheckToggleIt()
            {
                try {
                    if (PluginManager.instance.isActiveAndEnabled) {
                        foreach (var info in infos) {
                            if ((info.publishedFileID.AsUInt64 == 1764637396 || info.publishedFileID.AsUInt64 == 2573796841) && info.isEnabled) {
                                Log.Msg("[ToggleItSupport] \"Toggle It!\" (or its CHS version) was found!");
                                IsToggleItFoundandEnbled = true;
                                return;
                            }
                        }
                        IsToggleItFoundandEnbled = false;
                    }
                }
                catch (System.Exception e) {
                    Log.Err($"[ToggleItSupport] Something went wrong when finding the mod: {e}");
                    IsToggleItFoundandEnbled = false;
                }
            }
            internal static void CheckTrainDisplay()
            {
                try {
                    if (PluginManager.instance.isActiveAndEnabled) {
                        foreach (var info in infos) {
                            if (info.publishedFileID.AsUInt64 == 3233229958 && info.isEnabled) {
                                Log.Msg("[TrainDisplayUpdateSupport] \"Train Display - Update\" was found!");
                                IsTrainDisplayFoundandEnbled = true;
                                FollowVehicleID = default;
                                return;
                            }
                        }
                        IsTrainDisplayFoundandEnbled = false;
                    }
                }
                catch (System.Exception e) {
                    Log.Err($"[TrainDisplayUpdateSupport] Something went wrong when finding the mod: {e}");
                    IsTrainDisplayFoundandEnbled = false;
                }
            }
            private static IEnumerable<PluginInfo> infos => PluginManager.instance.GetPluginsInfo();
        }
    }
}
