using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Mochie {
    // Renders the Texture Packer window's previews and bakes its textures. Sources are read like the
    // Pack Textures buttons read them: from the original image files at full resolution.
    class PackerRenderer {
        public const int PreviewSize = 512;
        public const int ThumbnailSize = 128;
        const int PackPass = 0;
        const int CopyPass = 1;
        const int DisplayPass = 2;
        const string ImporterTag = "MochieTexturePacker:Window";

        static readonly int[] TexIDs = {
            Shader.PropertyToID("_Tex0"),
            Shader.PropertyToID("_Tex1"),
            Shader.PropertyToID("_Tex2"),
            Shader.PropertyToID("_Tex3")
        };
        static readonly int SelectID = Shader.PropertyToID("_Select");
        static readonly int ParamsID = Shader.PropertyToID("_Params");
        static readonly int FilterID = Shader.PropertyToID("_Filter");
        static readonly int LevelsID = Shader.PropertyToID("_Levels");
        static readonly int EncodeSRGBID = Shader.PropertyToID("_EncodeSRGB");
        static readonly int ViewID = Shader.PropertyToID("_View");

        // A preview sized copy of a source, holding its stored values
        class PreviewSource {
            public RenderTexture texture;
            public GraphicsFormat format;   // The loaded source's format, which decides how its channels are read
            public Hash128 hash;
        }

        Material material;
        Dictionary<Texture, PreviewSource> previewSources = new Dictionary<Texture, PreviewSource>();
        Dictionary<Texture, Hash128> failedSources = new Dictionary<Texture, Hash128>();
        RenderTexture packed;
        RenderTexture display;
        RenderTexture[] thumbnails = new RenderTexture[4];

        public PackerRenderer(){
            Shader shader = Shader.Find("Hidden/Mochie/TexturePackerWindow");
            if (shader != null)
                material = new Material(shader){ hideFlags = HideFlags.HideAndDontSave };
        }

        public bool IsValid { get { return material != null; } }
        public RenderTexture Display { get { return display; } }

        public RenderTexture GetThumbnail(int channel){
            return thumbnails[channel];
        }

        public void Dispose(){
            foreach (PreviewSource source in previewSources.Values)
                Release(source.texture);
            previewSources.Clear();
            Release(packed);
            Release(display);
            for (int i = 0; i < thumbnails.Length; i++)
                Release(thumbnails[i]);
            if (material != null)
                Object.DestroyImmediate(material);
        }

        public static bool IsActive(int channel, bool includeAlpha){
            return channel < 3 || includeAlpha;
        }

        // The size of the texture's image file, ignoring Max Size, or the texture's own size when it has no file
        public static Vector2Int GetSourceSize(Texture tex){
            TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex)) as TextureImporter;
            if (importer != null && AssetDatabase.IsMainAsset(tex)){
                importer.GetSourceTextureWidthAndHeight(out int width, out int height);
                if (width > 0 && height > 0)
                    return new Vector2Int(width, height);
            }
            return new Vector2Int(tex.width, tex.height);
        }

        // Renders the packed result at preview size, then the view shown in the window and a grayscale thumbnail of each channel
        public void RenderPreview(PackerChannel[] channels, bool includeAlpha, Vector2Int outputSize, int view){
            if (!IsValid)
                return;

            // The editor compiles shaders asynchronously, and a blit with a shader that isn't ready yet draws nothing
            RenderTexture previous = RenderTexture.active;
            bool allowAsync = ShaderUtil.allowAsyncCompilation;
            ShaderUtil.allowAsyncCompilation = false;
            try {
                Texture[] textures = new Texture[4];
                GraphicsFormat[] formats = new GraphicsFormat[4];
                HashSet<Texture> used = new HashSet<Texture>();
                for (int i = 0; i < 4; i++){
                    Texture tex = IsActive(i, includeAlpha) ? channels[i].texture : null;
                    if (tex == null)
                        continue;
                    used.Add(tex);
                    PreviewSource source = GetPreviewSource(tex);
                    if (source != null){
                        textures[i] = source.texture;
                        formats[i] = source.format;
                    }
                }
                foreach (Texture tex in new List<Texture>(previewSources.Keys)){
                    if (!used.Contains(tex)){
                        Release(previewSources[tex].texture);
                        previewSources.Remove(tex);
                    }
                }

                // Outputs that fit the preview are shown at their own resolution, so their pixels stay sharp
                Vector2Int size = Fit(outputSize, PreviewSize);
                bool native = size == outputSize;
                EnsureTarget(ref packed, size, true);
                EnsureTarget(ref display, size, false);
                display.filterMode = native ? FilterMode.Point : FilterMode.Bilinear;
                SetChannels(channels, textures, formats, new bool[4]);

                Graphics.Blit(null, packed, material, PackPass);
                SetView(view);
                Graphics.Blit(packed, display, material, DisplayPass);
                Vector2Int thumbnailSize = Fit(outputSize, ThumbnailSize);
                for (int i = 0; i < 4; i++){
                    EnsureTarget(ref thumbnails[i], thumbnailSize, false);
                    thumbnails[i].filterMode = thumbnailSize == outputSize ? FilterMode.Point : FilterMode.Bilinear;
                    SetView(i + 1);
                    Graphics.Blit(packed, thumbnails[i], material, DisplayPass);
                }
            }
            finally {
                ShaderUtil.allowAsyncCompilation = allowAsync;
                RenderTexture.active = previous;
            }
        }

        // Packs the channels from their full quality sources into a new texture, which the caller destroys
        public Texture2D Bake(PackerChannel[] channels, bool includeAlpha, Vector2Int size){
            if (!IsValid)
                throw new InvalidOperationException("the shader Hidden/Mochie/TexturePackerWindow is missing.");

            List<Object> created = new List<Object>();
            List<RenderTexture> shrunkTargets = new List<RenderTexture>();
            Dictionary<Texture, Texture> loaded = new Dictionary<Texture, Texture>();
            Dictionary<Texture, bool> loadedSRGB = new Dictionary<Texture, bool>();
            Dictionary<Texture, RenderTexture> shrunk = new Dictionary<Texture, RenderTexture>();
            RenderTexture previous = RenderTexture.active;
            RenderTexture target = null;
            bool allowAsync = ShaderUtil.allowAsyncCompilation;
            ShaderUtil.allowAsyncCompilation = false;
            try {
                Texture[] textures = new Texture[4];
                GraphicsFormat[] formats = new GraphicsFormat[4];
                bool[] encodeSRGB = new bool[4];
                for (int i = 0; i < 4; i++){
                    Texture tex = IsActive(i, includeAlpha) ? channels[i].texture : null;
                    if (tex == null)
                        continue;
                    if (!loaded.TryGetValue(tex, out Texture source)){
                        EditorUtility.DisplayProgressBar("Texture Packer", "Loading " + tex.name + "...", (i + 0.5f) / 5f);
                        source = TexturePacker.LoadSource(tex, created, out bool isSRGB);
                        loaded[tex] = source;
                        loadedSRGB[tex] = isSRGB;
                    }
                    formats[i] = source.graphicsFormat;

                    // Bilinear sources larger than the output are shrunk first so every texel counts.
                    // Point sampling reads the nearest texels as they are.
                    if (channels[i].sampling == PackerSampling.Bilinear && (source.width > size.x || source.height > size.y)){
                        if (!shrunk.TryGetValue(tex, out RenderTexture small)){
                            small = Shrink(source, size.x, size.y, loadedSRGB[tex], false);
                            shrunk[tex] = small;
                            shrunkTargets.Add(small);
                        }
                        textures[i] = small;
                    }
                    else {
                        textures[i] = source;
                        encodeSRGB[i] = loadedSRGB[tex];
                    }
                }

                EditorUtility.DisplayProgressBar("Texture Packer", "Packing...", 0.9f);
                SetChannels(channels, textures, formats, encodeSRGB);
                target = RenderTexture.GetTemporary(size.x, size.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                Graphics.Blit(null, target, material, PackPass);
                RenderTexture.active = target;
                Texture2D result = new Texture2D(size.x, size.y, includeAlpha ? TextureFormat.RGBA32 : TextureFormat.RGB24, false, true);
                result.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0, false);
                result.Apply(false, false);
                return result;
            }
            finally {
                ShaderUtil.allowAsyncCompilation = allowAsync;
                RenderTexture.active = previous;
                if (target != null)
                    RenderTexture.ReleaseTemporary(target);
                foreach (RenderTexture rt in shrunkTargets)
                    Release(rt);
                foreach (Object obj in created){
                    if (obj != null)
                        Object.DestroyImmediate(obj);
                }
                TexturePacker.DeleteTempFolder();
            }
        }

        // Writes the texture to path (PNG or TGA by its extension) and imports it ready to use, with the same
        // settings as the Pack Textures buttons. Import settings changed since the last pack are kept.
        public static Texture2D Save(Texture2D packedTex, string path, bool hasAlpha, bool sRGB){
            bool tga = path.EndsWith(".tga", StringComparison.OrdinalIgnoreCase);
            byte[] bytes = tga ? packedTex.EncodeToTGA() : packedTex.EncodeToPNG();
            bool isNew = !(AssetImporter.GetAtPath(path) is TextureImporter);
            File.WriteAllBytes(FileUtil.GetPhysicalPath(path), bytes);

            // The importer only exists after the first import, so new textures are imported twice
            if (isNew)
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && TexturePacker.ApplyImportSettings(importer, packedTex, hasAlpha, ImporterTag, sRGB))
                importer.SaveAndReimport();
            else
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // Loads the source once and keeps a small copy until the texture's file or import settings change
        PreviewSource GetPreviewSource(Texture tex){
            Hash128 hash = GetHash(tex);
            if (previewSources.TryGetValue(tex, out PreviewSource cached)){
                if (cached.hash == hash && cached.texture != null && cached.texture.IsCreated())
                    return cached;
                Release(cached.texture);
                previewSources.Remove(tex);
            }
            // Don't retry (and log again) every repaint until the texture changes
            if (failedSources.TryGetValue(tex, out Hash128 failedHash) && failedHash == hash)
                return null;

            List<Object> created = new List<Object>();
            bool slow = !IsDecodedDirectly(tex);
            try {
                if (slow)
                    EditorUtility.DisplayProgressBar("Texture Packer", "Loading " + tex.name + "...", 0.5f);
                Texture source = TexturePacker.LoadSource(tex, created, out bool isSRGB);
                PreviewSource result = new PreviewSource {
                    texture = Shrink(source, PreviewSize, PreviewSize, isSRGB, true),
                    format = source.graphicsFormat,
                    hash = hash
                };
                previewSources[tex] = result;
                failedSources.Remove(tex);
                return result;
            }
            catch (Exception e){
                failedSources[tex] = hash;
                Debug.LogError("[Mochie] Texture Packer: couldn't load " + tex.name + ".", tex);
                Debug.LogException(e);
                return null;
            }
            finally {
                if (slow)
                    EditorUtility.ClearProgressBar();
                foreach (Object obj in created){
                    if (obj != null)
                        Object.DestroyImmediate(obj);
                }
                TexturePacker.DeleteTempFolder();
            }
        }

        // Copies src into a new texture no larger than maxWidth x maxHeight, holding its stored values (sRGB samples
        // are re-encoded). Each step at most halves the size, so every texel is averaged in like a box filter.
        RenderTexture Shrink(Texture src, int maxWidth, int maxHeight, bool encodeSRGB, bool mips){
            int width = src.width;
            int height = src.height;
            Texture current = src;
            RenderTexture previous = RenderTexture.active;
            try {
                material.SetFloat(EncodeSRGBID, encodeSRGB ? 1f : 0f);
                while (true){
                    width = width > maxWidth ? Mathf.Max(maxWidth, (width + 1) / 2) : width;
                    height = height > maxHeight ? Mathf.Max(maxHeight, (height + 1) / 2) : height;
                    bool last = width <= maxWidth && height <= maxHeight;
                    RenderTexture next;
                    if (last)
                        next = CreateTarget(width, height, RenderTextureFormat.ARGBHalf, mips);
                    else {
                        next = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
                        next.filterMode = FilterMode.Bilinear;
                    }
                    Graphics.Blit(current, next, material, CopyPass);
                    material.SetFloat(EncodeSRGBID, 0f);
                    if (current != src)
                        RenderTexture.ReleaseTemporary((RenderTexture)current);
                    if (last)
                        return next;
                    current = next;
                }
            }
            finally {
                RenderTexture.active = previous;
            }
        }

        void SetChannels(PackerChannel[] channels, Texture[] textures, GraphicsFormat[] formats, bool[] encodeSRGB){
            Vector4[] select = new Vector4[4];
            Vector4[] parameters = new Vector4[4];
            Vector4[] filter = new Vector4[4];
            Vector4[] levels = new Vector4[4];
            for (int i = 0; i < 4; i++){
                PackerChannel c = channels[i];
                bool hasTexture = textures[i] != null;
                material.SetTexture(TexIDs[i], textures[i]);
                select[i] = hasTexture ? GetSelectWeights(formats[i], c.source) : Vector4.zero;
                parameters[i] = new Vector4(hasTexture ? 1f : 0f, c.fill, c.invert ? 1f : 0f, c.sampling == PackerSampling.Point ? 1f : 0f);
                filter[i] = new Vector4(c.brightness, c.contrast, 1f / Mathf.Max(c.gamma, 0.01f), encodeSRGB[i] ? 1f : 0f);
                levels[i] = new Vector4(c.inputLevels.x, c.inputLevels.y, c.outputLevels.x, c.outputLevels.y);
            }
            material.SetVectorArray(SelectID, select);
            material.SetVectorArray(ParamsID, parameters);
            material.SetVectorArray(FilterID, filter);
            material.SetVectorArray(LevelsID, levels);
        }

        // Grayscale uses Rec. 709 luma weights on the stored values. Alpha-only and single channel
        // textures are read from the channel that holds their value, whichever one is asked for.
        static Vector4 GetSelectWeights(GraphicsFormat format, PackerSource source){
            if (source == PackerSource.Grayscale && GraphicsFormatUtility.GetComponentCount(format) >= 3 && !GraphicsFormatUtility.IsAlphaOnlyFormat(format))
                return new Vector4(0.2126f, 0.7152f, 0.0722f, 0f);
            Vector4 weights = Vector4.zero;
            weights[TexturePacker.ResolveSourceChannel(format, source == PackerSource.Grayscale ? 0 : (int)source)] = 1f;
            return weights;
        }

        void SetView(int view){
            Vector4 weights = Vector4.zero;
            if (view > 0)
                weights[view - 1] = 1f;
            material.SetVector(ViewID, weights);
        }

        static bool IsDecodedDirectly(Texture tex){
            string ext = Path.GetExtension(AssetDatabase.GetAssetPath(tex)).ToLowerInvariant();
            return ext == "" || ext == ".png" || ext == ".jpg" || ext == ".jpeg";
        }

        // Changes whenever the texture's file or import settings do
        static Hash128 GetHash(Texture tex){
            string path = AssetDatabase.GetAssetPath(tex);
            return string.IsNullOrEmpty(path) ? new Hash128() : AssetDatabase.GetAssetDependencyHash(path);
        }

        // Scales size down to fit within max on both axes, keeping its aspect ratio
        static Vector2Int Fit(Vector2Int size, int max){
            float scale = Mathf.Min(1f, (float)max / Mathf.Max(size.x, size.y));
            return new Vector2Int(Mathf.Max(1, Mathf.RoundToInt(size.x * scale)), Mathf.Max(1, Mathf.RoundToInt(size.y * scale)));
        }

        static void EnsureTarget(ref RenderTexture rt, Vector2Int size, bool mips){
            if (rt != null && rt.width == size.x && rt.height == size.y)
                return;
            Release(rt);
            rt = CreateTarget(size.x, size.y, RenderTextureFormat.ARGB32, mips);
        }

        static RenderTexture CreateTarget(int width, int height, RenderTextureFormat format, bool mips){
            RenderTexture rt = new RenderTexture(width, height, 0, format, RenderTextureReadWrite.Linear){
                useMipMap = mips,
                autoGenerateMips = mips,
                filterMode = mips ? FilterMode.Trilinear : FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };
            rt.Create();
            return rt;
        }

        static void Release(RenderTexture rt){
            if (rt == null)
                return;
            rt.Release();
            Object.DestroyImmediate(rt);
        }
    }
}
