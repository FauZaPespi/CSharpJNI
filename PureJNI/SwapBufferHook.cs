using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

public unsafe static class SwapBufferHook
{
    // Delegate for the original function trampoline
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public delegate IntPtr WglSwapBuffersDelegate(IntPtr hDc);

    private static IntPtr _originalSwapBuffersPtr = IntPtr.Zero;
    private static WglSwapBuffersDelegate _originalSwapBuffers;
    private static bool _isImGuiInitialized = false;

    // Entry point called when the DLL is injected
    [UnmanagedCallersOnly(EntryPoint = "DllMain")]
    public static bool DllMain(nint module, uint reason, nint reserved)
    {
        // DLL_PROCESS_ATTACH = 1
        if (reason == 1)
        {
            // Start initialization on a separate thread to avoid loader lock
            System.Threading.Thread thread = new System.Threading.Thread(InitializationThread);
            thread.Start();
        }
        return true;
    }

    private static void InitializationThread()
    {
        // 1. Initialize MinHook and hook wglSwapBuffers here
        // (Ensure MinHook.x64.dll is next to your DLL or injected properly)
    }

    // The detour function called every frame by the game's render loop
    [UnmanagedCallersOnly] // Omit CallConvs for x64 Native AOT compatibility
    public static IntPtr HookedWglSwapBuffers(IntPtr hDc)
    {
        if (!_isImGuiInitialized)
        {
            // TODO: Initialize your ImGui context and OpenGL bindings here on frame 1
            _isImGuiInitialized = true;
        }

        try
        {
            // --- ImGui Frame Rendering ---
            // ImGui.NewFrame();
            // Your UI code here...
            // ImGui.Render();
            // ImGui_ImplOpenGL2_RenderDrawData(ImGui.GetDrawData());
            // -----------------------------
        }
        catch (Exception)
        {
            // Prevent managed exceptions from crashing javaw.exe
        }

        // Lazy-load the original delegate from the function pointer stored by MinHook
        if (_originalSwapBuffers == null && _originalSwapBuffersPtr != IntPtr.Zero)
        {
            _originalSwapBuffers = Marshal.GetDelegateForFunctionPointer<WglSwapBuffersDelegate>(_originalSwapBuffersPtr);
        }

        // Call the original function so the game continues rendering normally
        return _originalSwapBuffers != null ? _originalSwapBuffers(hDc) : hDc;
    }

    // Method called by your MinHook setup to store the original function pointer
    public static void SetOriginal(IntPtr originalPtr)
    {
        _originalSwapBuffersPtr = originalPtr;
    }
}