using Hexa.NET.ImGui;
using PureJNI.Core;
using PureJNI.Game;
using PureJNI.Hook;
using PureJNI.Jvm;
using PureJNI.Modules;
using PureJNI.Modules.Settings;
using PureJNI.Native;

namespace PureJNI.UI
{
    internal static unsafe class Menu
    {
        private static volatile bool _visible;
        private static bool _appliedVisible;
        private static bool _mouseWasGrabbed;
        private static readonly Action DrawAction = Draw;
        private static readonly Category[] Categories = Enum.GetValues<Category>();

        public static bool Visible => _visible;
        public static bool ImGuiReady => ImGuiRenderer.Initialized;

        public static void Toggle() => _visible = !_visible;

        public static void OnFrame(IntPtr hdc)
        {
            if (!ImGuiRenderer.Initialized)
            {
                IntPtr hwnd = User32.WindowFromDC(hdc);
                if (hwnd == IntPtr.Zero) return;
                ImGuiRenderer.Init(hwnd);
                WndProcHook.Install(hwnd);
                Log.Info("ImGui ready");
            }

            ApplyMouseGrab();

            if (_visible)
                ImGuiRenderer.Render(DrawAction);
        }

        private static void ApplyMouseGrab()
        {
            bool visible = _visible;
            if (visible == _appliedVisible) return;
            _appliedVisible = visible;

            JniNative.JNIEnv* env = JavaVm.GetEnv(); 
            if (env == null || !GameClasses.Ready) return;

            if (visible)
            {
                _mouseWasGrabbed = LwjglMouse.IsGrabbed(env);
                if (_mouseWasGrabbed) LwjglMouse.SetGrabbed(env, false);
            }
            else if (_mouseWasGrabbed)
            {
                LwjglMouse.SetGrabbed(env, true);
            }
            Jni.ClearException(env);
        }

        private static void Draw()
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(360, 300), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("PureJNI"))
            {
                if (ImGui.BeginTabBar("categories"))
                {
                    foreach (var category in Categories)
                    {
                        if (!ImGui.BeginTabItem(category.ToString())) continue;
                        foreach (var module in ModuleManager.All)
                            if (module.Category == category)
                                DrawModule(module);
                        ImGui.EndTabItem();
                    }
                    ImGui.EndTabBar();
                }
            }
            ImGui.End();
        }

        private static void DrawModule(Module module)
        {
            bool enabled = module.Enabled;
            if (ImGui.Checkbox(module.Name, ref enabled))
                module.Enabled = enabled;

            string display = module.Display;
            if (display.Length > 0 && (enabled || module.AlwaysShowDisplay))
            {
                ImGui.SameLine();
                ImGui.TextDisabled(display);
            }

            if (!enabled || module.Settings.Count == 0)
                return;

            ImGui.Indent();
            foreach (var setting in module.Settings)
            {
                string label = setting.Name + "##" + module.Name;
                switch (setting)
                {
                    case SliderSetting slider:
                    {
                        float v = slider.Value;
                        if (ImGui.SliderFloat(label, ref v, slider.Min, slider.Max, slider.Format))
                            slider.Value = v;
                        break;
                    }
                    case BoolSetting toggle:
                    {
                        bool v = toggle.Value;
                        if (ImGui.Checkbox(label, ref v))
                            toggle.Value = v;
                        break;
                    }
                }
            }
            ImGui.Unindent();
        }
    }
}
