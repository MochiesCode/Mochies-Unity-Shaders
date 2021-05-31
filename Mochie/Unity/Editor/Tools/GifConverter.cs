// By Poiyomi - poiyomi#0001
// https://www.patreon.com/poiyomi

// Used with permission:
// https://i.imgur.com/G4OZj8V.png

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using UnityEditor;
using UnityEngine;

namespace Mochie {
    public class GifImporter{
        [MenuItem("Mochie/Convert to Flipbook Asset")]
		[MenuItem("Assets/Convert to Flipbook Asset")]
        static void GifImport(){
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            List<Texture2D> array = GetGifFrames(path);
            if (array == null) return;
            Texture2DArray arrayTexture = Textre2DArrayToAsset(array.ToArray());
            AssetDatabase.CreateAsset(arrayTexture, path.Replace(".gif", ".asset"));
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Mochie/Convert to Flipbook Asset", true)]
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
            List<Texture2D> gifFrames = new List<Texture2D>();
            var gifImage = Image.FromFile(path);
            var dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);

            int width = Mathf.ClosestPowerOfTwo(gifImage.Width-1);
            int height = Mathf.ClosestPowerOfTwo(gifImage.Height-1);

            bool hasAlpha = false;

            int frameCount = gifImage.GetFrameCount(dimension);

            float totalProgress = frameCount * width;
            for (int i = 0; i < frameCount; i++){
                gifImage.SelectActiveFrame(dimension, i);
                var ogframe = new Bitmap(gifImage.Width, gifImage.Height);
                System.Drawing.Graphics.FromImage(ogframe).DrawImage(gifImage, Point.Empty);
                var frame = ResizeBitmap(ogframe,width,height);

                Texture2D frameTexture = new Texture2D(frame.Width, frame.Height);

                float doneProgress = i * width;
                for (int x = 0; x < frame.Width; x++){
                    if(x%20 == 0)
                    if (EditorUtility.DisplayCancelableProgressBar("From GIF", "Frame "+i+": "+(int)((float)x/width*100)+"%", (doneProgress + x + 1) / totalProgress)){
                        EditorUtility.ClearProgressBar();
                        return null;
                    }

                    for (int y = 0; y < frame.Height; y++){
                        System.Drawing.Color sourceColor = frame.GetPixel(x, y);
                        frameTexture.SetPixel(x, frame.Height - 1 - y, new UnityEngine.Color32(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A));
                        if (sourceColor.A < 255.0f){
                            hasAlpha = true;
                        }
                    }
                }

                frameTexture.Apply();
                gifFrames.Add(frameTexture);
            }
            EditorUtility.ClearProgressBar();
            for(int i = 0; i < frameCount; i++){
                EditorUtility.CompressTexture(gifFrames[i], hasAlpha?TextureFormat.DXT5 : TextureFormat.DXT1, UnityEditor.TextureCompressionQuality.Normal);
                gifFrames[i].Apply(true,false);
            }
            return gifFrames;
        }

        public static Bitmap ResizeBitmap(Image image, int width, int height){
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = System.Drawing.Graphics.FromImage(destImage)){
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes()){
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }
    }
}