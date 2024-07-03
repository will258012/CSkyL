namespace CSkyL.Harmony
{
    using CitiesHarmony.API;
    using HarmonyLib;
    using System.Collections.Generic;

    public static class Patcher
    {
        public static void PatchOnReady(System.Reflection.Assembly assembly, string HarmonyID)
        {
            try {
                HarmonyHelper.EnsureHarmonyInstalled();
                HarmonyHelper.DoOnHarmonyReady(() => Patch(assembly, HarmonyID));
            }
            catch (System.Exception e) {
                Log.Err("Harmony: " + e.ToString());
            }
        }
        public static void TryUnpatch(string HarmonyID)
        {
            try {
                if (HarmonyHelper.IsHarmonyInstalled) Unpatch(HarmonyID);
            }
            catch (System.Exception e) {
                Log.Err("Harmony: " + e.ToString());
            }
        }

        public static void Patch(System.Reflection.Assembly assembly, string HarmonyID)
        {
            if (_patchedAssemblies.Contains(HarmonyID)) {
                Log.Warn($"Harmony: <{HarmonyID}> already patched");
                return;
            }

            Log.Msg($"Harmony: patching <{HarmonyID}>");
            try {
                var harmony = new Harmony(HarmonyID);
                harmony.PatchAll(assembly);
                _patchedAssemblies.Add(HarmonyID);
                Log.Msg(" -- patched");
            }
            catch (System.Exception e) {
                Log.Err(" -- patching fails: " + e.ToString());
            }
        }

        public static void Unpatch(string HarmonyID)
        {
            if (!_patchedAssemblies.Remove(HarmonyID)) {
                Log.Warn($"Harmony: <{HarmonyID}> never been patched");
                return;
            }

            Log.Msg($"Harmony: unpatching <{HarmonyID}>");
            try {
                var harmony = new Harmony(HarmonyID);
                harmony.UnpatchAll(HarmonyID);
                Log.Msg(" -- unpatched");
            }
            catch (System.Exception e) {
                Log.Err(" -- unpatching fails: " + e.ToString());
            }
        }

        public static bool HasPatched(string HarmonyID)
            => _patchedAssemblies.Contains(HarmonyID);

        private static readonly List<string> _patchedAssemblies = new List<string>();
    }
}
