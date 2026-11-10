namespace PureJNI.Modules.Settings
{
    // Settings are written by the UI (render thread) and read by the module worker thread.
    // Each value is independent and atomic on x64, so volatile access is enough.
    public abstract class Setting
    {
        public string Name { get; }
        protected Setting(string name) => Name = name;
    }
}
