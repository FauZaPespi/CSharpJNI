namespace PureJNI.Modules.Settings
{
    public sealed class SliderSetting : Setting
    {
        private float _value;

        public float Min { get; }
        public float Max { get; }
        public float Default { get; }
        public string Format { get; }

        public SliderSetting(string name, float min, float max, float value, string format = "%.2f") : base(name)
        {
            Min = min;
            Max = max;
            Default = value;
            Format = format;
            _value = value;
        }

        public float Value
        {
            get => Volatile.Read(ref _value);
            set => Volatile.Write(ref _value, Math.Clamp(value, Min, Max));
        }
    }
}
