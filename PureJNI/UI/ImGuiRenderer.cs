using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends;
using Hexa.NET.ImGui.Backends.OpenGL2;
using Hexa.NET.ImGui.Backends.Win32;

namespace PureJNI.UI
{
    internal static unsafe class ImGuiRenderer
    {
        public static bool Initialized { get; private set; }

        public static void Init(IntPtr hwnd)
        {
            ImGuiContextPtr ctx = ImGui.CreateContext();
            ImGui.SetCurrentContext(ctx);
            ImGuiImpl.SetCurrentContext(ctx);

            ImGuiIOPtr io = ImGui.GetIO();
            io.IniFilename = null; 

            ImGui.StyleColorsDark();

            ImGuiImplWin32.InitForOpenGL((void*)hwnd);
            ImGuiImplOpenGL2.Init();
            Initialized = true;
        }

        public static void Render(Action draw)
        {
            ImGuiImplOpenGL2.NewFrame();
            ImGuiImplWin32.NewFrame();
            ImGui.NewFrame();

            draw();

            ImGui.Render();
            ImGuiImplOpenGL2.RenderDrawData(ImGui.GetDrawData());
        }
    }
}
