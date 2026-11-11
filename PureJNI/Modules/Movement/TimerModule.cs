using PureJNI.Game;
using PureJNI.Modules.Settings;

namespace PureJNI.Modules.Movement
{
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
