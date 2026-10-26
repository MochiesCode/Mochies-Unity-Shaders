using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    /// <summary>
    /// Reads the material version stamp. Every Mochie shader has a hidden <see cref="PropertyName"/> property whose default is the
    /// current package version (see <see cref="Encode"/>), so new materials are stamped automatically. Unity also writes that default into
    /// old materials whenever it saves them, so the in-memory value can't be trusted. The stamp that counts is the one in the .mat file on disk.
    /// </summary>
    public static class MaterialVersionStamp
    {
        public const string PropertyName = "_MochieMaterialVersion";

        public enum ReadResult
        {
            /// <summary>The file has a stamp</summary>
            Found,
            /// <summary>The file has no stamp, so it was last saved before v1.78 (version 0)</summary>
            Missing,
            /// <summary>No text .mat file to read (not saved yet, a sub-asset or binary serialized). Treat as up to date.</summary>
            Unreadable
        }

        static readonly Regex StampExtractor = new Regex($@"^\s*-\s*{PropertyName}:\s*(-?\d+(?:\.\d+)?)\s*$");
        static readonly Regex VersionParser = new Regex(@"^v?(\d+)\.(\d+)(?:\.(\d+))?$");

        /// <summary>
        /// Encodes a package version as <c>major * 10000 + minor * 100 + patch</c>, e.g. "1.78" is 17800 and "v1.77.1" is 17701.
        /// Kept as an integer so versions compare correctly (v1.8 is older than v1.78) and floats store them exactly.
        /// </summary>
        public static int Encode(string version)
        {
            var match = VersionParser.Match(version ?? "");
            int minor = match.Success ? int.Parse(match.Groups[2].Value) : 0;
            int patch = match.Success && match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;
            if(!match.Success || minor > 99 || patch > 99)
                throw new ArgumentException($"'{version}' isn't a version like 1.79 or 1.79.1", nameof(version));

            return int.Parse(match.Groups[1].Value) * 10000 + minor * 100 + patch;
        }

        /// <summary>
        /// Current package version, taken from the stamp property's default in the shader.
        /// </summary>
        /// <returns>False if the shader has no stamp property</returns>
        public static bool TryGetCurrentVersion(Shader shader, out int version)
        {
            version = 0;
            if(shader == null)
                return false;

            int index = shader.FindPropertyIndex(PropertyName);
            if(index < 0)
                return false;

            version = Mathf.RoundToInt(shader.GetPropertyDefaultFloatValue(index));
            return true;
        }

        /// <summary>
        /// Version stamped in the material's .mat file on disk. Missing stamps return version 0.
        /// </summary>
        public static ReadResult ReadFromDisk(Material material, out int version)
        {
            version = 0;
            string assetPath = AssetDatabase.GetAssetPath(material);
            if(string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".mat", StringComparison.OrdinalIgnoreCase) || !AssetDatabase.IsMainAsset(material))
                return ReadResult.Unreadable;

            string fullPath = FileUtil.GetPhysicalPath(assetPath);
            if(string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
                return ReadResult.Unreadable;

            try
            {
                using(var reader = new StreamReader(fullPath))
                {
                    string line = reader.ReadLine();
                    if(line == null || !line.StartsWith("%YAML", StringComparison.Ordinal))
                        return ReadResult.Unreadable;

                    while((line = reader.ReadLine()) != null)
                    {
                        var match = StampExtractor.Match(line);
                        if(match.Success)
                        {
                            version = Mathf.RoundToInt(float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture));
                            return ReadResult.Found;
                        }
                    }
                }
            }
#if MOCHIE_DEV
            catch(Exception ex)
            {
                Debug.LogWarning($"Couldn't read material file '{fullPath}': {ex.Message}");
                return ReadResult.Unreadable;
            }
#else
            catch(Exception)
            {
                return ReadResult.Unreadable;
            }
#endif

            return ReadResult.Missing;
        }
    }
}
