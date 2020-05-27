#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// From https://answers.unity.com/questions/382545/changing-texture-import-settings-during-runtime.html

public class RampImporter : AssetPostprocessor {

	void OnPreprocessTexture(){
		if(assetPath.Contains("Ramps")){
			TextureImporter importer = assetImporter as TextureImporter;
			importer.textureType  = TextureImporterType.Default;
			importer.textureCompression = TextureImporterCompression.Uncompressed;
			importer.isReadable = true;
			importer.mipmapEnabled = false;
			importer.filterMode = FilterMode.Point;
			importer.maxTextureSize = 128;
			importer.wrapMode = TextureWrapMode.Clamp;
	
			Object asset = AssetDatabase.LoadAssetAtPath(importer.assetPath, typeof(Texture2D));
			if (asset)
				EditorUtility.SetDirty(asset);
			else
				importer.textureType  = TextureImporterType.Default;   
		}
	}
}
#endif