// =====================================================================================
//  Jni.cs : liaisons C# de l'interface JNI (jni.h) pour Java 8
// -------------------------------------------------------------------------------------
//  Fichier écrit par Claude Code (Claude Opus 5.5, Anthropic) le 2026-11-02, à la
//  demande de FauZaPespi. Il remplace la première traduction de jni.h (commit 535a416).
//
//  D'où viennent les informations :
//    1. Le jni.h d'un JDK 8 installé sur cette machine :
//         C:\Users\oscar_clv\.jdks\azul-1.8.0_422\include\jni.h
//         (+ include\win32\jni_md.h pour jint, jlong, jbyte et JNICALL)
//       L'instance Minecraft tourne sur Oracle JRE 1.8.0_51 (Z:\mc\java\jre-legacy,
//       voir instance.cfg). Une JRE ne fournit pas jni.h, mais la table de fonctions JNI
//       ne change pas entre les mises à jour d'une même version majeure : le jni.h d'un
//       JDK 8 décrit donc aussi la JRE 8u51.
//       Un script Python a extrait et numéroté les structures JNINativeInterface_ et
//       JNIInvokeInterface_ de ce jni.h. Les constantes et les types annexes (jvalue,
//       jobjectRefType, JNINativeMethod, JavaVMAttachArgs) ont été relus dans le même
//       fichier.
//    2. Une vérification sur le binaire réellement chargé par le jeu :
//         Z:\mc\java\jre-legacy\bin\server\jvm.dll (Oracle 1.8.0_51, amd64)
//       Un scan de sa section .data trouve deux tables JNI (la normale et celle de
//       -Xcheck:jni) aux RVA 0x790D50 et 0x791620. Chacune contient 4 cases réservées
//       + 229 pointeurs vers .text = 233 entrées, comme dans jni.h.
//    3. Après l'écriture, un script a comparé les noms et l'ordre des champs de ce fichier
//       avec jni.h, et un programme de test a vérifié les tailles et offsets ci-dessous.
//
//  Conventions :
//    - Le commentaire [n] donne l'index de l'entrée dans la table. Chaque entrée a la
//      taille d'un pointeur : offset = n * 8 en x64.
//      sizeof(JNINativeInterface_) = 233 * 8 = 1864 octets, GetStaticMethodID est à 0x388.
//    - Ne jamais ajouter, retirer ou déplacer une entrée : seul l'ordre compte.
//    - Le premier paramètre (JNIEnv* ou JavaVM*) est passé en IntPtr.
//    - JNICALL vaut __stdcall sous Windows (jni_md.h), d'où delegate* unmanaged[Stdcall].
//      En x64 il n'existe qu'une convention d'appel et ce modificateur n'a aucun effet.
//    - Les variantes « ... » (varargs) et « V » (va_list) ne peuvent pas être appelées
//      proprement depuis C#. Elles restent en void* pour garder leur place dans la table :
//      utiliser les variantes « A », qui prennent un tableau de jvalue.
//    - Types : jboolean = byte, jbyte = sbyte, jchar = ushort, jshort = short,
//      jint et jsize = int, jlong = long, jfloat = float, jdouble = double.
//      Références (jobject, jclass, jstring, jarray, jthrowable, jweak...), jmethodID et
//      jfieldID = IntPtr. const char* (UTF-8 modifié, terminé par \0) = byte*.
//    - GetModule (Java 9) et IsVirtualThread (Java 21) n'existent pas en Java 8 : ne pas
//      les ajouter tant que la cible est la JRE 8.
// =====================================================================================

using System;
using System.Runtime.InteropServices;

namespace PureJNI.Jvm
{
    public unsafe static class JniNative
    {
        // ---------------------------------------------------------------------------
        // Constantes (jni.h)
        // ---------------------------------------------------------------------------

        public const byte JNI_FALSE = 0;
        public const byte JNI_TRUE = 1;

        // Codes de retour
        public const int JNI_OK = 0;         // succès
        public const int JNI_ERR = -1;       // erreur inconnue
        public const int JNI_EDETACHED = -2; // thread non attaché à la VM
        public const int JNI_EVERSION = -3;  // version JNI non supportée
        public const int JNI_ENOMEM = -4;    // mémoire insuffisante
        public const int JNI_EEXIST = -5;    // VM déjà créée
        public const int JNI_EINVAL = -6;    // arguments invalides

