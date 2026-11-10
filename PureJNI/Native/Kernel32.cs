using System.Runtime.InteropServices;

namespace PureJNI.Native
{
    internal static unsafe partial class Kernel32
    {
        public const uint DLL_PROCESS_ATTACH = 1;

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool AllocConsole();

        [LibraryImport("kernel32.dll", SetLastError = true)]
        public static partial IntPtr CreateThread(IntPtr attributes, nuint stackSize, delegate* unmanaged<IntPtr, uint> start,
            IntPtr parameter, uint creationFlags, out uint threadId);

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool CloseHandle(IntPtr handle);

        [LibraryImport("kernel32.dll", EntryPoint = "GetModuleFileNameW", SetLastError = true)]
        public static partial uint GetModuleFileName(IntPtr module, char* fileName, uint size);

        [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW", StringMarshalling = StringMarshalling.Utf16)]
        public static partial IntPtr GetModuleHandle(string moduleName);

        [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf8)]
        public static partial IntPtr GetProcAddress(IntPtr module, string procName);
    }
}
