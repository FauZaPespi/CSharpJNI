using PureJNI.Jvm;
using C = PureJNI.Game.GameClasses;

namespace PureJNI.Game
{
    public readonly unsafe struct Minecraft
    {
        public readonly JniNative.JNIEnv* Env;
        public readonly IntPtr Handle;

        private Minecraft(JniNative.JNIEnv* env, IntPtr handle)
        {
            Env = env;
            Handle = handle;
        }

        public bool IsValid => Handle != IntPtr.Zero;

        // Minecraft.getMinecraft(). Requires GameClasses.Init.
        public static Minecraft Get(JniNative.JNIEnv* env)
            => new(env, Jni.CallStaticObjectMethod(env, C.Minecraft, C.MinecraftGetMinecraft));

        public static int GetDebugFps(JniNative.JNIEnv* env)
            => Jni.CallStaticIntMethod(env, C.Minecraft, C.MinecraftGetDebugFps);

        public int Fps => GetDebugFps(Env);

        public Player Player => new(Env, IsValid ? Jni.GetObjectField(Env, Handle, C.MinecraftThePlayer) : IntPtr.Zero);

        public Timer Timer => new(Env, IsValid ? Jni.GetObjectField(Env, Handle, C.MinecraftTimer) : IntPtr.Zero);

        public Session Session => new(Env, IsValid ? Jni.CallObjectMethod(Env, Handle, C.MinecraftGetSession) : IntPtr.Zero);
    }
}
