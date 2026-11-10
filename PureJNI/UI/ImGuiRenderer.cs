using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends;
using Hexa.NET.ImGui.Backends.OpenGL2;
using Hexa.NET.ImGui.Backends.Win32;

namespace PureJNI.UI
{
    // ImGui context + Win32 (input) and OpenGL2 (render) backends. Render thread only.
    // OpenGL2 matches Minecraft 1.8's legacy GL and saves/restores the GL state it touches.
    internal static unsafe class ImGuiRenderer
    {
        public static bool Initialized { get; private set; }

        public static void Init(IntPtr hwnd)
        {
            ImGuiContextPtr ctx = ImGui.CreateContext();
            ImGui.SetCurrentContext(ctx);
            // The backends DLL has its own ImGui globals: give it the same context.
            ImGuiImpl.SetCurrentContext(ctx);

            ImGuiIOPtr io = ImGui.GetIO();
            io.IniFilename = null; // no imgui.ini written in the Minecraft folder

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
