namespace PureJNI.Modules.Settings
{
    public sealed class BoolSetting : Setting
    {
        private volatile bool _value;

        public BoolSetting(string name, bool value = false) : base(name) => _value = value;

        public bool Value
        {
            get => _value;
            set => _value = value;
        }
    }
}
