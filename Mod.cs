namespace CSkyL
{
    using ICities;
    using System.Reflection;

    public abstract class Mod<ConfigType, OptionsType> : LoadingExtensionBase, IUserMod
        where ConfigType : Config.Base where OptionsType : UI.OptionsBase
    {
        public string Name => $"{FullName} v{Version}";

        public abstract string FullName { get; }
        public abstract string ShortName { get; }
        public abstract string Description { get; }
        public abstract string Version { get; }

        public void OnEnabled()
        {
            var assembly = _Assembly;

            Log.Logger = new FileLog(ShortName);
            Log.Msg($"Mod: {ShortName} enabled - v" + assembly.GetName().Version);

            try { Harmony.Patcher.PatchOnReady(assembly); }
            catch (System.IO.FileNotFoundException e) {
                Log.Err("Assembly of Harmony is missing: " + e.Message);
            }
            ModSupport.Initialize();
            _PostEnable();
        }
        protected abstract void _PostEnable();
        public void OnDisabled()
        {
            _PreDisable();

            try { Harmony.Patcher.TryUnpatch(_Assembly); }
            catch (System.IO.FileNotFoundException e) {
                Log.Err("Assembly of Harmony is missing: " + e.Message);
            }
            
            Log.Msg("Mod disabled.");
        }
        protected abstract void _PreDisable();

        public override void OnLevelLoaded(LoadMode mode)
        {
            Log.Msg("Mod: level loaded in: " + mode.ToString());

            try {
                var assembly = _Assembly;
                if (!Harmony.Patcher.HasPatched(assembly))
                    Harmony.Patcher.PatchOnReady(assembly);
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

        protected abstract Assembly _Assembly { get; }
        private bool IsConfigLoadedOnGameStart = false;
    }
}
