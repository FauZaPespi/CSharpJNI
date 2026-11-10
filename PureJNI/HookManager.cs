using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PureJNI
{
    public unsafe static class HookManager
    {
        [DllImport("opengl32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr wglGetProcAddress(string lpszProc);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        // MinHook P/Invoke wrappers
        [DllImport("MinHook.x64.dll", EntryPoint = "MH_Initialize")]
        public static extern int MH_Initialize();

        [DllImport("MinHook.x64.dll", EntryPoint = "MH_CreateHook")]
        public static extern int MH_CreateHook(IntPtr pTarget, IntPtr pDetour, out IntPtr ppOriginal);

        [DllImport("MinHook.x64.dll", EntryPoint = "MH_EnableHook")]
        public static extern int MH_EnableHook(IntPtr pTarget);

        public static void Initialize()
        {
            MH_Initialize();

            // Locate wglSwapBuffers inside opengl32.dll
            IntPtr openglModule = GetModuleHandle("opengl32.dll");
            IntPtr targetFunction = GetProcAddress(openglModule, "wglSwapBuffers");

            // Get pointer to your hooked method
            delegate* unmanaged<IntPtr, IntPtr> detour = &SwapBufferHook.HookedWglSwapBuffers;

            // Create and enable the hook
            MH_CreateHook(targetFunction, (IntPtr)detour, out nint _originalSwapBuffersPtr);
            MH_EnableHook(targetFunction);
        }
    }
}
