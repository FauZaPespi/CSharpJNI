using System;
using System.Runtime.InteropServices;

namespace PureJNI
{
    public unsafe static class JniNative
    {
        // Types primitifs Java
        public struct jobject_t { public IntPtr Handle; }

        // Pointeur vers la table des fonctions JNIEnv
        public struct JNIEnv
        {
            public JNINativeInterface_* functions;
        }

        public struct JavaVM
        {
            public JNIInvokeInterface_* functions;
        }

        // Table complète des fonctions JNI (`JNINativeInterface_`)
        public struct JNINativeInterface_
        {
            public void* reserved0;
            public void* reserved1;
            public void* reserved2;
            public void* reserved3;

            public delegate* unmanaged<IntPtr, int> GetVersion;
            public delegate* unmanaged<IntPtr, byte*, IntPtr, sbyte*, int, IntPtr> DefineClass;
            public delegate* unmanaged<IntPtr, byte*, IntPtr> FindClass; // Très important !

            public delegate* unmanaged<IntPtr, IntPtr, IntPtr> FromReflectedMethod;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr> FromReflectedField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, byte, IntPtr> ToReflectedMethod;

            public delegate* unmanaged<IntPtr, IntPtr, IntPtr> GetSuperclass;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, byte> IsAssignableFrom;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, byte, IntPtr> ToReflectedField;

            public delegate* unmanaged<IntPtr, IntPtr, int> Throw;
            public delegate* unmanaged<IntPtr, IntPtr, byte*, int> ThrowNew;
            public delegate* unmanaged<IntPtr, IntPtr> ExceptionOccurred;
            public delegate* unmanaged<IntPtr, void> ExceptionDescribe;
            public delegate* unmanaged<IntPtr, void> ExceptionClear;
            public delegate* unmanaged<IntPtr, byte*, void> FatalError;

            public delegate* unmanaged<IntPtr, int, int> PushLocalFrame;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr> PopLocalFrame;

            public delegate* unmanaged<IntPtr, IntPtr, IntPtr> NewGlobalRef;
            public delegate* unmanaged<IntPtr, IntPtr, void> DeleteGlobalRef;
            public delegate* unmanaged<IntPtr, IntPtr, void> DeleteLocalRef;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, byte> IsSameObject;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr> NewLocalRef;
            public delegate* unmanaged<IntPtr, int, int> EnsureLocalCapacity;

            public delegate* unmanaged<IntPtr, IntPtr, IntPtr> AllocObject;
            // (Note: Les méthodes avec varargs (...) nécessitent généralement des tableaux JValue ou des wrappers spécifiques)
            public void* NewObject;
            public void* NewObjectV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, IntPtr> NewObjectA;

            public delegate* unmanaged<IntPtr, IntPtr, IntPtr> GetObjectClass;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, byte> IsInstanceOf;

            public delegate* unmanaged<IntPtr, IntPtr, byte*, byte*, IntPtr> GetMethodID; // Pour trouver les méthodes

            public void* CallObjectMethod;
            public void* CallObjectMethodV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, IntPtr> CallObjectMethodA;

            public void* CallBooleanMethod;
            public void* CallBooleanMethodV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, byte> CallBooleanMethodA;

            public void* CallByteMethod;
            public void* CallByteMethodV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, sbyte> CallByteMethodA;

            public void* CallCharMethod;
            public void* CallCharMethodV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, ushort> CallCharMethodA;

            public void* CallShortMethod;
            public void* CallShortMethodV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, short> CallShortMethodA;

            public void* CallIntMethod;
            public void* CallIntMethodV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, int> CallIntMethodA;

            public void* CallLongMethod;
            public void* CallLongMethodV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, long> CallLongMethodA;

            public void* CallFloatMethod;
            public void* CallFloatMethodV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, float> CallFloatMethodA;

            public void* CallDoubleMethod;
            public void* CallDoubleMethodV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, double> CallDoubleMethodA;

