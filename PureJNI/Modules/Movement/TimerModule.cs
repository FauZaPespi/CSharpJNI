using PureJNI.Game;
using PureJNI.Modules.Settings;

namespace PureJNI.Modules.Movement
{
    // Sets the game timer speed to the slider value while enabled, back to 1 when disabled.
    public sealed class TimerModule : Module
    {
        private readonly SliderSetting _speed;
        private float _applied = float.NaN;

        public TimerModule() : base("Timer", Category.Movement)
        {
            _speed = Add(new SliderSetting("Speed", 0.1f, 5f, 1f));
        }

        protected override void OnTick(Minecraft mc)
        {
            float speed = _speed.Value;
            // Only write when the value changed. NaN forces a write right after enable.
            if (speed != _applied)
            {
                var timer = mc.Timer;
                if (!timer.IsValid) return;
                timer.Speed = speed;
                _applied = speed;
            }
            SetDisplay($"x{speed:F2}");
        }

        protected override void OnDisable(Minecraft mc)
        {
            var timer = mc.Timer;
            if (timer.IsValid) timer.Speed = 1f;
            _applied = float.NaN;
            SetDisplay("");
        }
    }
}
