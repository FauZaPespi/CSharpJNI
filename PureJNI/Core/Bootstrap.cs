using System.Runtime.InteropServices;
using HexaGen.Runtime;
using PureJNI.Game;
using PureJNI.Hook;
using PureJNI.Jvm;
using PureJNI.Modules;
using PureJNI.Native;

namespace PureJNI.Core
{
    // Initialization order, on the thread started by DllMain. This thread then becomes the module worker.
    internal static unsafe class Bootstrap
    {
        // Native DLLs expected next to PureJNI.dll.
        private const string MinHookDll = "minhook.x64.dll";
        private const string CImGuiDll = "cimgui.dll";
        private const string ImGuiImplDll = "ImGuiImpl.dll";

        [UnmanagedCallersOnly]
        public static uint ThreadMain(IntPtr parameter)
        {
            try
            {
                Run();
            }
            catch (Exception e)
            {
                Log.Error($"bootstrap failed: {e}");
            }
            return 0;
        }

        private static void Run()
        {
            Kernel32.AllocConsole();
            string dir = ModuleDirectory();
            Log.Info($"PureJNI loaded from {dir}");

            // 1. JVM + mappings
            if (!JavaVm.Find())
            {
                Log.Error("no JVM found");
                return;
            }
            JniNative.JNIEnv* env = JavaVm.Attach();
            if (env == null)
            {
                Log.Error("AttachCurrentThread failed");
                return;
            }
            if (!GameClasses.Init(env))
            {
                Log.Error("mappings failed, stopping");
                JavaVm.Detach();
                return;
            }
            Log.Info("JVM attached, mappings resolved");

            // 2. Native DLLs by absolute path: javaw.exe searches its own folder, not ours.
            //    Once loaded, LoadLibrary("cimgui.dll") by name (done by Hexa) returns the loaded module.
            LibraryLoader.CustomLoadFolders.Add(dir);
            MinHook.Load(Path.Combine(dir, MinHookDll));
            NativeLibrary.Load(Path.Combine(dir, CImGuiDll));
            NativeLibrary.Load(Path.Combine(dir, ImGuiImplDll));

            // 3. Render hook (menu)
            if (!SwapBuffersHook.Install())
                Log.Error("SwapBuffers hook failed, no menu");

            // 4. Module worker loop (blocks until ModuleManager.Stop())
            Log.Info("module worker started, press INSERT in game for the menu");
            ModuleManager.Run(env);

            JavaVm.Detach();
            Log.Info("module worker stopped");
        }

        private static string ModuleDirectory()
        {
            char* buffer = stackalloc char[1024];
            uint length = Kernel32.GetModuleFileName(Entrypoint.Module, buffer, 1024);
            string path = new string(buffer, 0, (int)length);
            return Path.GetDirectoryName(path) ?? AppContext.BaseDirectory;
        }
    }
}
