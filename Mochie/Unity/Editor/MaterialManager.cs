using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

class Sorter : IComparer<string> { 
    public int Compare(string x, string y){   
        if (x == null || y == null) { 
            return 0; 
        }  
        return x.CompareTo(y);    
    } 
} 
  
public class MaterialManager : EditorWindow {

	List<string> matList = new List<string>();
	List<string> keywordList = new List<string>();
	uint matCount = 0;
	string displayList = "";
	bool refreshLabel = true;
	Vector2 scrollPos;

	[MenuItem("Mochie/Material Manager")]
	static void Init(){
		MaterialManager window = (MaterialManager)EditorWindow.GetWindow(typeof(MaterialManager));
		window.titleContent = new GUIContent("Materials");
		window.Show();
	}

	void OnGUI(){
		MGUI.Space10();
		float buttonSize = position.width/2.0f;
		if (MGUI.SimpleButton("Update Materials", buttonSize, buttonSize/2.0f)){
			UpdateMaterials();
			refreshLabel = true;
		}
		MGUI.Space8();
		GUILayout.Label(matCount + " Uber materials found...");
		MGUI.SpaceN4();
		ContentBox(new Vector2(position.width, position.height));
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width+1), GUILayout.Height(position.height-54));
		foreach (string s in matList){
			if (refreshLabel)
				displayList += s+"\n";
		}
		displayList = displayList.TrimEnd();
		GUILayout.Label(displayList);
		EditorGUILayout.EndScrollView();
		refreshLabel = false;
	}

	void UpdateMaterials(){
		matCount = 0;
		matList.Clear();
		displayList = "";
		List<Material> materials = FindAssetsByType<Material>();
		foreach (Material mat in materials){
			if (mat.shader.name.Contains("Uber Shader")){
				int renderMode = mat.GetInt("_RenderMode");
				int blendMode = mat.GetInt("_BlendMode");
				int cullMode = mat.GetInt("_CullingMode");
				int cubeMode = mat.GetInt("_CubeBlendMode");
				int workflow = mat.GetInt("_PBRWorkflow");
				int screenspace = mat.GetInt("_Screenspace");
				int reflections = mat.GetInt("_Reflections");
				int specular = mat.GetInt("_Specular");
				int ssr = mat.GetInt("_SSR");
				int maskingMode = mat.GetInt("_MaskingMode");
				int distortMode = mat.GetInt("_DistortionStyle");
				int reflToggle = mat.GetInt("_Reflections");
				int specToggle = mat.GetInt("_Specular");
				int filterToggle = mat.GetInt("_Filtering");
				int emissToggle = mat.GetInt("_EmissionToggle");
				int pulseToggle = mat.GetInt("_PulseToggle");
				int dissolveStyle = mat.GetInt("_DissolveStyle");
				int matcapToggle = mat.GetInt("_MatcapToggle");
				int eRimToggle = mat.GetInt("_EnvironmentRim");
				int spriteToggle0 = mat.GetInt("_EnableSpritesheet");
				int spriteToggle1 = mat.GetInt("_EnableSpritesheet1");
				int postFilterToggle = mat.GetInt("_PostFiltering");
				int stencilToggle = mat.GetInt("_StencilToggle");
				int cloneToggle = mat.GetInt("_CloneToggle");
				bool isUberX = MGUI.IsXVersion(mat);
				bool isOutline = MGUI.IsOutline(mat);

				if (isOutline){
					if (stencilToggle == 1){
						mat.SetFloat("_StencilPass", (float)UnityEngine.Rendering.StencilOp.Replace);
						mat.SetFloat("_StencilFail", (float)UnityEngine.Rendering.StencilOp.Keep);
						mat.SetFloat("_StencilZFail", (float)UnityEngine.Rendering.StencilOp.Keep);
						mat.SetFloat("_StencilCompare", (float)UnityEngine.Rendering.CompareFunction.Always);

						mat.SetFloat("_OutlineStencilPass", (float)UnityEngine.Rendering.StencilOp.Keep);
						mat.SetFloat("_OutlineStencilFail", (float)UnityEngine.Rendering.StencilOp.Keep);
						mat.SetFloat("_OutlineStencilZFail", (float)UnityEngine.Rendering.StencilOp.Keep);
						mat.SetFloat("_OutlineStencilCompare", (float)UnityEngine.Rendering.CompareFunction.NotEqual);
					}
					else {
						mat.SetFloat("_StencilPass", (float)UnityEngine.Rendering.StencilOp.Keep);
						mat.SetFloat("_StencilFail", (float)UnityEngine.Rendering.StencilOp.Keep);
						mat.SetFloat("_StencilZFail", (float)UnityEngine.Rendering.StencilOp.Keep);
						mat.SetFloat("_StencilCompare", (float)UnityEngine.Rendering.CompareFunction.Always);

						mat.SetFloat("_OutlineStencilPass", (float)UnityEngine.Rendering.StencilOp.Keep);
						mat.SetFloat("_OutlineStencilFail", (float)UnityEngine.Rendering.StencilOp.Keep);
						mat.SetFloat("_OutlineStencilZFail", (float)UnityEngine.Rendering.StencilOp.Keep);
						mat.SetFloat("_OutlineStencilCompare", (float)UnityEngine.Rendering.CompareFunction.Always);
					}
				}


				bool usingNormal = mat.GetTexture("_BumpMap");
				bool usingParallax = mat.GetTexture("_ParallaxMap");
				bool usingDetail = mat.GetTexture("_DetailNormalMap");
				bool prevAO = mat.GetInt("_AOFiltering") == 1 && mat.GetInt("_PreviewAO") == 1;
				bool prevRough = mat.GetInt("_RoughnessFiltering") == 1 && mat.GetInt("_PreviewRough") == 1;
				prevRough = prevRough && (workflow == 0 || workflow == 3);
				bool prevSmooth = mat.GetInt("_SmoothnessFiltering") == 1 && mat.GetInt("_PreviewSmooth") == 1;
				prevSmooth = prevSmooth && (workflow == 1 || workflow == 2);
				bool prevHeight = mat.GetInt("_HeightFiltering") == 1 && mat.GetInt("_PreviewHeight") == 1;
				bool dissolveWireframe = isUberX && dissolveStyle == 3 && mat.GetInt("_GeomDissolveWireframe") == 1;
				
				// Force backface culling off if using screenspace mesh
				if (isUberX && screenspace == 1 && cullMode != 0)
					mat.SetInt("_CullingMode", 0);

				// Setting floats based on render mode/texture presence/etc
				mat.SetInt("_IsCubeBlendMask", mat.GetTexture("_CubeBlendMask") ? 1 : 0);
				mat.SetInt("_UseSmoothMap", mat.GetTexture("_SmoothnessMap") && workflow == 1 ? 1 : 0);
				mat.SetInt("_UseMirrorAlbedo", mat.GetTexture("_MirrorTex") && isUberX ? 1 : 0);
				mat.SetInt("_OutlineCulling", screenspace == 1 || stencilToggle == 1 ? 2 : 1);
				mat.SetInt("_UseMatcap1", mat.GetTexture("_Matcap1") ? 1 : 0);
				mat.SetInt("_ATM", blendMode == 3 ? 1 : 0);

				if (workflow == 3){
					mat.SetInt("_UseMetallicMap", 1);
					mat.SetInt("_UseSpecMap", 1);
				}
				else {
					mat.SetInt("_UseMetallicMap", mat.GetTexture("_MetallicGlossMap") ? 1 : 0);
					mat.SetInt("_UseSpecMap", mat.GetTexture("_SpecGlossMap") ? 1 : 0);
				}
				
				if (workflow != 1 && workflow != 2)
					mat.SetInt("_PreviewSmooth", 0);
				else
					mat.SetInt("_PreviewRough", 0);

				if (workflow == 3 && mat.GetInt("_RoughnessFiltering") == 1 && mat.GetInt("_PreviewRough") == 1)
					mat.SetInt("_PackedRoughPreview", 1);
				else
					mat.SetInt("_PackedRoughPreview", 0);
				
				if (workflow == 3){
					if (!mat.GetTexture("_PackedMap"))
						mat.SetInt("_AOFiltering", 0);
				}
				else {
					if (!mat.GetTexture("_OcclusionMap"))
						mat.SetInt("_AOFiltering", 0);
				}
				if (dissolveWireframe){
					mat.SetInt("_WireframeToggle", 1);
				}

				SetKeyword(mat, "_METALLICGLOSSMAP", workflow == 3 && renderMode == 1);
				SetKeyword(mat, "_SPECGLOSSMAP", (workflow == 1 || workflow == 2) && renderMode == 1);
				SetKeyword(mat, "_GLOSSYREFLECTIONS_OFF", reflToggle == 0 || renderMode == 0);
				SetKeyword(mat, "_SPECULARHIGHLIGHTS_OFF", specToggle == 0 || renderMode == 0);
				SetKeyword(mat, "_MAPPING_6_FRAMES_LAYOUT", cubeMode == 1);
				SetKeyword(mat, "_TERRAIN_NORMAL_MAP",  cubeMode == 2);
				SetKeyword(mat, "_REQUIRE_UV2", maskingMode == 2 && renderMode == 1);
				SetKeyword(mat, "_COLORADDSUBDIFF_ON", maskingMode == 1 && renderMode == 1);
				SetKeyword(mat, "_SUNDISK_NONE", renderMode == 0);
				SetKeyword(mat, "_SUNDISK_SIMPLE", specToggle == 2 && renderMode == 1);
				SetKeyword(mat, "_SUNDISK_HIGH_QUALITY", specToggle == 3 && renderMode == 1);
				SetKeyword(mat, "USER_LUT", (prevAO || prevRough || prevSmooth || prevHeight) && renderMode == 1);
				SetKeyword(mat, "EFFECT_BUMP", distortMode > 0);
				SetKeyword(mat, "GRAIN", distortMode == 1);
				SetKeyword(mat, "_COLORCOLOR_ON", filterToggle == 1);
				SetKeyword(mat, "_COLOROVERLAY_ON", filterToggle == 1 && postFilterToggle == 1);
				SetKeyword(mat, "_DETAIL_MULX2", usingDetail && renderMode == 1);
				SetKeyword(mat, "_PARALLAXMAP",  usingParallax && renderMode == 1);
				SetKeyword(mat, "_NORMALMAP", usingNormal && renderMode == 1);
				SetKeyword(mat, "_EMISSION", emissToggle == 1);
				SetKeyword(mat, "_ALPHATEST_ON", blendMode > 0 && blendMode < 4);
				SetKeyword(mat, "_ALPHABLEND_ON", blendMode == 4);
				SetKeyword(mat, "_ALPHAPREMULTIPLY_ON", blendMode == 5);
				SetKeyword(mat, "CHROMATIC_ABBERATION_LOW", reflToggle == 1 && ssr == 1 && renderMode == 1);
				SetKeyword(mat, "BLOOM_LENS_DIRT", emissToggle == 1 && pulseToggle == 1);
				SetKeyword(mat, "BLOOM", isUberX && cloneToggle == 1);
				SetKeyword(mat, "_ALPHAMODULATE_ON", dissolveStyle == 2 && isUberX);
				SetKeyword(mat, "DEPTH_OF_FIELD", dissolveStyle == 3 && isUberX);
				SetKeyword(mat, "_FADING_ON", matcapToggle == 1 && renderMode == 1);
				SetKeyword(mat, "FXAA", workflow == 4 && renderMode == 1);
				SetKeyword(mat, "PIXELSNAP_ON", eRimToggle == 1 && renderMode == 1);
				SetKeyword(mat, "EFFECT_HUE_VARIATION", spriteToggle0 == 1 || spriteToggle1 == 1);
				SetKeyword(mat, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", reflToggle == 2 && renderMode == 1);

				// Debug.Log("Applied settings to: " + mat.name);
				EditorUtility.SetDirty(mat);
				matList.Add(mat.name);
				matCount ++;
			}
			else mat.DisableKeyword("_");
			
			Sorter sortyboi = new Sorter();
			matList.Sort(sortyboi);
		}
		// UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
		AssetDatabase.SaveAssets();
	}

	static void SetKeyword(Material m, string keyword, bool state) {
		if (state)
			m.EnableKeyword(keyword);
		else
			m.DisableKeyword(keyword);
	}

	static List<T> FindAssetsByType<T>() where T : UnityEngine.Object {
		List<T> assets = new List<T>();
		string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof (T).ToString().Replace("UnityEngine.", "")));
		for(int i = 0; i < guids.Length; i++){
			string assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );
			T asset = AssetDatabase.LoadAssetAtPath<T>( assetPath );
			if(asset != null){
				assets.Add(asset);
			}
		}
		return assets;
	}

	// Create a GUI box
    public void ContentBox(Vector2 boxSize){
        GUILayout.Space(4);
        Rect pos = GUILayoutUtility.GetRect(0f, boxSize.y);
        pos.width = boxSize.x;
        pos.x += 1.0f;
        //GUILayout.Space(4);
        GUI.Box(pos, "");
        GUILayout.Space(-boxSize.y);
    }
}