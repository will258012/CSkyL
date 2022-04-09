namespace CSkyL.UI
{
    using System.Collections.Generic;

    public abstract class OptionsBase : Game.Behavior
    {
        public abstract void Generate(GameElement settingPanel);

        protected override void _Init() { }
        protected override void _UpdateLate()
        {
            foreach (var setting in _settings) setting.UpdateUI();
        }

        protected readonly List<ISetting> _settings = new List<ISetting>();
    }
}
