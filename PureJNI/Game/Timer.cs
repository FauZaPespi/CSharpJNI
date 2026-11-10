using PureJNI.Jvm;
using C = PureJNI.Game.GameClasses;

namespace PureJNI.Game
{
    // net.minecraft.util.Timer (avl).
    public readonly unsafe struct Timer
    {
        private readonly JniNative.JNIEnv* _env;
        public readonly IntPtr Handle;

        internal Timer(JniNative.JNIEnv* env, IntPtr handle)
        {
            _env = env;
            Handle = handle;
        }

        public bool IsValid => Handle != IntPtr.Zero;

        public float Speed
        {
            get => Jni.GetFloatField(_env, Handle, C.TimerSpeed);
            set => Jni.SetFloatField(_env, Handle, C.TimerSpeed, value);
        }
    }
}
