﻿using CSkyL.Config;
using Ctransl = CSkyL.Translation.Translations;
namespace CSkyL.UI
{
    public interface ISetting
    {
        void UpdateUI();
    }

    public class SettingProperties : Properties
    {
        public Base configObj;
        public IConfigData config;
        public virtual void SetUpFromConfig()
        {
            if (string.IsNullOrEmpty(name)) name = config.Name;
            if (string.IsNullOrEmpty(text)) text = config.Description;
            if (string.IsNullOrEmpty(tooltip)) tooltip = config.Detail;
            if (config is CfFloat fConfig) {
                valueMin = fConfig.Min; valueMax = fConfig.Max;
            }
        }

        public SettingProperties Swap(IConfigData newConfig)
        {
            var newProps = MemberwiseClone() as SettingProperties;
            newProps.config = newConfig;
            newProps.name = newProps.text = newProps.tooltip = "";
            return newProps;
        }
    }

    public class ToggleSetting : CheckBox, ISetting
    {
        public ToggleSetting() { }
        public void UpdateUI() { if (IsChecked != _config) IsChecked = _config; }

        protected override Element _Create(Element parent, Properties props)
        {
            var configProps = props as SettingProperties;
            configProps.SetUpFromConfig();
            var box = base._Create(parent, configProps) as CheckBox;
            var config = configProps.config as ConfigData<bool>;
            box.IsChecked = config;
            box.SetTriggerAction((value)
                => { config.Assign(value); configProps.configObj.Save(); });
            return new ToggleSetting(box, config);
        }

        private ToggleSetting(CheckBox box, ConfigData<bool> config) : base(box)
        { _config = config; }

        private readonly ConfigData<bool> _config;
    }

    public class SliderSetting : Slider, ISetting
    {
        public SliderSetting() { }
        public void UpdateUI() { if (Value != _config) Value = _config; }

        protected override Element _Create(Element parent, Properties props)
        {
            var configProps = props as SettingProperties;
            configProps.SetUpFromConfig();
            var slider = base._Create(parent, configProps) as Slider;
            var config = configProps.config as CfFloat;
            slider.Value = config;
            slider.SetTriggerAction((value)
                => { config.Assign(value); configProps.configObj.Save(); });
            return new SliderSetting(slider, config);
        }

        private SliderSetting(Slider slider, CfFloat config) : base(slider)
        { _config = config; }

        private readonly CfFloat _config;
    }

    public class OffsetSetting : Panel, ISetting
    {
        public OffsetSetting() { }
        public void UpdateUI() { _forward.UpdateUI(); _up.UpdateUI(); _right.UpdateUI(); }

        protected override Element _Create(Element parent, Properties props)
        {
            var configProps = props as SettingProperties;
            configProps.SetUpFromConfig();

            var panel = base._Create(parent, configProps) as Panel;

            var label = panel.Add<Label>(new Properties
            { width = configProps.width, text = configProps.text, tooltip = configProps.tooltip });

            var config = configProps.config as CfOffset;
            var padding = Style.Current.padding / 2f;
            var sliderProp = new SettingProperties
            {
                x = padding, y = label.bottom + padding,
                width = panel.width - padding, wideCondition = true,
                text = Ctransl.Translate("SETTINGS_KEYMOVEFORWARD"),
                config = config.forward, configObj = configProps.configObj,
                stepSize = .05f, valueFormat = "F2"
            };
            sliderProp.SetUpFromConfig();
            var forward = panel.Add<SliderSetting>(sliderProp);

            sliderProp.y += forward.height + padding;
            sliderProp.text = Ctransl.Translate("SETTINGS_KEYMOVEUP"); sliderProp.config = config.up;
            sliderProp.SetUpFromConfig();
            var up = panel.Add<SliderSetting>(sliderProp);

            sliderProp.y += up.height + padding;
            sliderProp.text = Ctransl.Translate("SETTINGS_KEYMOVERIFHT"); sliderProp.config = config.right;
            sliderProp.SetUpFromConfig();
            var right = panel.Add<SliderSetting>(sliderProp);

            sliderProp.y += right.height;
            panel.height = sliderProp.y + 10f;
            return new OffsetSetting(panel, forward, up, right);
        }

        private OffsetSetting(Panel panel, SliderSetting forward, SliderSetting up,
            SliderSetting right)
            : base(panel)
        { _forward = forward; _up = up; _right = right; }

        private readonly SliderSetting _forward, _up, _right;
    }