        // Mode des Release<Type>ArrayElements
        public const int JNI_COMMIT = 1;
        public const int JNI_ABORT = 2;

        // Versions (GetEnv, JavaVMAttachArgs.version)
        public const int JNI_VERSION_1_1 = 0x00010001;
        public const int JNI_VERSION_1_2 = 0x00010002;
        public const int JNI_VERSION_1_4 = 0x00010004;
        public const int JNI_VERSION_1_6 = 0x00010006;
        public const int JNI_VERSION_1_8 = 0x00010008;

        // ---------------------------------------------------------------------------
        // Types annexes (jni.h)
        // ---------------------------------------------------------------------------

        // typedef union jvalue : un argument des variantes « A ». 8 octets.
        // CS8981 (nom tout en minuscules) est masqué : on garde le nom exact de jni.h.
#pragma warning disable CS8981
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct jvalue
#pragma warning restore CS8981
        {
            [FieldOffset(0)] public byte z;   // jboolean
            [FieldOffset(0)] public sbyte b;  // jbyte
            [FieldOffset(0)] public ushort c; // jchar
            [FieldOffset(0)] public short s;  // jshort
            [FieldOffset(0)] public int i;    // jint
            [FieldOffset(0)] public long j;   // jlong
            [FieldOffset(0)] public float f;  // jfloat
            [FieldOffset(0)] public double d; // jdouble
            [FieldOffset(0)] public IntPtr l; // jobject
        }

        public enum jobjectRefType : int
        {
            JNIInvalidRefType = 0,
            JNILocalRefType = 1,
            JNIGlobalRefType = 2,
            JNIWeakGlobalRefType = 3,
        }

        // Pour RegisterNatives
        public struct JNINativeMethod
        {
            public byte* name;
            public byte* signature;
            public void* fnPtr;
        }

        // Troisième argument (optionnel) de AttachCurrentThread / AttachCurrentThreadAsDaemon
        public struct JavaVMAttachArgs
        {
            public int version;
            public byte* name;
            public IntPtr group; // jobject (ThreadGroup)
        }

        // ---------------------------------------------------------------------------
        // JNIEnv / JavaVM : un pointeur vers la table de fonctions
        // ---------------------------------------------------------------------------

        public struct JNIEnv
        {
            public JNINativeInterface_* functions;
        }

        public struct JavaVM
        {
            public JNIInvokeInterface_* functions;
        }

        // ---------------------------------------------------------------------------
        // struct JNINativeInterface_ : 233 entrées (Java 8)
        // ---------------------------------------------------------------------------
        public struct JNINativeInterface_
        {
            public void* reserved0; // [0]
            public void* reserved1; // [1]
            public void* reserved2; // [2]
            public void* reserved3; // [3]

            public delegate* unmanaged[Stdcall]<IntPtr, int> GetVersion;                                    // [4]

            public delegate* unmanaged[Stdcall]<IntPtr, byte*, IntPtr, sbyte*, int, IntPtr> DefineClass;   // [5]  (name, loader, buf, len)
            public delegate* unmanaged[Stdcall]<IntPtr, byte*, IntPtr> FindClass;                          // [6]

            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr> FromReflectedMethod;               // [7]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr> FromReflectedField;                // [8]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, byte, IntPtr> ToReflectedMethod;   // [9]  (cls, methodID, isStatic)

            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr> GetSuperclass;                     // [10]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, byte> IsAssignableFrom;            // [11] (sub, sup)

            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, byte, IntPtr> ToReflectedField;    // [12] (cls, fieldID, isStatic)

            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int> Throw;                                // [13]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, byte*, int> ThrowNew;                      // [14] (clazz, msg)
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr> ExceptionOccurred;                         // [15]
            public delegate* unmanaged[Stdcall]<IntPtr, void> ExceptionDescribe;                           // [16]
            public delegate* unmanaged[Stdcall]<IntPtr, void> ExceptionClear;                              // [17]
            public delegate* unmanaged[Stdcall]<IntPtr, byte*, void> FatalError;                           // [18]

