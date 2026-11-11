using System.Runtime.InteropServices;
using HexaGen.Runtime;
using PureJNI.Game;
using PureJNI.Hook;
using PureJNI.Jvm;
using PureJNI.Modules;
using PureJNI.Native;

namespace PureJNI.Core
{
    internal static unsafe class Bootstrap
    {
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

            string nativeDir = NativeDeps.Extract();
            Log.Info($"native DLLs in {nativeDir}");
            LibraryLoader.CustomLoadFolders.Add(nativeDir);
            MinHook.Load(Path.Combine(nativeDir, NativeDeps.MinHook));
            NativeLibrary.Load(Path.Combine(nativeDir, NativeDeps.CImGui));
            NativeLibrary.Load(Path.Combine(nativeDir, NativeDeps.ImGuiImpl));

            if (!SwapBuffersHook.Install())
                Log.Error("SwapBuffers hook failed, no menu");

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
