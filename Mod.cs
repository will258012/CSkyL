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

            LoadConfig();

            Harmony.Patcher.PatchOnReady(assembly);

            _PostEnable();
        }
        protected abstract void _PostEnable();
        public void OnDisabled()
        {
            _PreDisable();
            Harmony.Patcher.TryUnpatch(_Assembly);

            Log.Msg("Mod disabled.");
        }
        protected abstract void _PreDisable();

        public override void OnLevelLoaded(LoadMode mode)
        {
            Log.Msg("Mod: level loaded in: " + mode.ToString());

            var assembly = _Assembly;
            if (!Harmony.Patcher.HasPatched(assembly))
                Harmony.Patcher.PatchOnReady(assembly);

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
            var comp = (helper as UIHelper)?.self as ColossalFramework.UI.UIComponent;
            var menu = comp.gameObject.AddComponent<OptionsType>();
            menu.name = ShortName;
            menu.Generate(UI.Helper.GetElement(helper));
            Log.Msg("Settings UI - generated");
        }

        public abstract void LoadConfig();
        public abstract void ResetConfig();

        protected abstract Assembly _Assembly { get; }
    }
}
