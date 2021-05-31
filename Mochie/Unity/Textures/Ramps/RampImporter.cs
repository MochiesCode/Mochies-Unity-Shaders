#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// From https://answers.unity.com/questions/382545/changing-texture-import-settings-during-runtime.html

namespace Mochie {
	public class RampImporter : AssetPostprocessor {

		void OnPreprocessTexture(){
			if(assetPath.Contains("Ramps") && assetPath.Contains("Mochie")){
				TextureImporter importer = assetImporter as TextureImporter;
				importer.textureType  = TextureImporterType.Default;
				importer.textureCompression = TextureImporterCompression.Uncompressed;
				importer.isReadable = true;
				importer.mipmapEnabled = true;
				importer.filterMode = FilterMode.Trilinear;
				importer.anisoLevel = 16;
				importer.maxTextureSize = 128;
				importer.wrapMode = TextureWrapMode.Clamp;
				importer.streamingMipmaps = true;
		
				Object asset = AssetDatabase.LoadAssetAtPath(importer.assetPath, typeof(Texture2D));
				if (asset)
					EditorUtility.SetDirty(asset);
				else
					importer.textureType  = TextureImporterType.Default;   
			}
		}
	}
}
#endif