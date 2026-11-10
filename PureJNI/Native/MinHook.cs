using System.Runtime.InteropServices;

namespace PureJNI.Native
{
    // MinHook bound through function pointers resolved from an absolute path:
    // inside javaw.exe the DLL search path is Java's folder, not ours.
    internal static unsafe class MinHook
    {
        public const int MH_OK = 0;
        public const int MH_ERROR_ALREADY_INITIALIZED = 1;

        private static delegate* unmanaged<int> _initialize;
        private static delegate* unmanaged<IntPtr, IntPtr, IntPtr*, int> _createHook;
        private static delegate* unmanaged<IntPtr, int> _enableHook;
        private static delegate* unmanaged<IntPtr, int> _disableHook;
        private static delegate* unmanaged<int> _uninitialize;

        public static void Load(string path)
        {
            IntPtr lib = NativeLibrary.Load(path);
            _initialize = (delegate* unmanaged<int>)NativeLibrary.GetExport(lib, "MH_Initialize");
            _createHook = (delegate* unmanaged<IntPtr, IntPtr, IntPtr*, int>)NativeLibrary.GetExport(lib, "MH_CreateHook");
            _enableHook = (delegate* unmanaged<IntPtr, int>)NativeLibrary.GetExport(lib, "MH_EnableHook");
            _disableHook = (delegate* unmanaged<IntPtr, int>)NativeLibrary.GetExport(lib, "MH_DisableHook");
            _uninitialize = (delegate* unmanaged<int>)NativeLibrary.GetExport(lib, "MH_Uninitialize");
        }

        public static int Initialize() => _initialize();
        public static int CreateHook(IntPtr target, IntPtr detour, out IntPtr original)
        {
            IntPtr o;
            int r = _createHook(target, detour, &o);
            original = o;
            return r;
        }
        public static int EnableHook(IntPtr target) => _enableHook(target);
        public static int DisableHook(IntPtr target) => _disableHook(target);
        public static int Uninitialize() => _uninitialize();
    }
}
