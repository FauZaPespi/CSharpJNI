using PureJNI.Game;
using PureJNI.Modules.Settings;

namespace PureJNI.Modules
{
    public abstract class Module
    {
        private volatile bool _enabled;
        private bool _wasEnabled; 
        private volatile string _display = "";

        public string Name { get; }
        public Category Category { get; }
        public List<Setting> Settings { get; } = new();

        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        public string Display => _display;

        public virtual bool AlwaysShowDisplay => false;

        protected Module(string name, Category category, bool enabled = false)
        {
            Name = name;
            Category = category;
            _enabled = enabled;
        }

        protected T Add<T>(T setting) where T : Setting
        {
            Settings.Add(setting);
            return setting;
        }

        protected void SetDisplay(string value) => _display = value;

        protected virtual void OnEnable(Minecraft mc) { }
        protected virtual void OnDisable(Minecraft mc) { }
        protected virtual void OnTick(Minecraft mc) { }

        internal void Update(Minecraft mc)
        {
            bool enabled = _enabled;
            if (enabled != _wasEnabled)
            {
                _wasEnabled = enabled;
                if (enabled) OnEnable(mc);
                else OnDisable(mc);
            }
            if (enabled) OnTick(mc);
        }
    }
}
