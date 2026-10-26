using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Mochie {
    public static class TexturePacker {

        // Describes how one channel of the packed texture is generated
        public struct Channel {
            public Texture texture;         // Source texture, or null to fill the channel with fallback
            public int sourceChannel;       // Channel read from the source (0 = R, 1 = G, 2 = B, 3 = A)
            public float strength;          // Multiplies the sampled value, or lerps it from 1 when isOcclusion is set
            public float fallback;          // Value written when there is no source texture
            public bool isOcclusion;
            public bool invert;
            public Vector4 scaleOffset;     // Tiling and offset baked into the packed texture

            public Channel(Texture texture, int sourceChannel = 0, float strength = 1f, float fallback = 1f, bool isOcclusion = false){
                this.texture = texture;
                this.sourceChannel = sourceChannel;
                this.strength = strength;
                this.fallback = fallback;
                this.isOcclusion = isOcclusion;
                invert = false;
                scaleOffset = new Vector4(1f, 1f, 0f, 0f);
            }
        }

        const string TempFolderName = "__MochieTexturePackerTemp";
        const string TempFolder = "Assets/" + TempFolderName;
        const string ImporterTag = "MochieTexturePacker";

        // Packs the channels into a PNG saved next to the material and assigns it to packedMapName.
        // The alpha channel is only written when it has a texture. Returns the saved texture, or null on failure.
        public static Texture2D PackTextures(Material mat, string packedMapName, Channel red, Channel green, Channel blue, Channel alpha = default){
            if (mat == null || string.IsNullOrEmpty(packedMapName))
                return null;

            Channel[] channels = { red, green, blue, alpha };
            bool hasAnyTexture = false;
            for (int i = 0; i < channels.Length; i++){
                Texture tex = channels[i].texture;
                if (tex != null && tex.dimension != UnityEngine.Rendering.TextureDimension.Tex2D){
                    Debug.LogWarning($"[Mochie] Texture Packer: {tex.name} isn't a 2D texture and was skipped.", mat);
                    channels[i].texture = null;
                }
                hasAnyTexture |= channels[i].texture != null;
            }
            if (!hasAnyTexture)
                return null;

            // Not "Hidden/Mochie/TexturePacker": v1.77 also shipped an old copy under that name (MochieTexturePackerNoAlpha.shader),
            // which stays in projects updated over it and Shader.Find could pick instead
            Shader packerShader = Shader.Find("Hidden/Mochie/ChannelPacker");
            if (packerShader == null){
                Debug.LogError("[Mochie] Texture Packer: shader Hidden/Mochie/ChannelPacker is missing.");
                return null;
            }

            List<Object> created = new List<Object>();
            try {
                Texture[] sources = new Texture[4];
                bool[] encodeSRGB = new bool[4];
                for (int i = 0; i < 4; i++){
                    if (channels[i].texture != null)
                        sources[i] = LoadSource(channels[i].texture, created, out encodeSRGB[i]);
                }
                bool hasAlpha = sources[3] != null;
                Vector2Int resolution = GetOutputResolution(sources);

                Material packer = new Material(packerShader);
                created.Add(packer);
                SetChannel(packer, "_Red", sources[0], channels[0], encodeSRGB[0]);
                SetChannel(packer, "_Green", sources[1], channels[1], encodeSRGB[1]);
                SetChannel(packer, "_Blue", sources[2], channels[2], encodeSRGB[2]);
                SetChannel(packer, "_Alpha", sources[3], channels[3], encodeSRGB[3]);

                Texture2D packed = Bake(packer, resolution, hasAlpha);
                created.Add(packed);

                string path = SavePackedTexture(mat, packedMapName, packed, hasAlpha);
                Texture2D saved = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (saved == null){
                    Debug.LogError($"[Mochie] Texture Packer: couldn't load the packed texture at {path}.", mat);
                    return null;
                }

                Undo.RecordObject(mat, "Pack Textures");
                mat.SetTexture(packedMapName, saved);
                mat.SetTextureScale(packedMapName, Vector2.one);
                mat.SetTextureOffset(packedMapName, Vector2.zero);
                return saved;
            }
            catch (Exception e){
                Debug.LogError($"[Mochie] Texture Packer: packing {mat.name} failed.", mat);
                Debug.LogException(e);
                return null;
            }
            finally {
                foreach (Object obj in created){
                    if (obj != null)
                        Object.DestroyImmediate(obj);
                }
                DeleteTempFolder();
            }
        }

        // Removes the uncompressed copies made by LoadSource
        internal static void DeleteTempFolder(){
            if (AssetDatabase.IsValidFolder(TempFolder))
                AssetDatabase.DeleteAsset(TempFolder);
        }

        public static Vector4 GetScaleOffset(Material mat, string texName){
            Vector2 scale = mat.GetTextureScale(texName);
            Vector2 offset = mat.GetTextureOffset(texName);
            return new Vector4(scale.x, scale.y, offset.x, offset.y);
        }

        // Finds the tiling and offset shared by every assigned texture in texNames.
        // Returns false if the assigned textures don't all match.
        public static bool TryGetSharedScaleOffset(Material mat, out Vector4 scaleOffset, params string[] texNames){
            scaleOffset = new Vector4(1f, 1f, 0f, 0f);
            bool found = false;
            foreach (string texName in texNames){
                if (!mat.GetTexture(texName))
                    continue;
                Vector4 st = GetScaleOffset(mat, texName);
                if (found && st != scaleOffset)
                    return false;
                scaleOffset = st;
                found = true;
            }
            return true;
        }

        // Returns a texture that samples the source's stored values at full resolution.
        // 8-bit PNG and JPG files are decoded directly; other image files are imported as an uncompressed linear copy.
        // Call DeleteTempFolder once the returned texture is no longer needed.
        internal static Texture LoadSource(Texture tex, List<Object> created, out bool encodeSRGB){
            encodeSRGB = false;
            string assetPath = AssetDatabase.GetAssetPath(tex);
            if (!string.IsNullOrEmpty(assetPath) && AssetDatabase.IsMainAsset(tex)){
                string physicalPath = FileUtil.GetPhysicalPath(assetPath);
                if (File.Exists(physicalPath)){
                    string ext = Path.GetExtension(physicalPath).ToLowerInvariant();
                    if (ext == ".png" || ext == ".jpg" || ext == ".jpeg"){
                        byte[] bytes = File.ReadAllBytes(physicalPath);
                        // LoadImage truncates 16-bit PNGs to 8 bits where the importer rounds, so leave those to the importer
                        bool is16BitPng = ext == ".png" && bytes.Length > 24 && bytes[24] == 16;
                        if (!is16BitPng){
                            Texture2D decoded = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
                            created.Add(decoded);
                            if (decoded.LoadImage(bytes, true)){
                                decoded.wrapMode = TextureWrapMode.Repeat;
                                decoded.filterMode = FilterMode.Bilinear;
                                return decoded;
                            }
                        }
                    }
                    Texture2D copy = ImportUncompressedCopy(physicalPath, ext);
                    if (copy != null)
                        return copy;
                }
            }

            // No image file to read, so sample the imported texture and undo any sRGB decode
            encodeSRGB = QualitySettings.activeColorSpace == ColorSpace.Linear && GraphicsFormatUtility.IsSRGBFormat(tex.graphicsFormat);
            return tex;
        }

        // Imports a temporary copy of the file without compression, size limits or sRGB decoding.
        // The copy is removed along with the temp folder once packing finishes.
        static Texture2D ImportUncompressedCopy(string physicalPath, string ext){
            if (!AssetDatabase.IsValidFolder(TempFolder))
                AssetDatabase.CreateFolder("Assets", TempFolderName);
            string tempPath = $"{TempFolder}/{Guid.NewGuid():N}{ext}";
            File.Copy(physicalPath, FileUtil.GetPhysicalPath(tempPath));
            AssetDatabase.ImportAsset(tempPath, ImportAssetOptions.ForceSynchronousImport);

            TextureImporter importer = AssetImporter.GetAtPath(tempPath) as TextureImporter;
            if (importer == null)
                return null;
            importer.textureType = TextureImporterType.Default;
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.sRGBTexture = false;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 16384;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(tempPath);
        }

        // Alpha-only textures store their value in alpha, and single channel textures in red,
        // whichever color channel is requested (their alpha still reads as 1)
        static int ResolveSourceChannel(Texture tex, int requested){
            return ResolveSourceChannel(tex.graphicsFormat, requested);
        }

        internal static int ResolveSourceChannel(GraphicsFormat format, int requested){
            requested = Mathf.Clamp(requested, 0, 3);
            if (GraphicsFormatUtility.IsAlphaOnlyFormat(format))
                return 3;
            if (GraphicsFormatUtility.GetComponentCount(format) == 1 && requested < 3)
                return 0;
            return requested;
        }

        // Uses the size of the largest source, never higher, even when baked tiling loses detail as a result
        static Vector2Int GetOutputResolution(Texture[] sources){
            Vector2Int resolution = Vector2Int.one;
            long largest = 0;
            foreach (Texture source in sources){
                if (source == null)
                    continue;
                long pixels = (long)source.width * source.height;
                if (pixels > largest){
                    largest = pixels;
                    resolution = new Vector2Int(source.width, source.height);
                }
            }
            return resolution;
        }

        static void SetChannel(Material packer, string texName, Texture source, Channel channel, bool encodeSRGB){
            bool hasTexture = source != null;
            Vector4 mask = Vector4.zero;
            if (hasTexture)
                mask[ResolveSourceChannel(source, channel.sourceChannel)] = 1f;

            packer.SetTexture(texName, source);
            packer.SetTextureScale(texName, new Vector2(channel.scaleOffset.x, channel.scaleOffset.y));
            packer.SetTextureOffset(texName, new Vector2(channel.scaleOffset.z, channel.scaleOffset.w));
            packer.SetVector(texName + "Mask", mask);
            packer.SetVector(texName + "Params", new Vector4(hasTexture ? 1f : 0f, channel.strength, channel.fallback, channel.invert ? 1f : 0f));
            packer.SetVector(texName + "Flags", new Vector4(channel.isOcclusion ? 1f : 0f, encodeSRGB ? 1f : 0f, 0f, 0f));
        }

        // Output is 8 bits per channel: Unity can only encode 8-bit PNGs, and its importer reduces
        // 16-bit PNGs to 8 bits unless an uncompressed 16-bit format is chosen
        static Texture2D Bake(Material packer, Vector2Int resolution, bool hasAlpha){
            TextureFormat texFormat = hasAlpha ? TextureFormat.RGBA32 : TextureFormat.RGB24;
            RenderTexture rt = RenderTexture.GetTemporary(resolution.x, resolution.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            RenderTexture previous = RenderTexture.active;
            Texture2D tex = null;
            try {
                Graphics.Blit(null, rt, packer);
                RenderTexture.active = rt;
                tex = new Texture2D(resolution.x, resolution.y, texFormat, false, true);
                tex.ReadPixels(new Rect(0, 0, resolution.x, resolution.y), 0, 0, false);
                tex.Apply(false, false);
                return tex;
            }
            catch {
                if (tex != null)
                    Object.DestroyImmediate(tex);
                throw;
            }
            finally {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        // Writes the packed texture to disk and returns its asset path. A texture this material and property
        // packed before is overwritten in place, keeping its references, platform overrides and changed settings.
        static string SavePackedTexture(Material mat, string packedMapName, Texture2D packed, bool hasAlpha){
            string matGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(mat));
            string tag = string.IsNullOrEmpty(matGuid) ? ImporterTag : $"{ImporterTag}:{matGuid}:{packedMapName}";

            string path = null;
            Texture current = mat.GetTexture(packedMapName);
            if (current != null && !string.IsNullOrEmpty(matGuid)){
                string currentPath = AssetDatabase.GetAssetPath(current);
                TextureImporter currentImporter = AssetImporter.GetAtPath(currentPath) as TextureImporter;
                if (currentImporter != null && IsPackedBy(currentImporter, tag) && currentPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    path = currentPath;
            }
            bool isNew = path == null;
            if (isNew)
                path = GetSavePath(mat, packedMapName);

            File.WriteAllBytes(FileUtil.GetPhysicalPath(path), packed.EncodeToPNG());

            // The importer only exists after the first import, so new textures are imported twice
            if (isNew)
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && ApplyImportSettings(importer, packed, hasAlpha, tag))
                importer.SaveAndReimport();
            else
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            return path;
        }

        // Without alpha, normal quality compression (BC1 on PC) is half the size of high quality (BC7).
        // With alpha, normal quality (BC3) is the same size as BC7, so high quality costs nothing extra.
        // userData stores the tag and the settings the last pack wanted. A setting that no longer matches
        // was changed by the user and is kept. sRGB and alpha must match the content, so they're always set.
        // Returns true if anything changed.
        internal static bool ApplyImportSettings(TextureImporter importer, Texture2D packed, bool hasAlpha, string tag, bool sRGB = false){
            TextureImporterAlphaSource alphaSource = hasAlpha ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
            int[] wanted = {
                (int)(hasAlpha ? TextureImporterCompression.CompressedHQ : TextureImporterCompression.Compressed),
                Mathf.Clamp(Mathf.NextPowerOfTwo(Mathf.Max(packed.width, packed.height)), 32, 16384),
                16,
                (int)FilterMode.Trilinear
            };
            int[] current = { (int)importer.textureCompression, importer.maxTextureSize, importer.anisoLevel, (int)importer.filterMode };
            int[] lastWanted = GetLastWanted(importer.userData, tag, wanted.Length);

            int[] result = new int[wanted.Length];
            bool changed = false;
            for (int i = 0; i < wanted.Length; i++){
                result[i] = lastWanted != null && current[i] != lastWanted[i] ? current[i] : wanted[i];
                changed |= result[i] != current[i];
            }
            string userData = $"{tag}|{string.Join(",", wanted)}";

            changed |= importer.sRGBTexture != sRGB || importer.alphaSource != alphaSource || importer.userData != userData;
            if (changed){
                importer.sRGBTexture = sRGB;
                importer.alphaSource = alphaSource;
                importer.textureCompression = (TextureImporterCompression)result[0];
                importer.maxTextureSize = result[1];
                importer.anisoLevel = result[2];
                importer.filterMode = (FilterMode)result[3];
                importer.userData = userData;
            }
            return changed;
        }

        static bool IsPackedBy(TextureImporter importer, string tag){
            return importer.userData == tag || importer.userData.StartsWith(tag + "|", StringComparison.Ordinal);
        }

        // Returns the settings stored by the last pack, or null if there are none (a new texture, or one packed before they were stored)
        static int[] GetLastWanted(string userData, string tag, int count){
            if (!userData.StartsWith(tag + "|", StringComparison.Ordinal))
                return null;
            string[] parts = userData.Substring(tag.Length + 1).Split(',');
            if (parts.Length != count)
                return null;
            int[] values = new int[count];
            for (int i = 0; i < count; i++){
                if (!int.TryParse(parts[i], out values[i]))
                    return null;
            }
            return values;
        }

        static string GetSavePath(Material mat, string packedMapName){
            string matPath = AssetDatabase.GetAssetPath(mat);
            // Unsaved materials and materials outside Assets (which may be read-only packages) save into Assets
            bool inAssets = matPath.StartsWith("Assets/", StringComparison.Ordinal);
            string dir = inAssets ? Path.GetDirectoryName(matPath).Replace('\\', '/') : "Assets";
            string matName = string.IsNullOrEmpty(matPath) ? mat.name : Path.GetFileNameWithoutExtension(matPath);

            string fileName = matName + packedMapName;
            foreach (char c in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(c, '_');
            return AssetDatabase.GenerateUniqueAssetPath($"{dir}/{fileName}.png");
        }
    }
}
