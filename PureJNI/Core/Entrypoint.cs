using System.Runtime.InteropServices;
using PureJNI.Native;

namespace PureJNI.Core
{
    // The only DllMain of the DLL. It runs under the loader lock: it only starts the bootstrap thread.
    internal static unsafe class Entrypoint
    {
        // HMODULE of PureJNI.dll, used to find the native DLLs shipped next to it.
        public static IntPtr Module { get; private set; }

        //https://github.com/dotnet/runtime/issues/66546
        [UnmanagedCallersOnly(EntryPoint = "DllMain")]
        public static int DllMain(IntPtr module, uint reason, IntPtr reserved)
        {
            if (reason == Kernel32.DLL_PROCESS_ATTACH)
            {
                Module = module;
                IntPtr thread = Kernel32.CreateThread(IntPtr.Zero, 0, &Bootstrap.ThreadMain, IntPtr.Zero, 0, out _);
                if (thread != IntPtr.Zero)
                    Kernel32.CloseHandle(thread);
            }
            return 1;
        }
    }
}
