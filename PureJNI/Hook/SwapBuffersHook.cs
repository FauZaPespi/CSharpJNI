using System.Runtime.InteropServices;
using PureJNI.Core;
using PureJNI.Native;
using PureJNI.UI;

namespace PureJNI.Hook
{
    // Hooks opengl32!wglSwapBuffers: called by Minecraft's render thread ("Client thread") at the end of every frame,
    // with its GL context current. The menu is drawn here, right before the frame is presented.
    internal static unsafe class SwapBuffersHook
    {
        private static delegate* unmanaged<IntPtr, int> _original;
        private static bool _failed;

        public static bool Install()
        {
            IntPtr opengl = Kernel32.GetModuleHandle("opengl32.dll");
            IntPtr target = opengl == IntPtr.Zero ? IntPtr.Zero : Kernel32.GetProcAddress(opengl, "wglSwapBuffers");
            if (target == IntPtr.Zero)
            {
                Log.Error("wglSwapBuffers not found");
                return false;
            }

            int status = MinHook.Initialize();
            if (status != MinHook.MH_OK && status != MinHook.MH_ERROR_ALREADY_INITIALIZED)
            {
                Log.Error($"MH_Initialize: {status}");
                return false;
            }

            delegate* unmanaged<IntPtr, int> detour = &Detour;
            status = MinHook.CreateHook(target, (IntPtr)detour, out IntPtr original);
            if (status != MinHook.MH_OK)
            {
                Log.Error($"MH_CreateHook: {status}");
                return false;
            }
            // Set before enabling: the detour may run right after EnableHook.
            _original = (delegate* unmanaged<IntPtr, int>)original;

            status = MinHook.EnableHook(target);
            if (status != MinHook.MH_OK)
            {
                Log.Error($"MH_EnableHook: {status}");
                return false;
            }
            Log.Info("wglSwapBuffers hooked");
            return true;
        }

        [UnmanagedCallersOnly]
        private static int Detour(IntPtr hdc)
        {
            // A managed exception must never cross into javaw.exe. After the first one the menu is disabled.
            if (!_failed)
            {
                try
                {
                    Menu.OnFrame(hdc);
                }
                catch (Exception e)
                {
                    _failed = true;
                    Log.Error($"render hook disabled: {e}");
                }
            }
            return _original(hdc);
        }
    }
}
