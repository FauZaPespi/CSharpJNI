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

                uint threadId;
                IntPtr hThread = CreateThread(
                    IntPtr.Zero,
                    0,
                    threadCallback,
                    IntPtr.Zero,
                    0,
                    out threadId
                );

                if (hThread == IntPtr.Zero)
                {
                    Console.WriteLine($"erreur : {Marshal.GetLastWin32Error()}");
                }
                else
                {   

                    Console.WriteLine($"ID du Thread : {threadId}");
                }
                System.Threading.Thread.Sleep(2000);
            }
            return true;
        }

        public static uint threadCallback(IntPtr lpParameter)
        {
            Console.WriteLine("[Log] From thread");
            IntPtr jvmPtr = IntPtr.Zero;
            int nVMs = 0;

            JNI_GetCreatedJavaVMs(ref jvmPtr, 1, out nVMs);

            if (nVMs == 0 || jvmPtr == IntPtr.Zero)
            {
                Console.WriteLine("[-] Aucune JVM trouvée.");
                return 0;
            }

            Console.WriteLine($"[+] JVM trouvée à : 0x{jvmPtr.ToInt64():X}");
            return 0;
        }

        #region Import

        [DllImport("jvm.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int JNI_GetCreatedJavaVMs(ref IntPtr vmBuf, int bufLen, out int nVMs);

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