            public delegate* unmanaged[Stdcall]<IntPtr, int, int> PushLocalFrame;                          // [19]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr> PopLocalFrame;                     // [20]

            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr> NewGlobalRef;                      // [21]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, void> DeleteGlobalRef;                     // [22]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, void> DeleteLocalRef;                      // [23]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, byte> IsSameObject;                // [24]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr> NewLocalRef;                       // [25]
            public delegate* unmanaged[Stdcall]<IntPtr, int, int> EnsureLocalCapacity;                     // [26]

            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr> AllocObject;                       // [27]
            public void* NewObject;                                                                         // [28] varargs
            public void* NewObjectV;                                                                        // [29] va_list
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, IntPtr> NewObjectA;       // [30] (clazz, methodID, args)

            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr> GetObjectClass;                    // [31]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, byte> IsInstanceOf;                // [32] (obj, clazz)

            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, byte*, byte*, IntPtr> GetMethodID;         // [33] (clazz, name, sig)

            // --- Méthodes d'instance : (obj, methodID, args) ---
            public void* CallObjectMethod;                                                                  // [34]
            public void* CallObjectMethodV;                                                                 // [35]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, IntPtr> CallObjectMethodA; // [36]

            public void* CallBooleanMethod;                                                                 // [37]
            public void* CallBooleanMethodV;                                                                // [38]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, byte> CallBooleanMethodA; // [39]

            public void* CallByteMethod;                                                                    // [40]
            public void* CallByteMethodV;                                                                   // [41]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, sbyte> CallByteMethodA;   // [42]

            public void* CallCharMethod;                                                                    // [43]
            public void* CallCharMethodV;                                                                   // [44]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, ushort> CallCharMethodA;  // [45]

            public void* CallShortMethod;                                                                   // [46]
            public void* CallShortMethodV;                                                                  // [47]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, short> CallShortMethodA;  // [48]

            public void* CallIntMethod;                                                                     // [49]
            public void* CallIntMethodV;                                                                    // [50]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, int> CallIntMethodA;      // [51]

            public void* CallLongMethod;                                                                    // [52]
            public void* CallLongMethodV;                                                                   // [53]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, long> CallLongMethodA;    // [54]

            public void* CallFloatMethod;                                                                   // [55]
            public void* CallFloatMethodV;                                                                  // [56]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, float> CallFloatMethodA;  // [57]

            public void* CallDoubleMethod;                                                                  // [58]
            public void* CallDoubleMethodV;                                                                 // [59]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, double> CallDoubleMethodA; // [60]

            public void* CallVoidMethod;                                                                    // [61]
            public void* CallVoidMethodV;                                                                   // [62]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, void> CallVoidMethodA;    // [63]

            // --- Méthodes non virtuelles : (obj, clazz, methodID, args) ---
            // C'est ce bloc qui manquait dans l'ancienne version.
            public void* CallNonvirtualObjectMethod;                                                                         // [64]
            public void* CallNonvirtualObjectMethodV;                                                                        // [65]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, IntPtr, jvalue*, IntPtr> CallNonvirtualObjectMethodA; // [66]

            public void* CallNonvirtualBooleanMethod;                                                                        // [67]
            public void* CallNonvirtualBooleanMethodV;                                                                       // [68]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, IntPtr, jvalue*, byte> CallNonvirtualBooleanMethodA; // [69]

            public void* CallNonvirtualByteMethod;                                                                           // [70]
            public void* CallNonvirtualByteMethodV;                                                                          // [71]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, IntPtr, jvalue*, sbyte> CallNonvirtualByteMethodA;  // [72]

            public void* CallNonvirtualCharMethod;                                                                           // [73]
            public void* CallNonvirtualCharMethodV;                                                                          // [74]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, IntPtr, jvalue*, ushort> CallNonvirtualCharMethodA; // [75]

            public void* CallNonvirtualShortMethod;                                                                          // [76]
            public void* CallNonvirtualShortMethodV;                                                                         // [77]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, IntPtr, jvalue*, short> CallNonvirtualShortMethodA; // [78]