    public class ChoiceSetting<EnumType> : DropDown<EnumType>, ISetting
                        where EnumType : struct, System.IConvertible, System.IComparable
    {
        public ChoiceSetting() { }
        public void UpdateUI() { if (!Choice.Equals(_config)) Choice = _config; }

        protected override Element _Create(Element parent, Properties props)
        {
            var configProps = props as SettingProperties;
            configProps.SetUpFromConfig();
            var dropdown = base._Create(parent, configProps) as DropDown<EnumType>;
            var config = configProps.config as ConfigData<EnumType>;
            dropdown.Choice = config;
            dropdown.SetTriggerAction((value)
                => { config.Assign(value); configProps.configObj.Save(); });
            return new ChoiceSetting<EnumType>(dropdown, config);
        }

        private ChoiceSetting(DropDown<EnumType> dropdown, ConfigData<EnumType> config)
            : base(dropdown) { _config = config; }

        private readonly ConfigData<EnumType> _config;
    }
    public class ChoiceSettingv2 : DropDownv2, ISetting
    {
        public ChoiceSettingv2()
        {
        }
        public void UpdateUI() { if (!Choice.Equals(_config)) Choice = _config; }
        protected override Element _Create(Element parent, Properties props)
        {
            var configProps = props as SettingProperties;
            configProps.SetUpFromConfig();
            var dropdown = base._Create(parent, configProps) as DropDownv2;
            var config = configProps.config as ConfigData<int>;
            dropdown.Choice = config;
            dropdown.SetTriggerAction((value)
                => { config.Assign(value); configProps.configObj.Save(); });
            return new ChoiceSettingv2(dropdown, config);
        }

        private ChoiceSettingv2(DropDownv2 dropdown, ConfigData<int> config)
            : base(dropdown) { _config = config; }

        private readonly ConfigData<int> _config;
    }
    public class LangChoiceSetting : DropDownv2, ISetting
    {
        public LangChoiceSetting()
        {
        }
        public void UpdateUI()
        {
            if (Ctransl.CurrentLanguage != _config) Ctransl.CurrentLanguage = _config;
            if (!Choice.Equals(Ctransl.Index)) Choice = Ctransl.Index;
        }
        protected override Element _Create(Element parent, Properties props)
        {
            var configProps = props as SettingProperties;
            configProps.SetUpFromConfig();
            var dropdown = base._Create(parent, configProps) as DropDownv2;
            var config = configProps.config as ConfigData<string>;
            dropdown.Choice = Ctransl.Index;
            dropdown.SetTriggerAction((value)
                => {
                    if (Ctransl.Index != value) Ctransl.Index = value;
                    var langcode = Ctransl.CurrentLanguage;
                    config.Assign(langcode); configProps.configObj.Save();
                });
            return new LangChoiceSetting(dropdown, config);
        }

        private LangChoiceSetting(DropDownv2 dropdown, ConfigData<string> config)
            : base(dropdown) { _config = config; }

        private readonly ConfigData<string> _config;
    }
    public class KeyOnlyMapSetting : KeyOnlyInput, ISetting
    {
        public KeyOnlyMapSetting() { }
        public void UpdateUI() { if (Key != _config) Key = _config; }

        protected override Element _Create(Element parent, Properties props)
        {
            var configProps = props as SettingProperties;
            configProps.SetUpFromConfig();
            var input = base._Create(parent, configProps) as KeyOnlyInput;
            var config = configProps.config as ConfigData<UnityEngine.KeyCode>;
            input.Key = config;
            input.SetTriggerAction((key) => {
                if (key == UnityEngine.KeyCode.Escape) return config;
                config.Assign(key); configProps.configObj.Save();
                return key;
            });
            return new KeyOnlyMapSetting(input, config);
        }

        private KeyOnlyMapSetting(KeyOnlyInput input, ConfigData<UnityEngine.KeyCode> config) : base(input)
        { _config = config; }

        private readonly ConfigData<UnityEngine.KeyCode> _config;
    }
    public class KeyMapSetting : KeyInput, ISetting
    {
        public KeyMapSetting() { }
        public void UpdateUI() { if (!Keys.Equals(_config.ToKeyCodeWithModifiers())) Keys = _config.ToKeyCodeWithModifiers(); }
        protected override Element _Create(Element parent, Properties props)
        {
            var configProps = props as SettingProperties;
            configProps.SetUpFromConfig();
            var input = base._Create(parent, configProps) as KeyInput;
            var config = configProps.config as CfKeyWithWithModifiers;
            input.Keys = config;
            input.SetTriggerAction((keys) => {
                config.Assign(keys);
                configProps.configObj.Save();
                return keys;
            });
            return new KeyMapSetting(input, config);
        }

        private KeyMapSetting(KeyInput input, CfKeyWithWithModifiers config) : base(input)
        { _config = config; }

        private readonly CfKeyWithWithModifiers _config;
    }
    public class UUIKeySetting : KeyInput, ISetting
    {
        public UUIKeySetting() { }
        public void UpdateUI() { if (!Keys.Equals(_config.ToKeyCodeWithModifiers())) Keys = _config.ToKeyCodeWithModifiers(); }
        protected override Element _Create(Element parent, Properties props)
        {
            var configProps = props as SettingProperties;
            configProps.SetUpFromConfig();
            var input = base._Create(parent, configProps) as KeyInput;
            var config = configProps.config as CfKeyWithWithModifiers;
            input.Keys = config;
            input.SetTriggerAction((keys) => {
                config.Assign(keys);
                configProps.configObj.Save();
                UpdateToggleKeyEvent?.Invoke();
                return keys;
            });
            return new UUIKeySetting(input, config);
        }

        private UUIKeySetting(KeyInput input, CfKeyWithWithModifiers config) : base(input)
        { _config = config; }

        private readonly CfKeyWithWithModifiers _config;
        public static event System.Action UpdateToggleKeyEvent;
    }
}
