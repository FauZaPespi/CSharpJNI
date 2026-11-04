using PureJNI;
using System.Runtime.InteropServices;
using System.Threading;

namespace PureJNI
{
    internal partial class EntrypointHandler
    {
        //https://github.com/dotnet/runtime/issues/66546
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

        public unsafe static uint threadCallback(IntPtr lpParameter)
        {
            Console.WriteLine("From thread");
            System.Threading.Thread.Sleep(8000);
            IntPtr jvmPtr = IntPtr.Zero;
            int nVMs = 0;

            JNI_GetCreatedJavaVMs(ref jvmPtr, 1, out nVMs);

            if (nVMs == 0 || jvmPtr == IntPtr.Zero)
            {
                Console.WriteLine("no jvm found.");
                return 0;
            }
            else
            {
                Console.WriteLine($"JVM found: 0x{jvmPtr.ToInt64():X}");
            }
            JniNative.JavaVM* javaVM = (JniNative.JavaVM*)jvmPtr;
            IntPtr jniEnvPtr = IntPtr.Zero;

            int res = javaVM->functions->GetEnv(jvmPtr, (void**)&jniEnvPtr, 0x00010008); // Pour Java 8 dcp 0x00010008 -> Java 8

            if (res == -2)
            {
                Console.WriteLine("Thread pas attach. Tentative d'attach...");
                res = javaVM->functions->AttachCurrentThread(jvmPtr, (void**)&jniEnvPtr, null);
            }

            if (res != 0 || jniEnvPtr == IntPtr.Zero)
            {
                Console.WriteLine($"erreur : {res}");
                return 0;
            }

            Console.WriteLine($"JNIEnv : 0x{jniEnvPtr.ToInt64():X}");

            JniNative.JNIEnv* env = (JniNative.JNIEnv*)jniEnvPtr;
            // https://mmv.wagyourtail.xyz/?q=%22net%2Fminecraft%2Fclient%2FMinecraft%22&MCV=%221.8.9%22
            IntPtr localClass = JniUtils.FindClass(env, "ave");

            if (localClass != IntPtr.Zero)
            {
                Console.WriteLine("class found");

                IntPtr getDebugFpsMethodId = JniUtils.GetStaticMethodID(env, localClass, "ai", "()I");

                if (getDebugFpsMethodId != IntPtr.Zero)
                {
                    Console.WriteLine("getDebugFPS method found");
                    if (getDebugFpsMethodId != IntPtr.Zero)
                    {
                        int fps = JniUtils.CallStaticIntMethod(env, localClass, getDebugFpsMethodId);
                        Console.WriteLine($"FPS actuels : {fps}");
                        System.Threading.Thread.Sleep(500);
                    }
                }
                else
                {
                    Console.WriteLine("getDebugFPS method not found");
                }
                IntPtr getMinecraftMethodID = JniUtils.GetStaticMethodID(env, localClass, "A", "()Lave;");
                if (getMinecraftMethodID != IntPtr.Zero)
                {
                    IntPtr minecraftInstance = JniUtils.CallStaticObjectMethod(env, localClass, getMinecraftMethodID);
                    if (minecraftInstance != IntPtr.Zero)
                    {
                        Console.WriteLine("Instance de Minecraft récupérée.");

                        IntPtr getSessionMethodID = JniUtils.GetMethodID(env, localClass, "L", "()Lavm;");
                        if (getSessionMethodID != IntPtr.Zero)
                        {
                            IntPtr sessionObj = JniUtils.CallObjectMethod(env, minecraftInstance, getSessionMethodID);
                            if (sessionObj != IntPtr.Zero)
                            {
                                Console.WriteLine("Objet Session récupéré.");

                                IntPtr sessionClass = JniUtils.FindClass(env, "avm");
                                IntPtr usernameMethodID = JniUtils.GetMethodID(env, sessionClass, "c", "()Ljava/lang/String;");

                                if (usernameMethodID != IntPtr.Zero)
                                {
                                    IntPtr usernameStrObj = JniUtils.CallObjectMethod(env, sessionObj, usernameMethodID);
                                    if (usernameStrObj != IntPtr.Zero)
                                    {
                                        byte* utfChars = JniUtils.GetStringUTFChars(env, usernameStrObj);
                                        if (utfChars != null)
                                        {
                                            string username = Marshal.PtrToStringAnsi((IntPtr)utfChars);
                                            Console.WriteLine($"Nom d'utilisateur : {username}");

                                            JniUtils.ReleaseStringUTFChars(env, usernameStrObj, utfChars);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Classe non trouvée.");
            }

            javaVM->functions->DetachCurrentThread(jvmPtr);
            Console.WriteLine("thread detach");
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

