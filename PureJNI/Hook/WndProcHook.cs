using System.Runtime.InteropServices;
using Hexa.NET.ImGui.Backends.Win32;
using PureJNI.Core;
using PureJNI.Native;
using PureJNI.UI;

namespace PureJNI.Hook
{
    // Subclasses the Minecraft window. INSERT toggles the menu; while it is open, mouse and keyboard
    // messages go to ImGui only, so the game does not move or type.
    // Window messages are dispatched on the Minecraft render thread (LWJGL Display.processMessages).
    internal static unsafe class WndProcHook
    {
        private static IntPtr _original;
        private static IntPtr _hwnd;

        public static void Install(IntPtr hwnd)
        {
            if (_hwnd == hwnd) return;
            delegate* unmanaged<IntPtr, uint, nuint, IntPtr, IntPtr> proc = &WndProc;
            _original = User32.SetWindowLongPtr(hwnd, User32.GWLP_WNDPROC, (IntPtr)proc);
            if (_original == IntPtr.Zero)
            {
                Log.Error($"SetWindowLongPtr failed: {Marshal.GetLastPInvokeError()}");
                return;
            }
            _hwnd = hwnd;
            Log.Info("WndProc hooked");
        }

        [UnmanagedCallersOnly]
        private static IntPtr WndProc(IntPtr hwnd, uint msg, nuint wParam, IntPtr lParam)
        {
            try
            {
                if (wParam == User32.VK_INSERT && (msg == User32.WM_KEYDOWN || msg == User32.WM_KEYUP))
                {
                    // Bit 30 of lParam = key was already down: ignore auto-repeat.
                    if (msg == User32.WM_KEYDOWN && (lParam & (1 << 30)) == 0)
                        Menu.Toggle();
                    return IntPtr.Zero;
                }

                if (Menu.Visible && Menu.ImGuiReady)
                {
                    ImGuiImplWin32.WndProcHandler(hwnd, msg, wParam, lParam);
                    if (IsInput(msg))
                        // WM_INPUT must reach DefWindowProc to free the raw input buffer.
                        return msg == User32.WM_INPUT ? User32.DefWindowProc(hwnd, msg, wParam, lParam) : IntPtr.Zero;
                }
            }
            catch (Exception e)
            {
                Log.Error($"WndProc: {e.Message}");
            }
            return User32.CallWindowProc(_original, hwnd, msg, wParam, lParam);
        }

        private static bool IsInput(uint msg)
            => msg == User32.WM_INPUT
               || (msg >= User32.WM_KEYFIRST && msg <= User32.WM_KEYLAST)
               || (msg >= User32.WM_MOUSEFIRST && msg <= User32.WM_MOUSELAST);
    }
}
