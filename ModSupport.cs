using ColossalFramework.Plugins;
namespace CSkyL
{
    public class ModSupport
    {
        public static bool IsToggleItFoundandEnabled { get; private set; }
        public static bool IsTrainDisplayFoundandEnabled { get; private set; }
        public static ushort FollowVehicleID { get; set; }
        public static bool IsUUIFoundandEnabled { get; private set; }

        internal static void Initialize()
        {
            try {
                var infos = PluginManager.instance.GetPluginsInfo();
                foreach (var info in infos) {
                    if ((info.publishedFileID.AsUInt64 == 1764637396 || info.publishedFileID.AsUInt64 == 2573796841) && info.isEnabled) {
                        Log.Msg("ModSupport: \"Toggle It!\" (or its CHS version) was found!");
                        IsToggleItFoundandEnabled = true;
                        continue;
                    }
                    if ((info.publishedFileID.AsUInt64 == 3233229958 || info.name.Contains("TrainDisplay")) && info.isEnabled) {
                        Log.Msg("ModSupport: \"Train Display - Update\" was found!");
                        IsTrainDisplayFoundandEnabled = true;
                        continue;
                    }
                    if ((info.publishedFileID.AsUInt64 == 2966990700 || info.publishedFileID.AsUInt64 == 2255219025) && info.isEnabled) {
                        Log.Msg("ModSupport: \"UnifiedUI \" was found!");
                        IsUUIFoundandEnabled = true;
                        continue;
                    }
                }
            }

            catch (System.Exception e) {
                Log.Err($"ModSupport: Falled to finding the mod: {e}");
            }
        }
    }
}
