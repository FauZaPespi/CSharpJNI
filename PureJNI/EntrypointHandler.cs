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

            int res = javaVM->functions->GetEnv(jvmPtr, (void**)&jniEnvPtr, 0x00010008);

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
            bool running = true;
            if (running)
            {
                IntPtr localClass = JniUtils.FindClass(env, "ave");

                if (localClass != IntPtr.Zero)
                {
                    //Console.WriteLine($"Current FPS: {GetDebugFPS(env, localClass)}");
                    IntPtr minecraftInstance = GetMCInstance(env, localClass);
                    if (minecraftInstance != IntPtr.Zero)
                    {
                        IntPtr sessionObj = GetSessionObj(env, localClass, minecraftInstance);
                        if (sessionObj != IntPtr.Zero)
                        {
                            //Console.WriteLine($"Username: {GetUsername(env, localClass, sessionObj)}");


                            IntPtr thePlayer = GetThePlayer(env, localClass);
                            if (thePlayer != IntPtr.Zero)
                            {
                                //Console.WriteLine("thePlayer found");
                                //Console.WriteLine($"Position: X={GetPosX(env, localClass, thePlayer)} Y={GetPosY(env, localClass, thePlayer)} Z={GetPosZ(env, localClass, thePlayer)}");

                                SetTimerSpeed(env, localClass, 2);
                                while (running)
                                {
                                    Console.WriteLine($"Current FPS: {GetDebugFPS(env, localClass)}");
                                    Console.WriteLine($"Username: {GetUsername(env, localClass, sessionObj)}");
                                    Console.WriteLine($"Position: X={GetPosX(env, localClass, thePlayer)} Y={GetPosY(env, localClass, thePlayer)} Z={GetPosZ(env, localClass, thePlayer)}");
                                    Thread.Sleep(1000);
                                }
                            }
                            else
                            {
                                Console.WriteLine("thePlayer not found");
                            }
                        }
                    }

                }
                else
                {
                    Console.WriteLine("Classe non trouvée.");
                }
                env->functions->PopLocalFrame((nint)env, IntPtr.Zero);
            }

            javaVM->functions->DetachCurrentThread(jvmPtr);
            Thread.Sleep(50);
            Console.WriteLine("thread detach");
            return 0;
        }

        unsafe public static int GetDebugFPS(JniNative.JNIEnv* env, IntPtr MinecraftClazz)
        {
            if (MinecraftClazz != IntPtr.Zero)
            {
                IntPtr getDebugFpsMethodId = JniUtils.GetStaticMethodID(env, MinecraftClazz, "ai", "()I");

                if (getDebugFpsMethodId != IntPtr.Zero)
                {
                    if (getDebugFpsMethodId != IntPtr.Zero)
                    {
                        int fps = JniUtils.CallStaticIntMethod(env, MinecraftClazz, getDebugFpsMethodId);
                        return fps;
                    }
                }
            }
            return 0;
        }

        unsafe public static IntPtr GetSessionObj(JniNative.JNIEnv* env, IntPtr MinecraftClazz, IntPtr mcInstance)
        {
            if (MinecraftClazz != IntPtr.Zero)
            {
                IntPtr getSessionMethodID = JniUtils.GetMethodID(env, MinecraftClazz, "L", "()Lavm;");
                if (getSessionMethodID != IntPtr.Zero)
                {
                    IntPtr sessionObj = JniUtils.CallObjectMethod(env, mcInstance, getSessionMethodID);
                    if (sessionObj != IntPtr.Zero)
                    {
                        return sessionObj;
                    }
                }
            }
            return 0;
        }
        unsafe public static IntPtr GetMCInstance(JniNative.JNIEnv* env, IntPtr MinecraftClazz)
        {
            if (MinecraftClazz != IntPtr.Zero)
            {
                IntPtr getMinecraftMethodID = JniUtils.GetStaticMethodID(env, MinecraftClazz, "A", "()Lave;");
                if (getMinecraftMethodID != IntPtr.Zero)
                {
                    IntPtr minecraftInstance = JniUtils.CallStaticObjectMethod(env, MinecraftClazz, getMinecraftMethodID);
                    if (minecraftInstance != IntPtr.Zero)
                    {
                        return minecraftInstance;
                    }
                }
            }
            return 0;
        }


        unsafe public static string GetUsername(JniNative.JNIEnv* env, IntPtr MinecraftClazz, IntPtr sessionObj)
        {
            string username = "Not found";
            if (MinecraftClazz != IntPtr.Zero)
            {
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
                            username = Marshal.PtrToStringAnsi((IntPtr)utfChars);
                            JniUtils.ReleaseStringUTFChars(env, usernameStrObj, utfChars);
                        }
                    }
                }
            }
            return username;
        }

        unsafe public static IntPtr GetThePlayer(JniNative.JNIEnv* env, IntPtr MinecraftClazz)
        {
            if (MinecraftClazz != IntPtr.Zero)
            {
                IntPtr mcInstance = GetMCInstance(env, MinecraftClazz);
                IntPtr playerFieldID = JniUtils.GetFieldID(env, MinecraftClazz, "h", "Lbew;");

                if (playerFieldID != IntPtr.Zero)
                {
                    if (mcInstance != IntPtr.Zero)
                    {
                        IntPtr thePlayer = JniUtils.GetObjectField(env, mcInstance, playerFieldID);

                        if (thePlayer != IntPtr.Zero)
                        {
                            return thePlayer;
                        }
                    }
                }
            }
            return 0;
        }

        unsafe public static IntPtr GetEntityClazz(JniNative.JNIEnv* env)
        {
            IntPtr entityClass = JniUtils.FindClass(env, "pk");
            if (entityClass != IntPtr.Zero)
            {
                return entityClass;
            }
            return 0;
        }

        unsafe public static double GetPosX(JniNative.JNIEnv* env, IntPtr MinecraftClazz, IntPtr thePlayer)
        {
            double posX = 0;
            if (MinecraftClazz != IntPtr.Zero)
            {
                IntPtr mcInstance = GetMCInstance(env, MinecraftClazz);

                if (thePlayer != IntPtr.Zero)
                {
                    IntPtr EntityClass = GetEntityClazz(env);
                    if (EntityClass != IntPtr.Zero && mcInstance != IntPtr.Zero)
                    {
                        IntPtr posXFieldID = JniUtils.GetFieldID(env, EntityClass, "s", "D");
                        if (posXFieldID != IntPtr.Zero)
                        {
                            posX = JniUtils.GetDoubleField(env, thePlayer, posXFieldID);
                        }
                    }
                }
            }
            return posX;
        }
        unsafe public static double GetPosY(JniNative.JNIEnv* env, IntPtr MinecraftClazz, IntPtr thePlayer)
        {
            double posY = 0;
            if (MinecraftClazz != IntPtr.Zero)
            {
                IntPtr mcInstance = GetMCInstance(env, MinecraftClazz);

                if (thePlayer != IntPtr.Zero)
                {
                    IntPtr EntityClass = GetEntityClazz(env);
                    if (EntityClass != IntPtr.Zero && mcInstance != IntPtr.Zero)
                    {
                        IntPtr posYFieldID = JniUtils.GetFieldID(env, EntityClass, "t", "D");
                        if (posYFieldID != IntPtr.Zero)
                        {
                            posY = JniUtils.GetDoubleField(env, thePlayer, posYFieldID);
                        }
                    }
                }
            }
            return posY;
        }

        unsafe public static double GetPosZ(JniNative.JNIEnv* env, IntPtr MinecraftClazz, IntPtr thePlayer)
        {
            double posZ = 0;
            if (MinecraftClazz != IntPtr.Zero)
            {
                IntPtr mcInstance = GetMCInstance(env, MinecraftClazz);

                if (thePlayer != IntPtr.Zero)
                {
                    IntPtr EntityClass = GetEntityClazz(env);
                    if (EntityClass != IntPtr.Zero && mcInstance != IntPtr.Zero)
                    {
                        IntPtr posZFieldID = JniUtils.GetFieldID(env, EntityClass, "u", "D");
                        if (posZFieldID != IntPtr.Zero)
                        {
                            posZ = JniUtils.GetDoubleField(env, thePlayer, posZFieldID);
                        }
                    }
                }
            }
            return posZ;
        }


        unsafe public static void SetPosX(JniNative.JNIEnv* env, IntPtr MinecraftClazz, IntPtr thePlayer, double val)
        {
            if (MinecraftClazz != IntPtr.Zero)
            {
                IntPtr mcInstance = GetMCInstance(env, MinecraftClazz);

                if (thePlayer != IntPtr.Zero)
                {
                    IntPtr EntityClass = GetEntityClazz(env);
                    if (EntityClass != IntPtr.Zero && mcInstance != IntPtr.Zero)
                    {
                        IntPtr posXFieldID = JniUtils.GetFieldID(env, EntityClass, "s", "D");
                        if (posXFieldID != IntPtr.Zero)
                        {
                            JniUtils.SetDoubleField(env, thePlayer, posXFieldID, val);
                        }
                    }
                }
            }
        }
        unsafe public static void SetPosY(JniNative.JNIEnv* env, IntPtr MinecraftClazz, IntPtr thePlayer, double val)
        {
            if (MinecraftClazz != IntPtr.Zero)
            {
                IntPtr mcInstance = GetMCInstance(env, MinecraftClazz);

                if (thePlayer != IntPtr.Zero)
                {
                    IntPtr EntityClass = GetEntityClazz(env);
                    if (EntityClass != IntPtr.Zero && mcInstance != IntPtr.Zero)
                    {
                        IntPtr posYFieldID = JniUtils.GetFieldID(env, EntityClass, "t", "D");
                        if (posYFieldID != IntPtr.Zero)
                        {
                            JniUtils.SetDoubleField(env, thePlayer, posYFieldID, val);
                        }
                    }
                }
            }
        }
        unsafe public static void SetPosZ(JniNative.JNIEnv* env, IntPtr MinecraftClazz, IntPtr thePlayer, double val)
        {
            if (MinecraftClazz != IntPtr.Zero)
            {
                IntPtr mcInstance = GetMCInstance(env, MinecraftClazz);

                if (thePlayer != IntPtr.Zero)
                {
                    IntPtr EntityClass = GetEntityClazz(env);
                    if (EntityClass != IntPtr.Zero && mcInstance != IntPtr.Zero)
                    {
                        IntPtr posZFieldID = JniUtils.GetFieldID(env, EntityClass, "u", "D");
                        if (posZFieldID != IntPtr.Zero)
                        {
                            JniUtils.SetDoubleField(env, thePlayer, posZFieldID, val);
                        }
                    }
                }
            }
        }


        unsafe public static void SetTimerSpeed(JniNative.JNIEnv* env, IntPtr MinecraftClazz, float speed)
        {
            if (MinecraftClazz != IntPtr.Zero)
            {
                IntPtr mcInstance = GetMCInstance(env, MinecraftClazz);
                if (mcInstance != IntPtr.Zero)
                {
                    IntPtr timerFieldID = JniUtils.GetFieldID(env, MinecraftClazz, "Y", "Lavl;");

                    if (timerFieldID != IntPtr.Zero)
                    {
                        IntPtr timerObj = JniUtils.GetObjectField(env, mcInstance, timerFieldID);
                        if (timerObj != IntPtr.Zero)
                        {
                            IntPtr timerClass = JniUtils.FindClass(env, "avl");
                            if (timerClass != IntPtr.Zero)
                            {
                                IntPtr timerSpeedFieldID = JniUtils.GetFieldID(env, timerClass, "d", "F");
                                if (timerSpeedFieldID != IntPtr.Zero)
                                {
                                    JniUtils.SetFloatField(env, timerObj, timerSpeedFieldID, speed);

                                }

                            }

                        }
                    }
                }
            }
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

