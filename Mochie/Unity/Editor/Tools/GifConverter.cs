// By Poiyomi - poiyomi#0001
// https://www.patreon.com/poiyomi

// Used with permission:
// https://i.imgur.com/G4OZj8V.png

// GIF decoding and resizing rewritten in plain C# so it doesn't need System.Drawing or a csc.rsp

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Mochie {
    public class GifImporter{
        [MenuItem("Assets/Convert to Flipbook Asset")]
        static void GifImport(){
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            List<Texture2D> array = GetGifFrames(path);
            if (array == null) return;
            Texture2DArray arrayTexture = Textre2DArrayToAsset(array.ToArray());
            AssetDatabase.CreateAsset(arrayTexture, path.Replace(".gif", ".asset"));
            AssetDatabase.SaveAssets();
            foreach (Texture2D frame in array)
                UnityEngine.Object.DestroyImmediate(frame);
        }

        [MenuItem("Assets/Convert to Flipbook Asset", true,50)]
        static bool ValidateGifImport(){
            if (Selection.activeObject == null)
                return false;
            string path = AssetDatabase.GetAssetPath(Selection.activeObject).ToLower();
            return path.EndsWith(".gif");
        }

        private static Texture2DArray Textre2DArrayToAsset(Texture2D[] array){
            Texture2DArray texture2DArray = new Texture2DArray(array[0].width, array[0].height, array.Length, array[0].format, true);

            for (int i = 0; i < array.Length; i++){
                for (int m = 0; m < array[i].mipmapCount; m++){
                    UnityEngine.Graphics.CopyTexture(array[i], 0, m, texture2DArray, i, m);
                }
            }

            texture2DArray.anisoLevel = 16;
            texture2DArray.filterMode = FilterMode.Trilinear;
            texture2DArray.wrapModeU = array[0].wrapModeU;
            texture2DArray.wrapModeV = array[0].wrapModeV;

            texture2DArray.Apply(false, true);

            return texture2DArray;
        }

        public static List<Texture2D> GetGifFrames(string path){
            GifDecoder gif;
            try {
                gif = new GifDecoder(File.ReadAllBytes(path));
            }
            catch (Exception e){
                MGUI.ErrorBox("Couldn't convert " + Path.GetFileName(path) + ": " + e.Message);
                return null;
            }

            int width = Mathf.Max(1, Mathf.ClosestPowerOfTwo(gif.width-1));
            int height = Mathf.Max(1, Mathf.ClosestPowerOfTwo(gif.height-1));

            List<Texture2D> gifFrames = new List<Texture2D>();
            bool hasAlpha = false;

            for (int i = 0; i < gif.frameCount; i++){
                if (EditorUtility.DisplayCancelableProgressBar("From GIF", "Frame "+(i+1)+" of "+gif.frameCount, (float)i / gif.frameCount)){
                    EditorUtility.ClearProgressBar();
                    foreach (Texture2D frame in gifFrames)
                        UnityEngine.Object.DestroyImmediate(frame);
                    return null;
                }

                Color32[] pixels = Resize(gif.ReadFrame(), gif.width, gif.height, width, height);
                for (int p = 0; p < pixels.Length && !hasAlpha; p++)
                    hasAlpha = pixels[p].a < 255;

                Texture2D frameTexture = new Texture2D(width, height);
                frameTexture.SetPixels32(pixels);
                frameTexture.Apply();
                gifFrames.Add(frameTexture);
            }
            EditorUtility.ClearProgressBar();
            for(int i = 0; i < gifFrames.Count; i++){
                EditorUtility.CompressTexture(gifFrames[i], hasAlpha?TextureFormat.DXT5 : TextureFormat.DXT1, UnityEditor.TextureCompressionQuality.Normal);
                gifFrames[i].Apply(true,false);
            }
            return gifFrames;
        }

        // Bicubic resize that widens the filter when shrinking and mirrors at the edges.
        // Takes top-down rows (GIF order) and returns bottom-up rows (Texture2D order).
        internal static Color32[] Resize(Color32[] src, int srcWidth, int srcHeight, int width, int height){
            int[] xIndex, yIndex;
            float[] xWeight, yWeight;
            int xTaps = BuildTaps(srcWidth, width, out xIndex, out xWeight);
            int yTaps = BuildTaps(srcHeight, height, out yIndex, out yWeight);

            // Premultiplied so transparent pixels don't bleed their color into the edges
            float[] pre = new float[src.Length * 4];
            for (int i = 0; i < src.Length; i++){
                float a = src[i].a / 255f;
                pre[i*4]   = src[i].r / 255f * a;
                pre[i*4+1] = src[i].g / 255f * a;
                pre[i*4+2] = src[i].b / 255f * a;
                pre[i*4+3] = a;
            }

            float[] rows = new float[srcHeight * width * 4];
            for (int y = 0; y < srcHeight; y++){
                for (int x = 0; x < width; x++){
                    float r = 0, g = 0, b = 0, a = 0;
                    for (int t = 0; t < xTaps; t++){
                        float w = xWeight[x*xTaps+t];
                        int s = (y * srcWidth + xIndex[x*xTaps+t]) * 4;
                        r += pre[s] * w; g += pre[s+1] * w; b += pre[s+2] * w; a += pre[s+3] * w;
                    }
                    int d = (y * width + x) * 4;
                    rows[d] = r; rows[d+1] = g; rows[d+2] = b; rows[d+3] = a;
                }
            }

            Color32[] dst = new Color32[width * height];
            for (int y = 0; y < height; y++){
                for (int x = 0; x < width; x++){
                    float r = 0, g = 0, b = 0, a = 0;
                    for (int t = 0; t < yTaps; t++){
                        float w = yWeight[y*yTaps+t];
                        int s = (yIndex[y*yTaps+t] * width + x) * 4;
                        r += rows[s] * w; g += rows[s+1] * w; b += rows[s+2] * w; a += rows[s+3] * w;
                    }
                    a = Mathf.Clamp01(a);
                    if (a > 0f)
                        dst[(height-1-y) * width + x] = new Color32(ToByte(r/a), ToByte(g/a), ToByte(b/a), ToByte(a));
                }
            }
            return dst;
        }

        static int BuildTaps(int srcSize, int dstSize, out int[] index, out float[] weight){
            float scale = (float)srcSize / dstSize;
            float filterScale = Mathf.Max(1f, scale);
            float support = 2f * filterScale;
            int taps = Mathf.CeilToInt(support) * 2 + 2;
            index = new int[dstSize * taps];
            weight = new float[dstSize * taps];
            for (int d = 0; d < dstSize; d++){
                float center = (d + 0.5f) * scale;
                int first = Mathf.FloorToInt(center - support);
                float sum = 0f;
                for (int t = 0; t < taps; t++){
                    float w = Cubic((first + t + 0.5f - center) / filterScale);
                    index[d*taps+t] = Mirror(first + t, srcSize);
                    weight[d*taps+t] = w;
                    sum += w;
                }
                for (int t = 0; t < taps; t++)
                    weight[d*taps+t] /= sum;
            }
            return taps;
        }

        static float Cubic(float x){
            x = Mathf.Abs(x);
            if (x < 1f) return (1.5f * x - 2.5f) * x * x + 1f;
            if (x < 2f) return ((-0.5f * x + 2.5f) * x - 4f) * x + 2f;
            return 0f;
        }

        static int Mirror(int i, int size){
            int period = size * 2;
            i %= period;
            if (i < 0) i += period;
            return i < size ? i : period - 1 - i;
        }

        static byte ToByte(float v){
            return (byte)Mathf.RoundToInt(Mathf.Clamp01(v) * 255f);
        }
    }

    // Minimal GIF decoder. ReadFrame returns each frame composited over the previous ones
    // (the way browsers show it) as top-down rows, with transparent pixels left at alpha 0.
    class GifDecoder {
        class Frame {
            public int x, y, width, height;
            public int disposal, transparent, minCodeSize;
            public bool interlaced;
            public Color32[] palette;
            public byte[] data;
        }

        public int width, height;
        public int frameCount { get { return frames.Count; } }

        List<Frame> frames = new List<Frame>();
        Color32[] canvas, saved;
        int next;
        byte[] bytes;
        int pos;

        public GifDecoder(byte[] bytes){
            this.bytes = bytes;
            if (bytes.Length < 13 || bytes[0] != 'G' || bytes[1] != 'I' || bytes[2] != 'F')
                throw new Exception("it isn't a GIF file.");
            pos = 6;
            width = ReadUInt16();
            height = ReadUInt16();
            int flags = ReadByte();
            pos += 2; // Background color and aspect ratio aren't used
            Color32[] globalPalette = (flags & 0x80) != 0 ? ReadPalette(flags) : null;

            int disposal = 0, transparent = -1;
            try {
                while (pos < bytes.Length){
                    int block = ReadByte();
                    if (block == 0x3B) break;
                    if (block == 0x21){
                        // Graphic control extension: disposal and transparency for the next frame
                        if (ReadByte() == 0xF9 && pos + 4 < bytes.Length){
                            disposal = (bytes[pos+1] >> 2) & 7;
                            transparent = (bytes[pos+1] & 1) != 0 ? bytes[pos+4] : -1;
                        }
                        SkipSubBlocks();
                    }
                    else if (block == 0x2C){
                        Frame f = new Frame();
                        f.x = ReadUInt16();
                        f.y = ReadUInt16();
                        f.width = ReadUInt16();
                        f.height = ReadUInt16();
                        int frameFlags = ReadByte();
                        f.interlaced = (frameFlags & 0x40) != 0;
                        f.palette = (frameFlags & 0x80) != 0 ? ReadPalette(frameFlags) : globalPalette;
                        f.minCodeSize = ReadByte();
                        f.data = ReadSubBlocks();
                        if (f.palette == null)
                            throw new Exception("a frame has no color table.");
                        if (f.minCodeSize < 1 || f.minCodeSize > 11)
                            throw new Exception("a frame has invalid image data.");
                        f.disposal = disposal;
                        f.transparent = transparent;
                        disposal = 0;
                        transparent = -1;
                        frames.Add(f);
                    }
                    else break;
                }
            }
            // Like browsers, keep the frames read before a damaged or cut off part
            catch (Exception) when (frames.Count > 0){}

            if (frames.Count == 0)
                throw new Exception("it has no frames.");
            if (width == 0 || height == 0){
                foreach (Frame f in frames){
                    width = Math.Max(width, f.x + f.width);
                    height = Math.Max(height, f.y + f.height);
                }
            }
            canvas = new Color32[width * height];
        }

        public Color32[] ReadFrame(){
            Frame f = frames[next];
            if (next > 0){
                Frame prev = frames[next-1];
                if (prev.disposal == 2)
                    Clear(prev);
                else if (prev.disposal == 3 && saved != null)
                    Array.Copy(saved, canvas, canvas.Length);
            }
            if (f.disposal == 3){
                if (saved == null) saved = new Color32[canvas.Length];
                Array.Copy(canvas, saved, canvas.Length);
            }
            next++;

            int count;
            byte[] indices = DecodeLzw(f.data, f.minCodeSize, f.width * f.height, out count);
            int[] rows = RowOrder(f.height, f.interlaced);
            for (int i = 0; i < count; i++){
                int index = indices[i];
                if (index == f.transparent || index >= f.palette.Length) continue;
                int x = f.x + i % f.width;
                int y = f.y + rows[i / f.width];
                if (x < width && y < height)
                    canvas[y * width + x] = f.palette[index];
            }
            return (Color32[])canvas.Clone();
        }

        // Browsers clear "restore to background" areas to transparent rather than the background color
        void Clear(Frame f){
            for (int y = f.y; y < Math.Min(f.y + f.height, height); y++)
                for (int x = f.x; x < Math.Min(f.x + f.width, width); x++)
                    canvas[y * width + x] = new Color32();
        }

        static int[] RowOrder(int height, bool interlaced){
            int[] rows = new int[height];
            if (!interlaced){
                for (int i = 0; i < height; i++) rows[i] = i;
                return rows;
            }
            // Interlaced frames store every 8th row, then the rows between them in 3 more passes
            int[] start = {0, 4, 2, 1};
            int[] step = {8, 8, 4, 2};
            int n = 0;
            for (int pass = 0; pass < 4; pass++)
                for (int r = start[pass]; r < height; r += step[pass])
                    rows[n++] = r;
            return rows;
        }

        // Stops early on damaged data, count is how many pixels were decoded
        static byte[] DecodeLzw(byte[] data, int minCodeSize, int pixelCount, out int count){
            byte[] output = new byte[pixelCount];
            short[] prefix = new short[4096];
            byte[] suffix = new byte[4096];
            byte[] stack = new byte[4097];
            int clear = 1 << minCodeSize;
            int end = clear + 1;
            for (int i = 0; i < clear; i++)
                suffix[i] = (byte)i;

            int codeSize = minCodeSize + 1;
            int available = clear + 2;
            int old = -1;
            byte first = 0;
            int bits = 0, buffer = 0, pos = 0;
            count = 0;

            while (count < pixelCount){
                while (bits < codeSize){
                    if (pos >= data.Length) return output;
                    buffer |= data[pos++] << bits;
                    bits += 8;
                }
                int code = buffer & ((1 << codeSize) - 1);
                buffer >>= codeSize;
                bits -= codeSize;

                if (code == clear){
                    codeSize = minCodeSize + 1;
                    available = clear + 2;
                    old = -1;
                    continue;
                }
                if (code == end) break;
                if (old == -1){
                    if (code >= clear) break;
                    output[count++] = (byte)code;
                    first = (byte)code;
                    old = code;
                    continue;
                }

                int inCode = code;
                int top = 0;
                if (code >= available){
                    if (code > available) break;
                    stack[top++] = first;
                    code = old;
                }
                while (code >= clear){
                    stack[top++] = suffix[code];
                    code = prefix[code];
                }
                first = suffix[code];
                stack[top++] = first;

                if (available < 4096){
                    prefix[available] = (short)old;
                    suffix[available] = first;
                    available++;
                    if (available == (1 << codeSize) && codeSize < 12)
                        codeSize++;
                }
                old = inCode;
                while (top > 0 && count < pixelCount)
                    output[count++] = stack[--top];
            }
            return output;
        }

        Color32[] ReadPalette(int flags){
            Color32[] palette = new Color32[2 << (flags & 7)];
            for (int i = 0; i < palette.Length; i++)
                palette[i] = new Color32((byte)ReadByte(), (byte)ReadByte(), (byte)ReadByte(), 255);
            return palette;
        }

        byte[] ReadSubBlocks(){
            using (MemoryStream data = new MemoryStream()){
                // A cut off file keeps the data it has, so its last frame is shown partly drawn
                while (pos < bytes.Length){
                    int size = Math.Min(bytes[pos++], bytes.Length - pos);
                    if (size == 0) break;
                    data.Write(bytes, pos, size);
                    pos += size;
                }
                return data.ToArray();
            }
        }

        void SkipSubBlocks(){
            int size;
            while ((size = ReadByte()) != 0)
                pos += size;
        }

        int ReadByte(){
            if (pos >= bytes.Length)
                throw new Exception("the file is cut off.");
            return bytes[pos++];
        }

        int ReadUInt16(){
            return ReadByte() | (ReadByte() << 8);
        }
    }
}
