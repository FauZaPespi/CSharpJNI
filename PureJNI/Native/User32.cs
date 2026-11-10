using System.Runtime.InteropServices;

namespace PureJNI.Native
{
    internal static unsafe partial class User32
    {
        public const int GWLP_WNDPROC = -4;

        public const uint WM_INPUT = 0x00FF;
        public const uint WM_KEYFIRST = 0x0100;
        public const uint WM_KEYDOWN = 0x0100;
        public const uint WM_KEYUP = 0x0101;
        public const uint WM_KEYLAST = 0x0109;
        public const uint WM_MOUSEFIRST = 0x0200;
        public const uint WM_MOUSELAST = 0x020E;

        public const int VK_INSERT = 0x2D;

        [LibraryImport("user32.dll")]
        public static partial IntPtr WindowFromDC(IntPtr hdc);

        [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        public static partial IntPtr SetWindowLongPtr(IntPtr hwnd, int index, IntPtr value);

        [LibraryImport("user32.dll", EntryPoint = "CallWindowProcW")]
        public static partial IntPtr CallWindowProc(IntPtr prevWndProc, IntPtr hwnd, uint msg, nuint wParam, IntPtr lParam);

        [LibraryImport("user32.dll", EntryPoint = "DefWindowProcW")]
        public static partial IntPtr DefWindowProc(IntPtr hwnd, uint msg, nuint wParam, IntPtr lParam);
    }
}
