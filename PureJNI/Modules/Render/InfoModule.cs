using PureJNI.Game;

namespace PureJNI.Modules.Render
{
    public sealed class InfoModule : Module
    {
        public InfoModule() : base("Info", Category.Render, enabled: true) { }

        public override bool AlwaysShowDisplay => true;

        protected override void OnTick(Minecraft mc)
        {
            int fps = mc.Fps;
            var player = mc.Player;
            SetDisplay(player.IsValid
                ? $"FPS {fps} | {player.X:F1} {player.Y:F1} {player.Z:F1}"
                : $"FPS {fps} | not in world");
        }
    }
}