            public void* CallVoidMethod;
            public void* CallVoidMethodV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, void> CallVoidMethodA;

            // --- Champs (Fields) ---
            public delegate* unmanaged<IntPtr, IntPtr, byte*, byte*, IntPtr> GetFieldID;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr> GetObjectField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, byte> GetBooleanField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, sbyte> GetByteField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, ushort> GetCharField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, short> GetShortField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, int> GetIntField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, long> GetLongField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, float> GetFloatField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, double> GetDoubleField;

            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr, void> SetObjectField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, byte, void> SetBooleanField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, sbyte, void> SetByteField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, ushort, void> SetCharField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, short, void> SetShortField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, int, void> SetIntField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, long, void> SetLongField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, float, void> SetFloatField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, double, void> SetDoubleField;

            // --- Méthodes Statiques ---
            public delegate* unmanaged<IntPtr, IntPtr, byte*, byte*, IntPtr> GetStaticMethodID;
            public void* CallStaticObjectMethod;
            public void* CallStaticObjectMethodV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, IntPtr> CallStaticObjectMethodA;

            public void* CallStaticBooleanMethod;
            public void* CallStaticBooleanMethodV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, byte> CallStaticBooleanMethodA;

            public void* CallStaticByteMethod;
            public void* CallStaticByteMethodV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, sbyte> CallStaticByteMethodA;

            public void* CallStaticCharMethod;
            public void* CallStaticCharMethodV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, ushort> CallStaticCharMethodA;

            public void* CallStaticShortMethod;
            public void* CallStaticShortMethodV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, short> CallStaticShortMethodA;

            public void* CallStaticIntMethod;
            public void* CallStaticIntMethodV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, int> CallStaticIntMethodA;

            public void* CallStaticLongMethod;
            public void* CallStaticLongMethodV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, long> CallStaticLongMethodA;

            public void* CallStaticFloatMethod;
            public void* CallStaticFloatMethodV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, float> CallStaticFloatMethodA;

            public void* CallStaticDoubleMethod;
            public void* CallStaticDoubleMethodV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, double> CallStaticDoubleMethodA;

            public void* CallStaticVoidMethod;
            public void* CallStaticVoidMethodV;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, void> CallStaticVoidMethodA;

            // --- Champs Statiques ---
            public delegate* unmanaged<IntPtr, IntPtr, byte*, byte*, IntPtr> GetStaticFieldID;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr> GetStaticObjectField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, byte> GetStaticBooleanField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, sbyte> GetStaticByteField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, ushort> GetStaticCharField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, short> GetStaticShortField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, int> GetStaticIntField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, long> GetStaticLongField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, float> GetStaticFloatField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, double> GetStaticDoubleField;

            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr, void> SetStaticObjectField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, byte, void> SetStaticBooleanField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, sbyte, void> SetStaticByteField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, ushort, void> SetStaticCharField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, short, void> SetStaticShortField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, int, void> SetStaticIntField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, long, void> SetStaticLongField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, float, void> SetStaticFloatField;
            public delegate* unmanaged<IntPtr, IntPtr, IntPtr, double, void> SetStaticDoubleField;

            // Gestion des chaînes et tableaux (la suite du fichier jni.h)
            // ... (Les autres offsets suivent exactement l'ordre du fichier d'origine)
        }

        // Table d'invocation JavaVM
        public struct JNIInvokeInterface_
        {
            public void* reserved0;
            public void* reserved1;
            public void* reserved2;

            public delegate* unmanaged<IntPtr, int> DestroyJavaVM;
            public delegate* unmanaged<IntPtr, void**, void*, int> AttachCurrentThread;
            public delegate* unmanaged<IntPtr, int> DetachCurrentThread;
            public delegate* unmanaged<IntPtr, void**, int, int> GetEnv; // Utilisé pour récupérer le JNIEnv*
            public delegate* unmanaged<IntPtr, void**, void*, int> AttachCurrentThreadAsDaemon;
        }
    }
}