            public void* CallNonvirtualIntMethod;                                                                            // [79]
            public void* CallNonvirtualIntMethodV;                                                                           // [80]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, IntPtr, jvalue*, int> CallNonvirtualIntMethodA;     // [81]

            public void* CallNonvirtualLongMethod;                                                                           // [82]
            public void* CallNonvirtualLongMethodV;                                                                          // [83]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, IntPtr, jvalue*, long> CallNonvirtualLongMethodA;   // [84]

            public void* CallNonvirtualFloatMethod;                                                                          // [85]
            public void* CallNonvirtualFloatMethodV;                                                                         // [86]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, IntPtr, jvalue*, float> CallNonvirtualFloatMethodA; // [87]

            public void* CallNonvirtualDoubleMethod;                                                                         // [88]
            public void* CallNonvirtualDoubleMethodV;                                                                        // [89]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, IntPtr, jvalue*, double> CallNonvirtualDoubleMethodA; // [90]

            public void* CallNonvirtualVoidMethod;                                                                           // [91]
            public void* CallNonvirtualVoidMethodV;                                                                          // [92]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, IntPtr, jvalue*, void> CallNonvirtualVoidMethodA;   // [93]

            // --- Champs d'instance : (obj, fieldID [, value]) ---
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, byte*, byte*, IntPtr> GetFieldID;          // [94] (clazz, name, sig)

            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, IntPtr> GetObjectField;            // [95]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, byte> GetBooleanField;             // [96]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, sbyte> GetByteField;               // [97]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, ushort> GetCharField;              // [98]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, short> GetShortField;              // [99]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, int> GetIntField;                  // [100]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, long> GetLongField;                // [101]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, float> GetFloatField;              // [102]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, double> GetDoubleField;            // [103]

            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, IntPtr, void> SetObjectField;      // [104]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, byte, void> SetBooleanField;       // [105]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, sbyte, void> SetByteField;         // [106]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, ushort, void> SetCharField;        // [107]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, short, void> SetShortField;        // [108]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, int, void> SetIntField;            // [109]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, long, void> SetLongField;          // [110]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, float, void> SetFloatField;        // [111]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, double, void> SetDoubleField;      // [112]

            // --- Méthodes statiques : (clazz, methodID, args) ---
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, byte*, byte*, IntPtr> GetStaticMethodID;   // [113] (clazz, name, sig)

            public void* CallStaticObjectMethod;                                                                  // [114]
            public void* CallStaticObjectMethodV;                                                                 // [115]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, IntPtr> CallStaticObjectMethodA; // [116]

            public void* CallStaticBooleanMethod;                                                                 // [117]
            public void* CallStaticBooleanMethodV;                                                                // [118]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, byte> CallStaticBooleanMethodA; // [119]

            public void* CallStaticByteMethod;                                                                    // [120]
            public void* CallStaticByteMethodV;                                                                   // [121]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, sbyte> CallStaticByteMethodA;   // [122]

            public void* CallStaticCharMethod;                                                                    // [123]
            public void* CallStaticCharMethodV;                                                                   // [124]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, ushort> CallStaticCharMethodA;  // [125]

            public void* CallStaticShortMethod;                                                                   // [126]
            public void* CallStaticShortMethodV;                                                                  // [127]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, short> CallStaticShortMethodA;  // [128]

            public void* CallStaticIntMethod;                                                                     // [129]
            public void* CallStaticIntMethodV;                                                                    // [130]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, int> CallStaticIntMethodA;      // [131]

            public void* CallStaticLongMethod;                                                                    // [132]
            public void* CallStaticLongMethodV;                                                                   // [133]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, long> CallStaticLongMethodA;    // [134]

            public void* CallStaticFloatMethod;                                                                   // [135]
            public void* CallStaticFloatMethodV;                                                                  // [136]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, float> CallStaticFloatMethodA;  // [137]

            public void* CallStaticDoubleMethod;                                                                  // [138]
            public void* CallStaticDoubleMethodV;                                                                 // [139]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, double> CallStaticDoubleMethodA; // [140]

            public void* CallStaticVoidMethod;                                                                    // [141]
            public void* CallStaticVoidMethodV;                                                                   // [142]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, jvalue*, void> CallStaticVoidMethodA;    // [143]

