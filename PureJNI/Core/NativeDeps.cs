using System.Reflection;
using System.Security.Cryptography;

namespace PureJNI.Core
{
    // Native DLLs are embedded in PureJNI.dll (see PureJNI.csproj) and extracted here on first use.
    // The folder name is a hash of the embedded content: a new build never overwrites DLLs that a
    // previous injection may still hold locked, and an unchanged build reuses the existing files.
    internal static class NativeDeps
    {
        private const string ResourcePrefix = "native.";
        public const string MinHook = "minhook.x64.dll";
        public const string CImGui = "cimgui.dll";
        public const string ImGuiImpl = "ImGuiImpl.dll";

        private static readonly string[] Names = { MinHook, CImGui, ImGuiImpl };

        // Extracts every embedded DLL and returns the folder that holds them.
        public static string Extract()
        {
            Assembly assembly = typeof(NativeDeps).Assembly;
            var blobs = new byte[Names.Length][];
            using (var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
            {
                for (int i = 0; i < Names.Length; i++)
                {
                    using Stream stream = assembly.GetManifestResourceStream(ResourcePrefix + Names[i])
                        ?? throw new InvalidOperationException($"embedded resource missing: {Names[i]}");
                    using var memory = new MemoryStream();
                    stream.CopyTo(memory);
                    blobs[i] = memory.ToArray();
                    hash.AppendData(blobs[i]);
                }

                string id = Convert.ToHexString(hash.GetHashAndReset(), 0, 8);
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PureJNI", id);
                Directory.CreateDirectory(dir);

                for (int i = 0; i < Names.Length; i++)
                {
                    string path = Path.Combine(dir, Names[i]);
                    if (File.Exists(path) && new FileInfo(path).Length == blobs[i].Length)
                        continue;
                    // Write to a temp file then move, so a crash never leaves a truncated DLL behind.
                    string temp = path + ".tmp";
                    File.WriteAllBytes(temp, blobs[i]);
                    File.Move(temp, path, overwrite: true);
                }
                return dir;
            }
        }
    }
}
