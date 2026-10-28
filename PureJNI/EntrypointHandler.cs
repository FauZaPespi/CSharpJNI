using System.Runtime.InteropServices;

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
            }
            return true;
        }

        [LibraryImport("user32.dll", EntryPoint = "MessageBoxA", StringMarshalling = StringMarshalling.Utf8)]
        private static partial int MessageBoxA(nint hWnd, string text, string caption, uint type);
    }
}