            // --- Champs statiques : (clazz, fieldID [, value]) ---
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, byte*, byte*, IntPtr> GetStaticFieldID;    // [144] (clazz, name, sig)

            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, IntPtr> GetStaticObjectField;      // [145]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, byte> GetStaticBooleanField;       // [146]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, sbyte> GetStaticByteField;         // [147]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, ushort> GetStaticCharField;        // [148]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, short> GetStaticShortField;        // [149]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, int> GetStaticIntField;            // [150]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, long> GetStaticLongField;          // [151]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, float> GetStaticFloatField;        // [152]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, double> GetStaticDoubleField;      // [153]

            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, IntPtr, void> SetStaticObjectField; // [154]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, byte, void> SetStaticBooleanField;  // [155]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, sbyte, void> SetStaticByteField;    // [156]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, ushort, void> SetStaticCharField;   // [157]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, short, void> SetStaticShortField;   // [158]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, int, void> SetStaticIntField;       // [159]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, long, void> SetStaticLongField;     // [160]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, float, void> SetStaticFloatField;   // [161]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, double, void> SetStaticDoubleField; // [162]

            // --- Chaînes ---
            public delegate* unmanaged[Stdcall]<IntPtr, ushort*, int, IntPtr> NewString;                   // [163] (unicode, len)
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int> GetStringLength;                      // [164]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, byte*, ushort*> GetStringChars;            // [165] (str, isCopy)
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, ushort*, void> ReleaseStringChars;         // [166] (str, chars)

            public delegate* unmanaged[Stdcall]<IntPtr, byte*, IntPtr> NewStringUTF;                       // [167]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int> GetStringUTFLength;                   // [168]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, byte*, byte*> GetStringUTFChars;           // [169] (str, isCopy)
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, byte*, void> ReleaseStringUTFChars;        // [170] (str, chars)

            // --- Tableaux ---
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int> GetArrayLength;                       // [171]

            public delegate* unmanaged[Stdcall]<IntPtr, int, IntPtr, IntPtr, IntPtr> NewObjectArray;       // [172] (len, clazz, init)
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, IntPtr> GetObjectArrayElement;        // [173] (array, index)
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, IntPtr, void> SetObjectArrayElement;  // [174] (array, index, val)

            public delegate* unmanaged[Stdcall]<IntPtr, int, IntPtr> NewBooleanArray;                      // [175]
            public delegate* unmanaged[Stdcall]<IntPtr, int, IntPtr> NewByteArray;                         // [176]
            public delegate* unmanaged[Stdcall]<IntPtr, int, IntPtr> NewCharArray;                         // [177]
            public delegate* unmanaged[Stdcall]<IntPtr, int, IntPtr> NewShortArray;                        // [178]
            public delegate* unmanaged[Stdcall]<IntPtr, int, IntPtr> NewIntArray;                          // [179]
            public delegate* unmanaged[Stdcall]<IntPtr, int, IntPtr> NewLongArray;                         // [180]
            public delegate* unmanaged[Stdcall]<IntPtr, int, IntPtr> NewFloatArray;                        // [181]
            public delegate* unmanaged[Stdcall]<IntPtr, int, IntPtr> NewDoubleArray;                       // [182]

            // (array, isCopy)
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, byte*, byte*> GetBooleanArrayElements;     // [183]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, byte*, sbyte*> GetByteArrayElements;       // [184]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, byte*, ushort*> GetCharArrayElements;      // [185]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, byte*, short*> GetShortArrayElements;      // [186]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, byte*, int*> GetIntArrayElements;          // [187]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, byte*, long*> GetLongArrayElements;        // [188]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, byte*, float*> GetFloatArrayElements;      // [189]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, byte*, double*> GetDoubleArrayElements;    // [190]

            // (array, elems, mode) ; mode = 0, JNI_COMMIT ou JNI_ABORT
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, byte*, int, void> ReleaseBooleanArrayElements;  // [191]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, sbyte*, int, void> ReleaseByteArrayElements;    // [192]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, ushort*, int, void> ReleaseCharArrayElements;   // [193]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, short*, int, void> ReleaseShortArrayElements;   // [194]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int*, int, void> ReleaseIntArrayElements;       // [195]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, long*, int, void> ReleaseLongArrayElements;     // [196]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, float*, int, void> ReleaseFloatArrayElements;   // [197]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, double*, int, void> ReleaseDoubleArrayElements; // [198]

