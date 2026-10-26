using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace Mochie {
    enum PackerSource {Red, Green, Blue, Alpha, Grayscale}
    enum PackerSampling {Bilinear, Point}
    enum PackerFormat {PNG, TGA}

    // One channel of the Texture Packer window's output: where its value comes from and how it's adjusted
    [Serializable]
    class PackerChannel {
        public string name = "";
        public Texture texture;
        public PackerSource source;
        public float fill;              // Written when there's no texture
        public bool invert;
        public float brightness = 1f;
        public float contrast = 1f;
        public float gamma = 1f;
        public Vector2 inputLevels = new Vector2(0f, 1f);
        public Vector2 outputLevels = new Vector2(0f, 1f);
        public PackerSampling sampling;

        public PackerChannel(){}

        public PackerChannel(string name, float fill, PackerSource source = PackerSource.Red){
            this.name = name;
            this.fill = fill;
            this.source = source;
        }

        public PackerChannel Clone(){
            return (PackerChannel)MemberwiseClone();
        }

        public bool HasFiltering(){
            return brightness != 1f || contrast != 1f || gamma != 1f || sampling != PackerSampling.Bilinear
                || inputLevels != new Vector2(0f, 1f) || outputLevels != new Vector2(0f, 1f);
        }

        public void ResetFiltering(){
            brightness = 1f;
            contrast = 1f;
            gamma = 1f;
            inputLevels = new Vector2(0f, 1f);
            outputLevels = new Vector2(0f, 1f);
            sampling = PackerSampling.Bilinear;
        }

        // Takes the texture and everything about how it's read, but keeps this channel's name and fill value
        public void CopySource(PackerChannel other){
            texture = other.texture;
            source = other.source;
            invert = other.invert;
            brightness = other.brightness;
            contrast = other.contrast;
            gamma = other.gamma;
            inputLevels = other.inputLevels;
            outputLevels = other.outputLevels;
            sampling = other.sampling;
        }
    }

    // A packing format: what each channel holds, plus the output settings that go with it
    [Serializable]
    class PackerPreset {
        public string name;
        public string suffix;           // Added to the default file name
        public bool sRGB;
        public bool includeAlpha;
        public PackerChannel[] channels;

        public PackerPreset(){}

        public PackerPreset(string name, string suffix, bool sRGB, bool includeAlpha, params PackerChannel[] channels){
            this.name = name;
            this.suffix = suffix;
            this.sRGB = sRGB;
            this.includeAlpha = includeAlpha;
            this.channels = channels;
        }

        public bool IsValid(){
            return !string.IsNullOrEmpty(name) && channels != null && channels.Length == 4 && channels.All(c => c != null);
        }
    }

    [Serializable]
    class PackerPresetList {
        public List<PackerPreset> presets = new List<PackerPreset>();
    }

    static class PackerPresets {
        const string PrefsKey = "Mochie.TexturePacker.Presets";

        public static readonly PackerPreset[] BuiltIn = {
            new PackerPreset("Mochie Packed Map", "_PackedMap", false, false,
                new PackerChannel("Occlusion", 1f),
                new PackerChannel("Roughness", 1f),
                new PackerChannel("Metallic", 0f),
                new PackerChannel("Height", 0.5f)
            ),
            // Uber's Packed masking mode, where white is full effect (GGX for Specular Blend)
            new PackerPreset("Uber Packed Mask 1", "_Mask1", false, false,
                new PackerChannel("Reflections", 1f),
                new PackerChannel("Specular", 1f),
                new PackerChannel("Primary Matcap", 1f),
                new PackerChannel("Secondary Matcap", 1f)
            ),
            new PackerPreset("Uber Packed Mask 2", "_Mask2", false, false,
                new PackerChannel("Shadows", 1f),
                new PackerChannel("Diffuse Shading", 1f),
                new PackerChannel("Subsurface", 1f),
                new PackerChannel("Detail Maps", 1f)
            ),
            new PackerPreset("Uber Packed Mask 3", "_Mask3", false, false,
                new PackerChannel("Basic Rim", 1f),
                new PackerChannel("Environment Rim", 1f),
                new PackerChannel("Refraction", 1f),
                new PackerChannel("Specular Blend", 1f)
            ),
            new PackerPreset("Uber Packed Mask 4", "_Mask4", false, false,
                new PackerChannel("Emission", 1f),
                new PackerChannel("Emission Pulse", 1f),
                new PackerChannel("Filtering", 1f),
                new PackerChannel("Outline Thickness", 1f)
            ),
            new PackerPreset("ORM (Unreal, glTF)", "_ORM", false, false,
                new PackerChannel("Occlusion", 1f),
                new PackerChannel("Roughness", 1f),
                new PackerChannel("Metallic", 0f),
                new PackerChannel("Alpha", 1f, PackerSource.Alpha)
            ),
            new PackerPreset("Unity Metallic Smoothness", "_MetallicSmoothness", false, true,
                new PackerChannel("Metallic", 0f),
                new PackerChannel("Unused", 0f),
                new PackerChannel("Unused", 0f),
                new PackerChannel("Smoothness", 0.5f)
            ),
            new PackerPreset("HDRP Mask Map", "_MaskMap", false, true,
                new PackerChannel("Metallic", 0f),
                new PackerChannel("Occlusion", 1f),
                new PackerChannel("Detail Mask", 1f),
                new PackerChannel("Smoothness", 0.5f)
            ),
            new PackerPreset("Terrain Mask Map", "_MaskMap", false, true,
                new PackerChannel("Metallic", 0f),
                new PackerChannel("Occlusion", 1f),
                new PackerChannel("Height", 0.5f),
                new PackerChannel("Smoothness", 0.5f)
            ),
            new PackerPreset("Color + Alpha", "_RGBA", true, true,
                new PackerChannel("Red", 1f, PackerSource.Red),
                new PackerChannel("Green", 1f, PackerSource.Green),
                new PackerChannel("Blue", 1f, PackerSource.Blue),
                new PackerChannel("Alpha", 1f, PackerSource.Alpha)
            ),
            new PackerPreset("Custom", "_Packed", false, false,
                new PackerChannel("Red", 0f, PackerSource.Red),
                new PackerChannel("Green", 0f, PackerSource.Green),
                new PackerChannel("Blue", 0f, PackerSource.Blue),
                new PackerChannel("Alpha", 1f, PackerSource.Alpha)
            )
        };

        public static PackerPreset Custom { get { return BuiltIn[BuiltIn.Length - 1]; } }

        public static bool IsBuiltIn(string name){
            return BuiltIn.Any(p => p.name == name);
        }

        public static PackerPreset Find(string name){
            return BuiltIn.FirstOrDefault(p => p.name == name) ?? LoadUser().FirstOrDefault(p => p.name == name);
        }

        // User presets are kept in EditorPrefs, so they're shared by every project on this computer
        public static List<PackerPreset> LoadUser(){
            string json = EditorPrefs.GetString(PrefsKey, "");
            if (string.IsNullOrEmpty(json))
                return new List<PackerPreset>();
            try {
                PackerPresetList list = JsonUtility.FromJson<PackerPresetList>(json);
                if (list != null && list.presets != null)
                    return list.presets.Where(p => p != null && p.IsValid()).ToList();
            }
            catch (Exception e){
                Debug.LogWarning("[Mochie] Texture Packer: couldn't read the saved presets. " + e.Message);
            }
            return new List<PackerPreset>();
        }

        public static void SaveUser(List<PackerPreset> presets){
            EditorPrefs.SetString(PrefsKey, JsonUtility.ToJson(new PackerPresetList { presets = presets }));
        }

        // Words that name each kind of map in texture names. The first word is the channel name presets use.
        static readonly string[][] MapWords = {
            new[]{"occlusion", "ao", "occ", "ambientocclusion"},
            new[]{"roughness", "rough", "rgh"},
            new[]{"smoothness", "smooth", "gloss", "glossiness"},
            new[]{"metallic", "metal", "metalness", "metalic"},
            new[]{"height", "heightmap", "disp", "displacement", "parallax"}
        };

        // How well two channel names match: 2 = the same map, 1 = roughness and smoothness (the same map inverted), 0 = no match
        public static int MatchChannel(string from, string to){
            string a = Compact(from);
            string b = Compact(to);
            if (a.Length == 0 || b.Length == 0)
                return 0;
            if (a == b)
                return 2;
            return GetOpposite(a) == b ? 1 : 0;
        }

        // How well a texture's name fits a channel: 2 = it's named after the channel's map ("Rock_AO" for Occlusion),
        // 1 = it's named after the opposite map (roughness for smoothness and the reverse), 0 = no match
        public static int MatchTexture(string textureName, string channelName){
            string key = Compact(channelName);
            if (key.Length == 0)
                return 0;
            List<string> words = GetWords(textureName);
            if (words.Intersect(GetMapWords(key)).Any())
                return 2;
            string opposite = GetOpposite(key);
            if (opposite != null && words.Intersect(GetMapWords(opposite)).Any())
                return 1;
            return 0;
        }

        // The shared start of the texture names ("Rock_AO" and "Rock_Roughness" give "Rock"), or the first name
        // without its map word ("Rock_AO" gives "Rock")
        public static string GetBaseName(IEnumerable<string> textureNames){
            List<string> names = textureNames.Distinct().ToList();
            if (names.Count == 0)
                return "Texture";
            if (names.Count > 1){
                string prefix = names.Aggregate(CommonPrefix);
                // Cut back to the last word boundary so "Rock_AO" and "Rock_Albedo" don't give "Rock_A"
                while (prefix.Length > 0 && names.Any(n => !IsWordBoundary(n, prefix.Length)))
                    prefix = prefix.Substring(0, prefix.Length - 1);
                prefix = prefix.TrimEnd('_', '-', ' ', '.');
                if (prefix.Length >= 3)
                    return prefix;
            }
            string name = names[0];
            int cut = name.LastIndexOfAny(new[]{'_', '-', ' ', '.'});
            if (cut > 0 && MapWords.Any(group => group.Contains(Compact(name.Substring(cut + 1)))))
                return name.Substring(0, cut);
            return name;
        }

        static string CommonPrefix(string a, string b){
            int length = 0;
            while (length < a.Length && length < b.Length && a[length] == b[length])
                length++;
            return a.Substring(0, length);
        }

        // True if a word ends at index in name: at a separator, the end, or a camelCase hump
        static bool IsWordBoundary(string name, int index){
            if (index <= 0 || index >= name.Length)
                return true;
            char before = name[index - 1];
            char after = name[index];
            return !char.IsLetterOrDigit(before) || !char.IsLetterOrDigit(after) || (char.IsLower(before) && char.IsUpper(after));
        }

        static string GetOpposite(string key){
            if (key == "roughness")
                return "smoothness";
            if (key == "smoothness")
                return "roughness";
            return null;
        }

        static IEnumerable<string> GetMapWords(string key){
            return MapWords.FirstOrDefault(group => group.Contains(key)) ?? new[]{key};
        }

        // Splits a texture name into lowercase words, plus each pair of neighbors joined so two-word names like
        // "Detail Mask" match. "T_RockAO_DetailMask" gives t, rock, ao, detail, mask, trock, rockao, aodetail, detailmask.
        static List<string> GetWords(string name){
            List<string> words = new List<string>();
            StringBuilder word = new StringBuilder();
            for (int i = 0; i <= name.Length; i++){
                bool end = i == name.Length || !char.IsLetterOrDigit(name[i]);
                bool hump = !end && i > 0 && char.IsUpper(name[i]) && char.IsLower(name[i - 1]);
                if ((end || hump) && word.Length > 0){
                    words.Add(word.ToString());
                    word.Clear();
                }
                if (!end)
                    word.Append(char.ToLowerInvariant(name[i]));
            }
            int count = words.Count;
            for (int i = 0; i < count - 1; i++)
                words.Add(words[i] + words[i + 1]);
            return words;
        }

        static string Compact(string name){
            if (string.IsNullOrEmpty(name))
                return "";
            return new string(name.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
        }
    }
}
