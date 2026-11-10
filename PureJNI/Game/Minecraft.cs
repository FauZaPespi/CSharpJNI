using PureJNI.Jvm;
using C = PureJNI.Game.GameClasses;

namespace PureJNI.Game
{
    // Wrapper of the Minecraft instance (ave) for one thread.
    // Objects it returns are JNI local refs: use them inside the current tick / local frame only.
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

        // Invalid (IsValid == false) when not in a world.
        public Player Player => new(Env, IsValid ? Jni.GetObjectField(Env, Handle, C.MinecraftThePlayer) : IntPtr.Zero);

        public Timer Timer => new(Env, IsValid ? Jni.GetObjectField(Env, Handle, C.MinecraftTimer) : IntPtr.Zero);

        public Session Session => new(Env, IsValid ? Jni.CallObjectMethod(Env, Handle, C.MinecraftGetSession) : IntPtr.Zero);
    }
}
