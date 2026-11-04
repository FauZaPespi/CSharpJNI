using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PureJNI
{
    // empiler les fixed ça me tendais et cétais pas clean la au moins c'est + propre
    public static unsafe class JniUtils
    {
        public static IntPtr FindClass(JniNative.JNIEnv* env, string className)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(className + "\0");
            fixed (byte* ptr = bytes)
            {
                IntPtr localClass = env->functions->FindClass((IntPtr)env, ptr);
                if (localClass == IntPtr.Zero) return IntPtr.Zero;
                IntPtr globalClass = env->functions->NewGlobalRef((IntPtr)env, localClass);

                env->functions->DeleteLocalRef((IntPtr)env, localClass);

                return globalClass;
            }
        }

        public static IntPtr CallStaticObjectMethod(JniNative.JNIEnv* env, IntPtr clazz, IntPtr methodID)
        {
            JniNative.jvalue* args = null;
            return env->functions->CallStaticObjectMethodA((IntPtr)env, clazz, methodID, args);
        }

        public static IntPtr CallObjectMethod(JniNative.JNIEnv* env, IntPtr obj, IntPtr methodID)
        {
            JniNative.jvalue* args = null;
            return env->functions->CallObjectMethodA((IntPtr)env, obj, methodID, args);
        }

        public static byte* GetStringUTFChars(JniNative.JNIEnv* env, IntPtr str, byte* isCopy = null)
        {
            return env->functions->GetStringUTFChars((IntPtr)env, str, isCopy);
        }

        public static void ReleaseStringUTFChars(JniNative.JNIEnv* env, IntPtr str, byte* chars)
        {
            env->functions->ReleaseStringUTFChars((IntPtr)env, str, chars);
        }

        public static IntPtr GetStaticMethodID(JniNative.JNIEnv* env, IntPtr clazz, string name, string sig)
        {
            byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(name + "\0");
            byte[] sigBytes = System.Text.Encoding.UTF8.GetBytes(sig + "\0");

            fixed (byte* namePtr = nameBytes)
            {
                fixed (byte* sigPtr = sigBytes)
                {
                    return env->functions->GetStaticMethodID((IntPtr)env, clazz, namePtr, sigPtr);
                }
            }
        }

        public static IntPtr GetMethodID(JniNative.JNIEnv* env, IntPtr clazz, string name, string sig)
        {
            byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(name + "\0");
            byte[] sigBytes = System.Text.Encoding.UTF8.GetBytes(sig + "\0");

            fixed (byte* namePtr = nameBytes)
            {
                fixed (byte* sigPtr = sigBytes)
                {
                    return env->functions->GetMethodID((IntPtr)env, clazz, namePtr, sigPtr);
                }
            }
        }

        public static IntPtr GetMethodId(JniNative.JNIEnv* env, IntPtr clazz, string name, string sig)
        {
            return GetMethodID(env, clazz, name, sig);
        }

        public static int CallStaticIntMethod(JniNative.JNIEnv* env, IntPtr clazz, IntPtr methodID)
        {
            JniNative.jvalue* args = null;
            return env->functions->CallStaticIntMethodA((IntPtr)env, clazz, methodID, args);
        }

        public static int CallIntMethod(JniNative.JNIEnv* env, IntPtr obj, IntPtr methodID)
        {
            JniNative.jvalue* args = null;
            return env->functions->CallIntMethodA((IntPtr)env, obj, methodID, args);
        }
    }
}