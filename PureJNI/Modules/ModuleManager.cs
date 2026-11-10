using PureJNI.Core;
using PureJNI.Game;
using PureJNI.Jvm;
using PureJNI.Modules.Movement;
using PureJNI.Modules.Render;

namespace PureJNI.Modules
{
    public static unsafe class ModuleManager
    {
        // Register modules here. No reflection: NativeAOT friendly.
        public static readonly Module[] All =
        {
            new TimerModule(),
            new InfoModule(),
        };

        private const int TickIntervalMs = 50; // 20 ticks/s, like Minecraft

        private static volatile bool _running;

        // Worker loop. Runs on its own thread, attached to the JVM; returns when Stop() is called.
        public static void Run(JniNative.JNIEnv* env)
        {
            _running = true;
            while (_running)
            {
                Tick(env);
                Thread.Sleep(TickIntervalMs);
            }
            // Last tick with every module disabled, so OnDisable restores game state (timer back to 1).
            foreach (var module in All)
                module.Enabled = false;
            Tick(env);
        }

        public static void Stop() => _running = false;

        private static void Tick(JniNative.JNIEnv* env)
        {
            // Every local ref created during the tick is freed by PopLocalFrame.
            if (env->functions->PushLocalFrame((IntPtr)env, 64) != 0)
            {
                Jni.ClearException(env);
                return;
            }
            try
            {
                var mc = Minecraft.Get(env);
                if (!mc.IsValid)
                    return;

                foreach (var module in All)
                {
                    try
                    {
                        module.Update(mc);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"[{module.Name}] {e.Message}");
                    }
                    // A pending Java exception makes every later JNI call undefined.
                    if (Jni.ClearException(env))
                        Log.Error($"[{module.Name}] Java exception cleared");
                }
            }
            finally
            {
                env->functions->PopLocalFrame((IntPtr)env, IntPtr.Zero);
            }
        }
    }
}
