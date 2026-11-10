using PureJNI.Jvm;
using C = PureJNI.Game.GameClasses;

namespace PureJNI.Game
{
    // org.lwjgl.input.Mouse. LWJGL is not thread-safe: call only from the Minecraft render thread.
    public static unsafe class LwjglMouse
    {
        public static bool IsGrabbed(JniNative.JNIEnv* env)
            => Jni.CallStaticBooleanMethod(env, C.Mouse, C.MouseIsGrabbed);

        public static void SetGrabbed(JniNative.JNIEnv* env, bool grabbed)
            => Jni.CallStaticVoidMethod(env, C.Mouse, C.MouseSetGrabbed, new JniNative.jvalue { z = grabbed ? (byte)1 : (byte)0 });
    }
}
