using PureJNI.Jvm;
using C = PureJNI.Game.GameClasses;

namespace PureJNI.Game
{
    // EntityPlayerSP (bew), through the Entity (pk) fields.
    public readonly unsafe struct Player
    {
        private readonly JniNative.JNIEnv* _env;
        public readonly IntPtr Handle;

        internal Player(JniNative.JNIEnv* env, IntPtr handle)
        {
            _env = env;
            Handle = handle;
        }

        public bool IsValid => Handle != IntPtr.Zero;

        public double X
        {
            get => Jni.GetDoubleField(_env, Handle, C.EntityPosX);
            set => Jni.SetDoubleField(_env, Handle, C.EntityPosX, value);
        }

        public double Y
        {
            get => Jni.GetDoubleField(_env, Handle, C.EntityPosY);
            set => Jni.SetDoubleField(_env, Handle, C.EntityPosY, value);
        }

        public double Z
        {
            get => Jni.GetDoubleField(_env, Handle, C.EntityPosZ);
            set => Jni.SetDoubleField(_env, Handle, C.EntityPosZ, value);
        }
    }
}
