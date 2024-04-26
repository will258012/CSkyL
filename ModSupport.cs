using ColossalFramework.Plugins;
namespace CSkyL.ModSupport
{
    class ToggleIt
    {
        public static bool IsToggleItFoundandEnbled { get; private set; }
        public static void FindToggleIt()
        {
            try {
                var infos = PluginManager.instance.GetPluginsInfo();
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
    }
}
