namespace CSkyL
{
    using ICities;
    using System.Reflection;

    public abstract class Mod<ConfigType, OptionsType> : LoadingExtensionBase, IUserMod
        where ConfigType : Config.Base where OptionsType : UI.OptionsBase
    {
        public string Name => $"{FullName} v{Version}";

        public const string CskyLHarmonyID = "Will258012.CSkyL";
        public const string FPSCamHarmonyID = "Will258012.FPSCamera.Continued";
        public abstract string FullName { get; }
        public abstract string ShortName { get; }
        public abstract string Description { get; }
        public abstract string Version { get; }

        public void OnEnabled()
        {
            var _FPSCamAssembly = FPSCamAssembly;
            var _CSkyLAssembly = CSkyLAssembly;

            Log.Logger = new FileLog(ShortName);
            Log.Msg($"Mod: {ShortName} enabled - v" + _FPSCamAssembly.GetName().Version);

            try {
                Harmony.Patcher.PatchOnReady(_CSkyLAssembly, CskyLHarmonyID);
                Harmony.Patcher.PatchOnReady(_FPSCamAssembly, FPSCamHarmonyID);
            }
            catch (System.IO.FileNotFoundException e) {
                Log.Err("Assembly of Harmony is missing: " + e.Message);
            }

            _PostEnable();
        }
        protected abstract void _PostEnable();
        public void OnDisabled()
        {
            _PreDisable();

            try {
                Harmony.Patcher.TryUnpatch(CskyLHarmonyID);
                Harmony.Patcher.TryUnpatch(FPSCamHarmonyID);

            }
            catch (System.IO.FileNotFoundException e) {
                Log.Err("Assembly of Harmony is missing: " + e.Message);
            }

            Log.Msg("Mod disabled.");
        }
        protected abstract void _PreDisable();

        public override void OnLevelLoaded(LoadMode mode)
        {
            Log.Msg("Mod: level loaded in: " + mode.ToString());
            ModSupport.Initialize();
            try {
                var _CSkyLAssembly = CSkyLAssembly;
                var _FPSCamAssembly = FPSCamAssembly;
                if (!Harmony.Patcher.HasPatched(CskyLHarmonyID))
                    Harmony.Patcher.PatchOnReady(_CSkyLAssembly, CskyLHarmonyID);
                if (!Harmony.Patcher.HasPatched(FPSCamHarmonyID))
                    Harmony.Patcher.PatchOnReady(_FPSCamAssembly, FPSCamHarmonyID);
            }
            catch (System.IO.FileNotFoundException e) {
                Log.Err("Assembly of Harmony is missing: " + e.Message);
            }

            _PostLoad();
        }
        protected abstract void _PostLoad();
        public override void OnLevelUnloading()
        {
            _PreUnload();
            Log.Msg("Mod: level unloaded");
        }
        protected abstract void _PreUnload();

        public void OnSettingsUI(UIHelperBase helper)
        {
            if (!IsConfigLoadedOnGameStart) {
                IsConfigLoadedOnGameStart = true;
                LoadConfig();
            }
            var comp = (helper as UIHelper)?.self as ColossalFramework.UI.UIComponent;
            var menu = comp.gameObject.AddComponent<OptionsType>();
            menu.name = ShortName;
            menu.Generate(UI.Helper.GetElement(helper));
            Log.Msg("Settings UI - generated");
        }

        public abstract void LoadConfig();
        public abstract void ResetConfig();
        protected abstract Assembly FPSCamAssembly { get; }
        private Assembly CSkyLAssembly => Assembly.GetExecutingAssembly();
        private bool IsConfigLoadedOnGameStart = false;
    }
}
