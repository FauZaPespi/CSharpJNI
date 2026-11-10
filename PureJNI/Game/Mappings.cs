namespace PureJNI.Game
{
    // Every obfuscated name used by the client (Minecraft 1.8.9, notch names).
    // Change them here only: the wrappers never hardcode a name.
    public static class Mappings
    {
        public static class Minecraft
        {
            public const string Class = "ave";
            public const string GetMinecraft = "A", GetMinecraftSig = "()Lave;";      // static Minecraft getMinecraft()
            public const string GetDebugFps = "ai", GetDebugFpsSig = "()I";          // static int getDebugFPS()
            public const string GetSession = "L", GetSessionSig = "()Lavm;";         // Session getSession()
            public const string ThePlayer = "h", ThePlayerSig = "Lbew;";             // EntityPlayerSP thePlayer
            public const string Timer = "Y", TimerSig = "Lavl;";                     // Timer timer
        }

        public static class Timer
        {
            public const string Class = "avl";
            public const string TimerSpeed = "d", TimerSpeedSig = "F";               // float timerSpeed
        }

        public static class Session
        {
            public const string Class = "avm";
            public const string GetUsername = "c", GetUsernameSig = "()Ljava/lang/String;";
        }

        public static class Entity
        {
            public const string Class = "pk";
            public const string PosX = "s", PosY = "t", PosZ = "u", PosSig = "D";    // double posX/posY/posZ
        }

        // LWJGL 2 is not obfuscated.
        public static class LwjglMouse
        {
            public const string Class = "org/lwjgl/input/Mouse";
            public const string SetGrabbed = "setGrabbed", SetGrabbedSig = "(Z)V";
            public const string IsGrabbed = "isGrabbed", IsGrabbedSig = "()Z";
        }
    }
}
