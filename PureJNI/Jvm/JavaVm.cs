using System.Runtime.InteropServices;

namespace PureJNI.Jvm
{
    // Access to the JVM already running in the process (javaw.exe).
    public static unsafe partial class JavaVm
    {
        public const int JNI_OK = 0;
        public const int JNI_EDETACHED = -2;
        public const int JNI_VERSION_1_8 = 0x00010008;

        private static JniNative.JavaVM* _vm;

        public static bool IsFound => _vm != null;

        public static bool Find()
        {
            IntPtr vm = IntPtr.Zero;
            if (JNI_GetCreatedJavaVMs(&vm, 1, out int count) != JNI_OK || count == 0 || vm == IntPtr.Zero)
                return false;
            _vm = (JniNative.JavaVM*)vm;
            return true;
        }

        // Env of the current thread if it is already a Java thread (e.g. the Minecraft render thread), else null.
        public static JniNative.JNIEnv* GetEnv()
        {
            if (_vm == null) return null;
            IntPtr env = IntPtr.Zero;
            return _vm->functions->GetEnv((IntPtr)_vm, (void**)&env, JNI_VERSION_1_8) == JNI_OK
                ? (JniNative.JNIEnv*)env
                : null;
        }

        // Env of the current thread, attaching it to the JVM when needed.
        public static JniNative.JNIEnv* Attach()
        {
            JniNative.JNIEnv* env = GetEnv();
            if (env != null || _vm == null) return env;
            IntPtr p = IntPtr.Zero;
            return _vm->functions->AttachCurrentThreadAsDaemon((IntPtr)_vm, (void**)&p, null) == JNI_OK
                ? (JniNative.JNIEnv*)p
                : null;
        }

        public static void Detach()
        {
            if (_vm != null) _vm->functions->DetachCurrentThread((IntPtr)_vm);
        }

        [LibraryImport("jvm.dll")]
        private static partial int JNI_GetCreatedJavaVMs(IntPtr* vmBuf, int bufLen, out int nVMs);
    }
}
