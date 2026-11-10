using PureJNI.Jvm;
using C = PureJNI.Game.GameClasses;

namespace PureJNI.Game
{
    // net.minecraft.util.Session (avm).
    public readonly unsafe struct Session
    {
        private readonly JniNative.JNIEnv* _env;
        public readonly IntPtr Handle;

        internal Session(JniNative.JNIEnv* env, IntPtr handle)
        {
            _env = env;
            Handle = handle;
        }

        public bool IsValid => Handle != IntPtr.Zero;

        public string? Username
        {
            get
            {
                if (!IsValid) return null;
                IntPtr str = Jni.CallObjectMethod(_env, Handle, C.SessionGetUsername);
                try { return Jni.GetString(_env, str); }
                finally { Jni.DeleteLocalRef(_env, str); }
            }
        }
    }
}
