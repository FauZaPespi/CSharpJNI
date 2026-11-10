using PureJNI.Jvm;
using M = PureJNI.Game.Mappings;

namespace PureJNI.Game
{
    // Classes (global refs) and field/method IDs, resolved once.
    // IDs and global refs are valid on every thread, so the worker and the render thread share them.
    public static unsafe class GameClasses
    {
        public static IntPtr Minecraft, Timer, Session, Entity, Mouse;

        public static IntPtr MinecraftGetMinecraft, MinecraftGetDebugFps, MinecraftGetSession, MinecraftThePlayer, MinecraftTimer;
        public static IntPtr TimerSpeed;
        public static IntPtr SessionGetUsername;
        public static IntPtr EntityPosX, EntityPosY, EntityPosZ;
        public static IntPtr MouseSetGrabbed, MouseIsGrabbed;

        private static readonly object InitLock = new();
        private static volatile bool _ready;
        public static bool Ready => _ready;

        // Returns false (and logs what is missing) when a mapping does not resolve.
        public static bool Init(JniNative.JNIEnv* env)
        {
            lock (InitLock)
            {
                if (_ready) return true;
                bool ok = true;

                Minecraft = Class(env, M.Minecraft.Class, ref ok);
                Timer = Class(env, M.Timer.Class, ref ok);
                Session = Class(env, M.Session.Class, ref ok);
                Entity = Class(env, M.Entity.Class, ref ok);
                Mouse = Class(env, M.LwjglMouse.Class, ref ok);
                if (!ok) return false;

                MinecraftGetMinecraft = Check(Jni.GetStaticMethodID(env, Minecraft, M.Minecraft.GetMinecraft, M.Minecraft.GetMinecraftSig), "Minecraft.getMinecraft", ref ok);
                MinecraftGetDebugFps = Check(Jni.GetStaticMethodID(env, Minecraft, M.Minecraft.GetDebugFps, M.Minecraft.GetDebugFpsSig), "Minecraft.getDebugFPS", ref ok);
                MinecraftGetSession = Check(Jni.GetMethodID(env, Minecraft, M.Minecraft.GetSession, M.Minecraft.GetSessionSig), "Minecraft.getSession", ref ok);
                MinecraftThePlayer = Check(Jni.GetFieldID(env, Minecraft, M.Minecraft.ThePlayer, M.Minecraft.ThePlayerSig), "Minecraft.thePlayer", ref ok);
                MinecraftTimer = Check(Jni.GetFieldID(env, Minecraft, M.Minecraft.Timer, M.Minecraft.TimerSig), "Minecraft.timer", ref ok);
                TimerSpeed = Check(Jni.GetFieldID(env, Timer, M.Timer.TimerSpeed, M.Timer.TimerSpeedSig), "Timer.timerSpeed", ref ok);
                SessionGetUsername = Check(Jni.GetMethodID(env, Session, M.Session.GetUsername, M.Session.GetUsernameSig), "Session.getUsername", ref ok);
                EntityPosX = Check(Jni.GetFieldID(env, Entity, M.Entity.PosX, M.Entity.PosSig), "Entity.posX", ref ok);
                EntityPosY = Check(Jni.GetFieldID(env, Entity, M.Entity.PosY, M.Entity.PosSig), "Entity.posY", ref ok);
                EntityPosZ = Check(Jni.GetFieldID(env, Entity, M.Entity.PosZ, M.Entity.PosSig), "Entity.posZ", ref ok);
                MouseSetGrabbed = Check(Jni.GetStaticMethodID(env, Mouse, M.LwjglMouse.SetGrabbed, M.LwjglMouse.SetGrabbedSig), "Mouse.setGrabbed", ref ok);
                MouseIsGrabbed = Check(Jni.GetStaticMethodID(env, Mouse, M.LwjglMouse.IsGrabbed, M.LwjglMouse.IsGrabbedSig), "Mouse.isGrabbed", ref ok);

                _ready = ok;
                return ok;
            }
        }

        private static IntPtr Class(JniNative.JNIEnv* env, string name, ref bool ok)
            => Check(Jni.FindClassGlobal(env, name), "class " + name, ref ok);

        private static IntPtr Check(IntPtr value, string what, ref bool ok)
        {
            if (value == IntPtr.Zero)
            {
                Core.Log.Error($"mapping not found: {what}");
                ok = false;
            }
            return value;
        }
    }
}
