using System.Runtime.InteropServices;
using System.Text;

namespace PureJNI.Jvm
{
    // Thin helpers over the JNIEnv function table, so callers never write env->functions->X((IntPtr)env, ...).
    public static unsafe class Jni
    {
        // ---- classes / IDs -------------------------------------------------------------

        // Returns a GLOBAL ref (valid on every thread, never freed: classes live for the whole session).
        public static IntPtr FindClassGlobal(JniNative.JNIEnv* env, string name)
        {
            fixed (byte* p = Utf8(name))
            {
                IntPtr local = env->functions->FindClass((IntPtr)env, p);
                if (local == IntPtr.Zero)
                {
                    ClearException(env);
                    return IntPtr.Zero;
                }
                IntPtr global = env->functions->NewGlobalRef((IntPtr)env, local);
                env->functions->DeleteLocalRef((IntPtr)env, local);
                return global;
            }
        }

        public static IntPtr GetFieldID(JniNative.JNIEnv* env, IntPtr clazz, string name, string sig)
        {
            fixed (byte* n = Utf8(name))
            fixed (byte* s = Utf8(sig))
                return CheckId(env, env->functions->GetFieldID((IntPtr)env, clazz, n, s));
        }

        public static IntPtr GetStaticFieldID(JniNative.JNIEnv* env, IntPtr clazz, string name, string sig)
        {
            fixed (byte* n = Utf8(name))
            fixed (byte* s = Utf8(sig))
                return CheckId(env, env->functions->GetStaticFieldID((IntPtr)env, clazz, n, s));
        }

        public static IntPtr GetMethodID(JniNative.JNIEnv* env, IntPtr clazz, string name, string sig)
        {
            fixed (byte* n = Utf8(name))
            fixed (byte* s = Utf8(sig))
                return CheckId(env, env->functions->GetMethodID((IntPtr)env, clazz, n, s));
        }

        public static IntPtr GetStaticMethodID(JniNative.JNIEnv* env, IntPtr clazz, string name, string sig)
        {
            fixed (byte* n = Utf8(name))
            fixed (byte* s = Utf8(sig))
                return CheckId(env, env->functions->GetStaticMethodID((IntPtr)env, clazz, n, s));
        }

        // ---- fields --------------------------------------------------------------------

        public static IntPtr GetObjectField(JniNative.JNIEnv* env, IntPtr obj, IntPtr field) => env->functions->GetObjectField((IntPtr)env, obj, field);
        public static int GetIntField(JniNative.JNIEnv* env, IntPtr obj, IntPtr field) => env->functions->GetIntField((IntPtr)env, obj, field);
        public static float GetFloatField(JniNative.JNIEnv* env, IntPtr obj, IntPtr field) => env->functions->GetFloatField((IntPtr)env, obj, field);
        public static double GetDoubleField(JniNative.JNIEnv* env, IntPtr obj, IntPtr field) => env->functions->GetDoubleField((IntPtr)env, obj, field);

        public static void SetIntField(JniNative.JNIEnv* env, IntPtr obj, IntPtr field, int v) => env->functions->SetIntField((IntPtr)env, obj, field, v);
        public static void SetFloatField(JniNative.JNIEnv* env, IntPtr obj, IntPtr field, float v) => env->functions->SetFloatField((IntPtr)env, obj, field, v);
        public static void SetDoubleField(JniNative.JNIEnv* env, IntPtr obj, IntPtr field, double v) => env->functions->SetDoubleField((IntPtr)env, obj, field, v);

        // ---- calls (no arguments, or one jvalue) ----------------------------------------

        public static IntPtr CallObjectMethod(JniNative.JNIEnv* env, IntPtr obj, IntPtr method) => env->functions->CallObjectMethodA((IntPtr)env, obj, method, null);
        public static IntPtr CallStaticObjectMethod(JniNative.JNIEnv* env, IntPtr clazz, IntPtr method) => env->functions->CallStaticObjectMethodA((IntPtr)env, clazz, method, null);
        public static int CallStaticIntMethod(JniNative.JNIEnv* env, IntPtr clazz, IntPtr method) => env->functions->CallStaticIntMethodA((IntPtr)env, clazz, method, null);
        public static bool CallStaticBooleanMethod(JniNative.JNIEnv* env, IntPtr clazz, IntPtr method) => env->functions->CallStaticBooleanMethodA((IntPtr)env, clazz, method, null) != 0;

        public static void CallStaticVoidMethod(JniNative.JNIEnv* env, IntPtr clazz, IntPtr method, JniNative.jvalue arg)
            => env->functions->CallStaticVoidMethodA((IntPtr)env, clazz, method, &arg);

        // ---- strings / refs / exceptions -------------------------------------------------

        public static string? GetString(JniNative.JNIEnv* env, IntPtr jstring)
        {
            if (jstring == IntPtr.Zero) return null;
            byte* chars = env->functions->GetStringUTFChars((IntPtr)env, jstring, null);
            if (chars == null) return null;
            try { return Marshal.PtrToStringUTF8((IntPtr)chars); }
            finally { env->functions->ReleaseStringUTFChars((IntPtr)env, jstring, chars); }
        }

        public static void DeleteLocalRef(JniNative.JNIEnv* env, IntPtr obj)
        {
            if (obj != IntPtr.Zero) env->functions->DeleteLocalRef((IntPtr)env, obj);
        }

        // Clears a pending Java exception. Returns true when there was one.
        public static bool ClearException(JniNative.JNIEnv* env)
        {
            if (env->functions->ExceptionCheck((IntPtr)env) == 0) return false;
            env->functions->ExceptionClear((IntPtr)env);
            return true;
        }

        private static IntPtr CheckId(JniNative.JNIEnv* env, IntPtr id)
        {
            // A missing field/method throws NoSuchFieldError/NoSuchMethodError: clear it, return 0.
            if (id == IntPtr.Zero) ClearException(env);
            return id;
        }

        private static byte[] Utf8(string s) => Encoding.UTF8.GetBytes(s + "\0");
    }
}