            // (array, start, len, buf)
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, int, byte*, void> GetBooleanArrayRegion;  // [199]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, int, sbyte*, void> GetByteArrayRegion;    // [200]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, int, ushort*, void> GetCharArrayRegion;   // [201]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, int, short*, void> GetShortArrayRegion;   // [202]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, int, int*, void> GetIntArrayRegion;       // [203]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, int, long*, void> GetLongArrayRegion;     // [204]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, int, float*, void> GetFloatArrayRegion;   // [205]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, int, double*, void> GetDoubleArrayRegion; // [206]

            // (array, start, len, buf)
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, int, byte*, void> SetBooleanArrayRegion;  // [207]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, int, sbyte*, void> SetByteArrayRegion;    // [208]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, int, ushort*, void> SetCharArrayRegion;   // [209]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, int, short*, void> SetShortArrayRegion;   // [210]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, int, int*, void> SetIntArrayRegion;       // [211]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, int, long*, void> SetLongArrayRegion;     // [212]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, int, float*, void> SetFloatArrayRegion;   // [213]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, int, double*, void> SetDoubleArrayRegion; // [214]

            // --- Méthodes natives, moniteurs, VM ---
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, JNINativeMethod*, int, int> RegisterNatives; // [215] (clazz, methods, nMethods)
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int> UnregisterNatives;                      // [216]

            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int> MonitorEnter;                         // [217]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int> MonitorExit;                          // [218]

            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr*, int> GetJavaVM;                           // [219] (JavaVM** vm)

            // --- Ajouts JNI 1.2 ---
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, int, ushort*, void> GetStringRegion;  // [220] (str, start, len, buf)
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, int, byte*, void> GetStringUTFRegion; // [221] (str, start, len, buf)

            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, byte*, void*> GetPrimitiveArrayCritical;        // [222] (array, isCopy)
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, void*, int, void> ReleasePrimitiveArrayCritical; // [223] (array, carray, mode)

            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, byte*, ushort*> GetStringCritical;         // [224] (string, isCopy)
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, ushort*, void> ReleaseStringCritical;      // [225] (string, cstring)

            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr> NewWeakGlobalRef;                  // [226]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, void> DeleteWeakGlobalRef;                 // [227]

            public delegate* unmanaged[Stdcall]<IntPtr, byte> ExceptionCheck;                              // [228]

            // --- Ajouts JNI 1.4 ---
            public delegate* unmanaged[Stdcall]<IntPtr, void*, long, IntPtr> NewDirectByteBuffer;         // [229] (address, capacity)
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, void*> GetDirectBufferAddress;             // [230]
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, long> GetDirectBufferCapacity;             // [231]

            // --- Ajout JNI 1.6 ---
            public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, jobjectRefType> GetObjectRefType;          // [232]
        }

        // ---------------------------------------------------------------------------
        // struct JNIInvokeInterface_ : table de JavaVM, 8 entrées
        // ---------------------------------------------------------------------------
        public struct JNIInvokeInterface_
        {
            public void* reserved0; // [0]
            public void* reserved1; // [1]
            public void* reserved2; // [2]

            public delegate* unmanaged[Stdcall]<IntPtr, int> DestroyJavaVM;                                // [3]
            public delegate* unmanaged[Stdcall]<IntPtr, void**, void*, int> AttachCurrentThread;           // [4] (penv, JavaVMAttachArgs* ou null)
            public delegate* unmanaged[Stdcall]<IntPtr, int> DetachCurrentThread;                          // [5]
            public delegate* unmanaged[Stdcall]<IntPtr, void**, int, int> GetEnv;                          // [6] (penv, version)
            public delegate* unmanaged[Stdcall]<IntPtr, void**, void*, int> AttachCurrentThreadAsDaemon;   // [7] (penv, JavaVMAttachArgs* ou null)
        }
    }
}
