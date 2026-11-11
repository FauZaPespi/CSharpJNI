namespace PureJNI.Core
{
    public static class Log
    {
        private static readonly object Lock = new();

        public static void Info(string message) => Write("INFO ", message);
        public static void Error(string message) => Write("ERROR", message);

        private static void Write(string level, string message)
        {
            lock (Lock)
            {
                try { Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {level} {message}"); }
                catch { /* no console: ignore */ }
            }
        }
    }
}
