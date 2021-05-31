using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Mochie {

	class StringSorter : IComparer<string> { 
		public int Compare(string x, string y){   
			if (x == null || y == null) { 
				return 0; 
			}  
			return x.CompareTo(y);    
		} 
	} 

	class MaterialSorter : IComparer<Material> { 
		public int Compare(Material x, Material y){   
			if (x == null || y == null) { 
				return 0; 
			}  
			return x.name.CompareTo(y.name);    
		} 
	} 

	public class KeywordHunter : EditorWindow {

		List<Material> allMaterials = new List<Material>();
		Dictionary<string, Material> materials = new Dictionary<string, Material>();
		Material selectedMat = null;
		string[] materialNames = null;
		string[] keywords = null;
		string[] whitelist = {
			"_ALPHABLEND_ON",
			"_ALPHAMODULATE_ON",
			"_ALPHAPREMULTIPLY_ON",
			"_ALPHATEST_ON",
			"_COLORADDSUBDIFF_ON",
			"_COLORCOLOR_ON",
			"_COLOROVERLAY_ON",
			"_DETAIL_MULX2",
			"_EMISSION",
			"_FADING_ON",
			"_GLOSSYREFLECTIONS_OFF",
			"_MAPPING_6_FRAMES_LAYOUT",
			"_METALLICGLOSSMAP",
			"_NORMALMAP",
			"_PARALLAXMAP",
			"_REQUIRE_UV2",
			"_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A",
			"_SPECGLOSSMAP",
			"_SPECULARHIGHLIGHTS_OFF",
			"_SUNDISK_HIGH_QUALITY",
			"_SUNDISK_NONE",
			"_SUNDISK_SIMPLE",
			"_TERRAIN_NORMAL_MAP",
			"BILLBOARD_FACE_CAMERA_POS",
			"EFFECT_BUMP",
			"EFFECT_HUE_VARIATION",
			"GEOM_TYPE_BRANCH",
			"GEOM_TYPE_BRANCH_DETAIL",
			"GEOM_TYPE_FROND",
			"GEOM_TYPE_LEAF",
			"GEOM_TYPE_MESH",
			"PIXELSNAP_ON",
			"SOFTPARTICLES_ON",
			"GRAIN",
			"DITHERING",
			"TONEMAPPING_NEUTRAL",
			"TONEMAPPING_FILMIC",
			"TONEMAPPING_ACES",
			"TONEMAPPING_CUSTOM",
			"CHROMATIC_ABERRATION",
			"DEPTH_OF_FIELD",
			"DEPTH_OF_FIELD_COC_VIEW",
			"BLOOM",
			"BLOOM_LENS_DIRT",
			"COLOR_GRADING",
			"COLOR_GRADING_LOG_VIEW",
			"USER_LUT",
			"VIGNETTE_CLASSIC",
			"VIGNETTE_MASKED",
			"DISTORT",
			"CHROMATIC_ABERRATION_LOW",
			"BLOOM_LOW",
			"VIGNETTE",
			"FINALPASS",
			"COLOR_GRADING_HDR_3D",
			"COLOR_GRADING_HDR",
			"AUTO_EXPOSURE",
			"LOD_FADE_CROSSFADE",
			"FXAA"
		};
		int selection = 0;
		uint matCount = 0;
		bool excludeWhitelisted = true;
		bool needsRefresh =  true;
		bool hasToggled = true;

		Vector2 scrollPos0;
		Vector2 scrollPos1;

		[MenuItem("Mochie/Keyword Hunter")]
		static void Init(){
			KeywordHunter window = (KeywordHunter)EditorWindow.GetWindow(typeof(KeywordHunter));
			window.titleContent = new GUIContent("Keyword Hunter");
			window.minSize = new Vector2(window.minSize.x, 565);
			window.Show();
		}

		void OnGUI(){
			MGUI.Space10();
			if (needsRefresh){
				selection = 0;
				PopulateMaterialList();
				needsRefresh = false;
			}
			MGUI.Space8();
			GUILayout.Label(matCount + " material(s) with keywords found...");
			MGUI.SpaceN4();

			// Display list of materials
			// float boxSpacing = position.height-200 > 10 ? position.height-200 : 10;
			float spacing = (position.height/2.0f);
			ContentBox(new Vector2(position.width, spacing));
			scrollPos0 = EditorGUILayout.BeginScrollView(scrollPos0, GUILayout.Width(position.width+1), GUILayout.Height(spacing));
			if (materialNames != null)
				selection = GUILayout.SelectionGrid(selection, materialNames, 1);
			if (materials.Count > 0)
				materials.TryGetValue(materialNames[selection], out selectedMat);
			EditorGUILayout.EndScrollView();

			// Display list of keywords
			ContentBox(new Vector2(position.width, spacing/2));
			scrollPos1 = EditorGUILayout.BeginScrollView(scrollPos1, GUILayout.Width(position.width+1), GUILayout.Height(spacing/2));
			if (selectedMat != null){
				keywords = selectedMat.shaderKeywords;
				if (excludeWhitelisted)
					keywords = IsolateKeywords(selectedMat.shaderKeywords, whitelist);
				GUILayout.SelectionGrid(0, keywords, 1, new GUIStyle("Label"));
			}
			EditorGUILayout.EndScrollView();

			// Display info about selection
			if (selectedMat != null){
				GUILayout.Label("Shader:		" + selectedMat.shader.name);
				GUILayout.Label("Total Keywords:	" + keywords.Length);
			}

			// Controls

			excludeWhitelisted = GUILayout.Toggle(excludeWhitelisted, " Exclude Whitelisted Keywords");
			if (hasToggled != excludeWhitelisted)
				needsRefresh = true;
			hasToggled = excludeWhitelisted;
			if (GUILayout.Button("Refresh List")){
				needsRefresh = true;
			}
			if (GUILayout.Button("Locate Selected")){
				EditorUtility.FocusProjectWindow();
				EditorGUIUtility.PingObject(selectedMat);
			}
		}

		static List<T> FindAssetsByType<T>() where T : UnityEngine.Object {
			List<T> assets = new List<T>();
			string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof (T).ToString().Replace("UnityEngine.", "")));
			for (int i = 0; i < guids.Length; i++){
				string assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );
				T asset = AssetDatabase.LoadAssetAtPath<T>( assetPath );
				if (asset != null){
					assets.Add(asset);
				}
			}
			return assets;
		}

		private void PopulateMaterialList(){
			matCount = 0;
			allMaterials.Clear();
			materials.Clear();
			allMaterials = FindAssetsByType<Material>();
			if (allMaterials != null){
				List<string> names = new List<string>();
				foreach (Material mat in allMaterials){
					if (mat.shaderKeywords.Length != 0){
						string[] kw = IsolateKeywords(mat.shaderKeywords, whitelist);
						if (excludeWhitelisted){
							if (!materials.ContainsKey(mat.name) && kw != null){
								materials.Add(mat.name, mat);
								names.Add(mat.name);
								matCount ++;
							}	
						}
						else if (!materials.ContainsKey(mat.name)){
							materials.Add(mat.name, mat);
							names.Add(mat.name);
							matCount ++;
						}	
					}
				}
				if (names != null){
					materialNames = names.ToArray();
				}
			}
		}
		
		private void PopulateKeywordList(){

		}

		static string[] IsolateKeywords(string[] a, string[] b)  {  
			HashSet<string> s = new HashSet<string>();
			for (int i = 0; i < b.Length; i++)
				s.Add(b[i]);

			List<string> extraKeywords = new List<string>();
			for (int i = 0; i < a.Length; i++){
				if (!s.Contains(a[i]))
					extraKeywords.Add(a[i]);
			}
			string[] kwds = null;
			if (extraKeywords.Count > 0)
				kwds = extraKeywords.ToArray();
			return kwds;
		}

		public void ContentBox(Vector2 boxSize){
			GUILayout.Space(4);
			Rect pos = GUILayoutUtility.GetRect(0f, boxSize.y);
			pos.width = boxSize.x;
			pos.x += 1.0f;
			GUI.Box(pos, "");
			GUILayout.Space(-boxSize.y);
		}
	}
}