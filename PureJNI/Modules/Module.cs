using PureJNI.Game;
using PureJNI.Modules.Settings;

namespace PureJNI.Modules
{
    // Base class of every module. Threading contract:
    //  - UI (render thread): reads Name/Settings/Display, writes Enabled and setting values. No JNI.
    //  - Worker (JVM-attached thread): calls Update(), which runs OnEnable/OnDisable/OnTick with JNI.
    public abstract class Module
    {
        private volatile bool _enabled;
        private bool _wasEnabled; // worker thread only
        private volatile string _display = "";

        public string Name { get; }
        public Category Category { get; }
        public List<Setting> Settings { get; } = new();

        // Toggled by the UI; the worker applies the change on its next tick.
        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        // Optional value shown next to the module name (FPS, position...). Empty = nothing shown.
        public string Display => _display;

        // Value modules are shown even when disabled, with their last value.
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

        // Worker thread only.
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
