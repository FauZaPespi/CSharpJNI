using System.Runtime.InteropServices;
using System.Threading;

namespace PureJNI
{
    internal partial class EntrypointHandler
    {
        [UnmanagedCallersOnly(EntryPoint = "DllMain")]
        public static bool DllMain(nint hModule, uint ul_reason_for_call, nint lpReserved)
        {
            if (ul_reason_for_call == 1)
            {
                MessageBoxA(nint.Zero, "DllMain called", "Test", 0);
                AllocConsole();
                Console.WriteLine("DllMain called with console..");
                

            }
            return true;
        }

        #region Import

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [LibraryImport("user32.dll", EntryPoint = "MessageBoxA", StringMarshalling = StringMarshalling.Utf8)]
        private static partial int MessageBoxA(nint hWnd, string text, string caption, uint type);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateThread(IntPtr lpThreadAttributes, uint dwStackSize, ThreadProc lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out uint lpThreadId);
        private delegate uint ThreadProc(IntPtr lpParameter);
        #endregion
    }

}