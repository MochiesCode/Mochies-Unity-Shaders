// A collection of UI functions I've developed over the years to improve customization of editor scripts
// By Mochie#8794

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Mochie {
	public static class MGUI {
		
		public static Texture2D resetIcon = (Texture2D)Resources.Load("ResetIcon", typeof(Texture2D));
		public static Texture2D collapseIcon = (Texture2D)Resources.Load("CollapseIcon", typeof(Texture2D));
		public static Texture2D mochieLogo = (Texture2D)Resources.Load("MochieLogo", typeof(Texture2D));
		public static Texture2D mochieLogoPro = (Texture2D)Resources.Load("MochieLogo_Pro", typeof(Texture2D));
		public static Texture2D patIconTex = (Texture2D)Resources.Load("Patreon_Icon", typeof(Texture2D));
		public static GUIContent collapseLabel = new GUIContent(collapseIcon, "Collapse all foldout tabs.");
		public static GUIContent resetLabel = new GUIContent(resetIcon, "Reset all properties in this tab to their default values.");
		
		public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object {
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
		
		public static void UpdateMaterials(){
			List<Material> materials = FindAssetsByType<Material>();
			foreach (Material m in materials){
				if (m.shader.name.Contains("Uber Shader")){
					Debug.Log("Selected next material");
					Selection.activeObject = m;
				}
			}
		}
		
		public static void ClearKeywords(Material mat){
			foreach (string s in mat.shaderKeywords){
				mat.DisableKeyword(s);
			}
		}

		public static bool IsXVersion(Material mat){
			return mat.shader.name.Contains(" X") || mat.shader.name.Contains(" X ");
		}

		public static bool IsTessellated(Material mat){
			return mat.shader.name.Contains("(Tessellated)");
		}

		public static bool IsOutline(Material mat){
			return mat.shader.name.Contains("(Outline)");
		}

		public static bool IsLiteVersion(Material mat){
			return mat.shader.name.Contains("(Lite)");
		}
		
		public static void FillArray<T>(T[] array, T value){
			for (int i = 0; i < array.Length; i++)
				array[i] = value;
		}

		public static void FillArray<T>(T[] array, T value, int startIndex, int count){
			for (int i = startIndex; i < startIndex + count; i++)
				array[i] = value;
		}

		public static bool WriteBytes(byte[] bytes, string path){
			try {
				using (var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
				{
					fs.Write(bytes, 0, bytes.Length);
					return true;
				}
			}
			catch (Exception ex) {
				Debug.Log("Exception caught in process: " + ex.ToString());
				return false;
			}
		}

		public static Texture2D GetTextureAsset(string path){
			return (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/" + path, typeof(Texture2D));
		}

		public static void ExclusiveToggle(MaterialEditor me, MaterialProperty[] toggles){
			for (int i = 0; i < toggles.Length; i++){
				me.ShaderProperty(toggles[i], toggles[i].displayName);
				if (toggles[i].floatValue == 1){
					for (int j = 0; j < toggles.Length; j++){
						if (j != i)
							toggles[j].floatValue = 0;
					}
				}
			}
		}
		
		public static void DoFooter(string versionLabel){
			GUILayout.Space(20);
			float buttonSize = 35f;
			Rect footerRect = EditorGUILayout.GetControlRect();
			footerRect.x += (MGUI.GetInspectorWidth()/2f)-buttonSize-5f;
			footerRect.width = buttonSize;
			footerRect.height = buttonSize;
			if (GUI.Button(footerRect, MGUI.patIconTex))
				Application.OpenURL("https://www.patreon.com/mochieshaders");
			footerRect.x += buttonSize + 5f;
			footerRect.y += 17f;
			GUIStyle formatting = new GUIStyle();
			formatting.fontSize = 15;
			formatting.fontStyle = FontStyle.Bold;
			if (EditorGUIUtility.isProSkin){
				Color proCol = new Color(0.8f, 0.8f, 0.8f, 1);
				formatting.normal.textColor = proCol;
				formatting.hover.textColor = proCol;
			}
			GUI.Label(footerRect, versionLabel, formatting);
			footerRect.y += 20f;
			footerRect.x -= 35f;
			footerRect.width = 70f;
			footerRect.height = 70f;
			GUI.Label(footerRect, MGUI.mochieLogo);
			GUILayout.Space(90);
		}

		public static void DisplayError(string message){
			EditorGUILayout.HelpBox(message, MessageType.Error);
		}

		public static void DisplayWarning(string message){
			EditorGUILayout.HelpBox(message, MessageType.Warning);
		}

		public static void DisplayInfo(string message){
			EditorGUILayout.HelpBox(message, MessageType.Info);
		}
		
		public static void DisplayText(string message){
			EditorGUILayout.HelpBox(message, MessageType.None);
		}

		public static void DummyProperty(string label, string property){
			Rect r = EditorGUILayout.GetControlRect();
			r.x -= 1f;
			GUI.Label(r, label);
			r.x += EditorGUIUtility.labelWidth;
			GUI.Label(r, property);
		}

		public static void MaskProperty(Material mat, MaterialEditor me, bool display, MaterialProperty mask, MaterialProperty scroll){
			if (display){
				me.TexturePropertySingleLine(new GUIContent("Mask Texture"), mask);
				TextureSOScroll(me, mask, scroll, mask.textureValue);
				MGUI.Space4();
			}
		}

		public static bool LinkButton(Texture2D tex, float width, float height, float xPos){
			Rect buttonRect = EditorGUILayout.GetControlRect();
			buttonRect.width = width;
			buttonRect.height = height;
			buttonRect.x += ((GetInspectorWidth()/2f)-width/2f)-xPos;
			return GUI.Button(buttonRect, tex);
		}

		public static bool LinkButton(GUIContent g, float width, float height, float xPos){
			Rect buttonRect = EditorGUILayout.GetControlRect();
			buttonRect.width = width;
			buttonRect.height = height;
			buttonRect.x += ((GetInspectorWidth()/2f)-width/2f)-xPos;
			return GUI.Button(buttonRect, g);
		}

		public static bool SimpleButton(string text, float width, float xPos){
			Rect buttonRect = EditorGUILayout.GetControlRect();
			buttonRect.width = width;
			buttonRect.x += xPos;
			return GUI.Button(buttonRect, text);
		}

		public static bool SimpleButton(Texture2D tex, float width, float xPos){
			Rect buttonRect = EditorGUILayout.GetControlRect();
			buttonRect.width = width;
			buttonRect.x += xPos;
			return GUI.Button(buttonRect, tex);
		}

		public static bool PropertyButton(String label){
			return SimpleButton(label, GetPropertyWidth(), EditorGUIUtility.labelWidth);
		}
		
		public static bool ResetButton(){
			return SimpleButton("Reset", GetPropertyWidth(), EditorGUIUtility.labelWidth);
		}

		public static void DoResetButton(MaterialProperty vec0, MaterialProperty vec1, Vector4 default0, Vector4 default1){
			if (ResetButton()){
				vec0.vectorValue = default0;
				vec1.vectorValue = default1;
			}
		}

		public static void DoResetButton(MaterialProperty vec0, MaterialProperty vec1){
			if (ResetButton()){
				vec0.vectorValue = new Vector4(0,0,0,0);
				vec1.vectorValue = new Vector4(0,0,0,0);
			}
		}

		public static bool TabButton(Texture2D tex, float offset){
			GUILayout.Space(-28);
			Rect buttonRect = EditorGUILayout.GetControlRect();
			buttonRect.width = 27;
			buttonRect.height = 23;
			buttonRect.x += GetInspectorWidth()-offset;
			return GUI.Button(buttonRect, tex);
		}

		public static bool TabButton(GUIContent label, float offset){
			GUILayout.Space(-28);
			Rect buttonRect = EditorGUILayout.GetControlRect();
			buttonRect.width = 27;
			buttonRect.height = 23;
			buttonRect.x += GetInspectorWidth()-offset;
			return GUI.Button(buttonRect, label);
		}

		public static bool MedTabButton(Texture2D tex, float offset){
			GUILayout.Space(-25);
			Rect buttonRect = EditorGUILayout.GetControlRect();
			buttonRect.width = 23;
			buttonRect.height = 19;
			buttonRect.x += GetInspectorWidth()-offset;
			return GUI.Button(buttonRect, tex);
		}

		public static bool MedTabButton(GUIContent label, float offset){
			GUILayout.Space(-25);
			Rect buttonRect = EditorGUILayout.GetControlRect();
			buttonRect.width = 23;
			buttonRect.height = 19;
			buttonRect.x += GetInspectorWidth()-offset;
			return GUI.Button(buttonRect, label);
		}

		// Slider with a toggle
		public static void ToggleSlider(MaterialEditor me, string label, MaterialProperty toggle, MaterialProperty slider){
			float lw = EditorGUIUtility.labelWidth;
			float indent = lw + 25f;
			GUILayoutOption clickArea = GUILayout.MaxWidth(lw+13f);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = toggle.hasMixedValue;
			var tog = EditorGUILayout.Toggle(label, toggle.floatValue==1, clickArea)?1:0;
			if (EditorGUI.EndChangeCheck())
				toggle.floatValue = tog;
			EditorGUI.showMixedValue = false;

			SpaceN20();
			Rect r = EditorGUILayout.GetControlRect();
			r.x += indent;
			r.width -= indent;

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = slider.hasMixedValue;
			EditorGUI.BeginDisabledGroup(toggle.floatValue == 0);
			var slide = EditorGUI.Slider(r, slider.floatValue, slider.rangeLimits.x, slider.rangeLimits.y);
			EditorGUI.EndDisabledGroup();
			if (EditorGUI.EndChangeCheck())
				slider.floatValue = slide;
			EditorGUI.showMixedValue = false;
		}

		public static void ToggleSlider(MaterialEditor me, GUIContent label, MaterialProperty toggle, MaterialProperty slider){
			float lw = EditorGUIUtility.labelWidth;
			float indent = lw + 25f;
			GUILayoutOption clickArea = GUILayout.MaxWidth(lw+13f);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = toggle.hasMixedValue;
			var tog = EditorGUILayout.Toggle(label, toggle.floatValue==1, clickArea)?1:0;
			if (EditorGUI.EndChangeCheck())
				toggle.floatValue = tog;
			EditorGUI.showMixedValue = false;

			SpaceN20();
			Rect r = EditorGUILayout.GetControlRect();
			r.x += indent;
			r.width -= indent;

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = slider.hasMixedValue;
			EditorGUI.BeginDisabledGroup(toggle.floatValue == 0);
			var slide = EditorGUI.Slider(r, slider.floatValue, slider.rangeLimits.x, slider.rangeLimits.y);
			EditorGUI.EndDisabledGroup();
			if (EditorGUI.EndChangeCheck())
				slider.floatValue = slide;
			EditorGUI.showMixedValue = false;
		}

		public static void ToggleIntSlider(MaterialEditor me, string label, MaterialProperty toggle, MaterialProperty slider){
			float lw = EditorGUIUtility.labelWidth;
			float indent = lw + 25f;
			GUILayoutOption clickArea = GUILayout.MaxWidth(lw+13f);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = toggle.hasMixedValue;
			var tog = EditorGUILayout.Toggle(label, toggle.floatValue==1, clickArea)?1:0;
			if (EditorGUI.EndChangeCheck())
				toggle.floatValue = tog;
			EditorGUI.showMixedValue = false;

			SpaceN18();
			Rect r = EditorGUILayout.GetControlRect();
			r.x += indent;
			r.width -= indent;

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = slider.hasMixedValue;
			EditorGUI.BeginDisabledGroup(toggle.floatValue == 0);
			var slide = (int)EditorGUI.Slider(r, slider.floatValue, slider.rangeLimits.x, slider.rangeLimits.y);
			EditorGUI.EndDisabledGroup();
			if (EditorGUI.EndChangeCheck())
				slider.floatValue = slide;
			EditorGUI.showMixedValue = false;
		}
		
		public static void CustomToggleSlider(string label, MaterialProperty toggle, MaterialProperty value, float min, float max){
			SpaceN2();
			float iw = GetInspectorWidth();
			float lw = EditorGUIUtility.labelWidth;
			GUILayoutOption clickArea = GUILayout.MaxWidth(lw+13);
			Rect r0 = EditorGUILayout.GetControlRect();
			Rect r1 = r0;
			GUI.Label(r0, label);
			
			r0.width = iw-lw-(77);
			r0.x += lw+22;
			r1.width = 50;
			r1.x += iw-50;

			SpaceN20();
			toggle.floatValue = EditorGUILayout.Toggle(" ", toggle.floatValue==1, clickArea)?1:0;
			EditorGUI.BeginDisabledGroup(toggle.floatValue == 0);
			value.floatValue = GUI.HorizontalSlider(r0, value.floatValue, min, max);
			value.floatValue = EditorGUI.IntField(r1, (int)value.floatValue);
			EditorGUI.EndDisabledGroup();
		}

		// Float with a toggle
		public static void ToggleFloat(MaterialEditor me, string label, MaterialProperty toggle, MaterialProperty floatProp){
			float lw = EditorGUIUtility.labelWidth;
			float indent = lw + 20f;
			GUILayoutOption clickArea = GUILayout.MaxWidth(lw+13f);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = toggle.hasMixedValue;
			var tog = EditorGUILayout.Toggle(label, toggle.floatValue==1, clickArea)?1:0;
			if (EditorGUI.EndChangeCheck())
				toggle.floatValue = tog;
			EditorGUI.showMixedValue = false;

			SpaceN20();
			Rect r = EditorGUILayout.GetControlRect();
			r.x += indent;
			r.width -= indent;

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = floatProp.hasMixedValue;
			EditorGUI.BeginDisabledGroup(toggle.floatValue == 0);
			var floatVal = EditorGUI.FloatField(r, floatProp.floatValue);
			EditorGUI.EndDisabledGroup();
			if (EditorGUI.EndChangeCheck())
				floatProp.floatValue = floatVal;
			EditorGUI.showMixedValue = false;
		}

		public static void ToggleFloat(MaterialEditor me, GUIContent label, MaterialProperty toggle, MaterialProperty floatProp){
			float lw = EditorGUIUtility.labelWidth;
			float indent = lw + 20f;
			GUILayoutOption clickArea = GUILayout.MaxWidth(lw+13f);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = toggle.hasMixedValue;
			var tog = EditorGUILayout.Toggle(label, toggle.floatValue==1, clickArea)?1:0;
			if (EditorGUI.EndChangeCheck())
				toggle.floatValue = tog;
			EditorGUI.showMixedValue = false;

			SpaceN20();
			Rect r = EditorGUILayout.GetControlRect();
			r.x += indent;
			r.width -= indent;

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = floatProp.hasMixedValue;
			EditorGUI.BeginDisabledGroup(toggle.floatValue == 0);
			var floatVal = EditorGUI.FloatField(r, floatProp.floatValue);
			EditorGUI.EndDisabledGroup();
			if (EditorGUI.EndChangeCheck())
				floatProp.floatValue = floatVal;
			EditorGUI.showMixedValue = false;
		}

		public static void Vector3FieldToggle(string label, MaterialProperty toggle, MaterialProperty vec){
			SpaceN2();
			Vector4 newVec = vec.vectorValue;
			float labelWidth = EditorGUIUtility.labelWidth;
			float fieldWidth = (GetPropertyWidth()/3)-6f;

			Rect r = EditorGUILayout.GetControlRect();
			r.x += labelWidth+18f;

			SpaceN20();
			GUILayoutOption clickArea = GUILayout.MaxWidth(labelWidth+14f);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = toggle.hasMixedValue;
			var tog = EditorGUILayout.Toggle(label, toggle.floatValue==1, clickArea)?1:0;
			if (EditorGUI.EndChangeCheck())
				toggle.floatValue = tog;
			EditorGUI.showMixedValue = false;

			EditorGUIUtility.labelWidth = 10f;
			EditorGUI.BeginDisabledGroup(toggle.floatValue == 0);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = vec.hasMixedValue;

				// X Field
				r.width = fieldWidth-2f;
				newVec.x = EditorGUI.FloatField(r, "X", newVec.x);
				r.width = fieldWidth-4;

				// Y Field
				r.x += fieldWidth+2f;
				newVec.y = EditorGUI.FloatField(r, "Y", newVec.y);

				// Z Field
				r.x += fieldWidth+2f;
				newVec.z = EditorGUI.FloatField(r, "Z", newVec.z);

			if (EditorGUI.EndChangeCheck())
				vec.vectorValue = newVec;
			EditorGUI.showMixedValue = false;
			EditorGUIUtility.labelWidth = labelWidth;

			EditorGUI.EndDisabledGroup();
			Space1();
		}

		public static void Vector3FieldToggle(GUIContent label, MaterialProperty toggle, MaterialProperty vec){
			SpaceN2();
			Vector4 newVec = vec.vectorValue;
			float labelWidth = EditorGUIUtility.labelWidth;
			float fieldWidth = (GetPropertyWidth()/3)-6f;

			Rect r = EditorGUILayout.GetControlRect();
			r.x += labelWidth+18f;

			SpaceN20();
			GUILayoutOption clickArea = GUILayout.MaxWidth(labelWidth+14f);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = toggle.hasMixedValue;
			var tog = EditorGUILayout.Toggle(label, toggle.floatValue==1, clickArea)?1:0;
			if (EditorGUI.EndChangeCheck())
				toggle.floatValue = tog;
			EditorGUI.showMixedValue = false;

			EditorGUIUtility.labelWidth = 10f;
			EditorGUI.BeginDisabledGroup(toggle.floatValue == 0);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = vec.hasMixedValue;

				// X Field
				r.width = fieldWidth-2f;
				newVec.x = EditorGUI.FloatField(r, "X", newVec.x);
				r.width = fieldWidth-4;

				// Y Field
				r.x += fieldWidth+2f;
				newVec.y = EditorGUI.FloatField(r, "Y", newVec.y);

				// Z Field
				r.x += fieldWidth+2f;
				newVec.z = EditorGUI.FloatField(r, "Z", newVec.z);

			if (EditorGUI.EndChangeCheck())
				vec.vectorValue = newVec;
			EditorGUI.showMixedValue = false;
			EditorGUIUtility.labelWidth = labelWidth;

			EditorGUI.EndDisabledGroup();
			Space1();
		}

		public static void Vector3FieldToggleW(string label, int toggle, MaterialProperty vec){
			SpaceN2();
			Vector4 newVec = vec.vectorValue;
			float labelWidth = EditorGUIUtility.labelWidth;
			float fieldWidth = (GetPropertyWidth()/3)-6f;

			Rect r = EditorGUILayout.GetControlRect();
			r.x += labelWidth+18f;

			SpaceN20();
			GUILayoutOption clickArea = GUILayout.MaxWidth(labelWidth+14f);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = vec.hasMixedValue;
			var tog = EditorGUILayout.Toggle(label, toggle == 1, clickArea)?1:0;
			if (EditorGUI.EndChangeCheck())
				vec.vectorValue = new Vector4(vec.vectorValue.x, vec.vectorValue.y, vec.vectorValue.z, tog);
			EditorGUI.showMixedValue = false;

			EditorGUIUtility.labelWidth = 10f;
			EditorGUI.BeginDisabledGroup(toggle == 0);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = vec.hasMixedValue;

				// X Field
				r.width = fieldWidth-2f;
				newVec.x = EditorGUI.FloatField(r, "X", newVec.x);
				
				// Y Field
				r.x += fieldWidth+2;
				newVec.y = EditorGUI.FloatField(r, "Y", newVec.y);

				// Z Field
				r.x += fieldWidth;
				newVec.z = EditorGUI.FloatField(r, "Z", newVec.z);

			if (EditorGUI.EndChangeCheck())
				vec.vectorValue = new Vector4(newVec.x, newVec.y, newVec.z, tog);
			EditorGUI.showMixedValue = false;
			EditorGUIUtility.labelWidth = labelWidth;

			EditorGUI.EndDisabledGroup();
			Space1();
		}

		// Vector3 property with corrected width scaling
		public static void Vector3Field(MaterialProperty vec, string label, bool needsIndent){
			SpaceN2();
			Vector4 newVec = vec.vectorValue;
			float labelWidth = EditorGUIUtility.labelWidth;
			float fieldWidth = GetPropertyWidth()/3;
			if (needsIndent) label = "        "+ label;
			EditorGUILayout.LabelField(label);
			SpaceN20();
			Rect r = EditorGUILayout.GetControlRect();
			r.x += labelWidth;
			EditorGUIUtility.labelWidth = 10f;

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = vec.hasMixedValue;

				// R Field
				r.width = fieldWidth-2;
				newVec.x = EditorGUI.FloatField(r, "X", newVec.x);
				r.width = fieldWidth-4;

				// G Field
				r.x += fieldWidth+2;
				newVec.y = EditorGUI.FloatField(r, "Y", newVec.y);
				r.width = fieldWidth-2;
				
				// B Field
				r.x += fieldWidth;
				newVec.z = EditorGUI.FloatField(r, "Z", newVec.z);

			if (EditorGUI.EndChangeCheck())
				vec.vectorValue = newVec;
			EditorGUI.showMixedValue = false;
			EditorGUIUtility.labelWidth = labelWidth;
		}


		// Vector3 property with corrected width scaling
		public static void Vector3FieldRGB(MaterialProperty vec, string label){
			SpaceN2();
			Vector4 newVec = vec.vectorValue;
			float labelWidth = EditorGUIUtility.labelWidth;
			float fieldWidth = GetPropertyWidth()/3;

			EditorGUILayout.LabelField(label);
			SpaceN20();
			Rect r = EditorGUILayout.GetControlRect();
			r.x += labelWidth;
			EditorGUIUtility.labelWidth = 10f;

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = vec.hasMixedValue;

				// R Field
				r.width = fieldWidth-2;
				newVec.x = EditorGUI.FloatField(r, "R", newVec.x);
				r.width = fieldWidth-4;

				// G Field
				r.x += fieldWidth+2;
				newVec.y = EditorGUI.FloatField(r, "G", newVec.y);
				r.width = fieldWidth-2;

				// B Field
				r.x += fieldWidth;
				newVec.z = EditorGUI.FloatField(r, "B", newVec.z);

			if (EditorGUI.EndChangeCheck())
				vec.vectorValue = newVec;
			EditorGUI.showMixedValue = false;
			EditorGUIUtility.labelWidth = labelWidth;
		}

		// Vector2 property with corrected width scaling
		public static void Vector2Field(MaterialProperty vec, string label){
			SpaceN2();
			Vector4 newVec = vec.vectorValue;
			float labelWidth = EditorGUIUtility.labelWidth;
			float fieldWidth = GetPropertyWidth()/2;

			EditorGUILayout.LabelField(label);
			SpaceN20();
			Rect r = EditorGUILayout.GetControlRect();
			r.x += labelWidth;
			EditorGUIUtility.labelWidth = 10f;

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = vec.hasMixedValue;

				// X Field
				r.width = fieldWidth-2f;
				newVec.x = EditorGUI.FloatField(r, "X", newVec.x);
				
				// Y Field
				r.x += fieldWidth+2f;
				newVec.y = EditorGUI.FloatField(r, "Y", newVec.y);

			if (EditorGUI.EndChangeCheck())
				vec.vectorValue = newVec;
			EditorGUI.showMixedValue = false;
			EditorGUIUtility.labelWidth = labelWidth;
		}

		public static void SliderMinMax(MaterialProperty minRange, MaterialProperty maxRange, float minLimit, float maxLimit, string label, int groupLayers){
			DoMinMaxSlider(minRange, maxRange, minLimit, maxLimit, label, groupLayers);
		}

		public static void SliderMinMax01(MaterialProperty minRange, MaterialProperty maxRange, string label, int groupLayers){
			DoMinMaxSlider(minRange, maxRange, 0f, 1f, label, groupLayers);
		}
		
		public static void DoMinMaxSlider(MaterialProperty minRange, MaterialProperty maxRange, float minLimit, float maxLimit, string label, int groupLayers){
			SpaceN2();
			float offset0 = groupLayers == 1 ? 16f : 20f;
			string numFormat = "F";
			float minR = minRange.floatValue;
			float maxR = maxRange.floatValue;
			float propWidth = GetPropertyWidth();

			GUILayout.BeginHorizontal();
				Rect r = EditorGUILayout.GetControlRect();
				GUI.Label(r, label);

				r.x += EditorGUIUtility.labelWidth;
				GUI.Label(r, minR.ToString(numFormat));

				Rect prevRect = GUILayoutUtility.GetLastRect();
				r.x += prevRect.x+offset0;
				r.width = propWidth-97f;

				EditorGUI.BeginChangeCheck();
				EditorGUI.MinMaxSlider(r, ref minR, ref maxR, minLimit, maxLimit);
				prevRect = GUILayoutUtility.GetLastRect();
				if (EditorGUI.EndChangeCheck()){
					minRange.floatValue = Mathf.Floor(minR*100f)/100f;
					maxRange.floatValue = Mathf.Clamp(Mathf.Floor(maxR*100f)/100f, minRange.floatValue+0.01f, 2f);
				}
				r.x += propWidth-87f;
				GUI.Label(r, maxR.ToString(numFormat));
			GUILayout.EndHorizontal();
		}

		public static void CenteredTexture(Texture2D tex1, Texture2D tex2, float spacing, float upperMargin, float lowerMargin){
			GUILayout.Space(upperMargin);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(tex1);
			GUILayout.Space(spacing);
			GUILayout.Label(tex2);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(lowerMargin);
		}
		public static void CenteredTexture(Texture2D tex, float upperMargin, float lowerMargin){
			GUILayout.Space(upperMargin);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(tex);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(lowerMargin);
		}

		public static void CenteredText(string text, int fontSize, float upperMargin, float lowerMargin){
			GUIStyle f = new GUIStyle(EditorStyles.boldLabel);
			f.fontSize = fontSize;
			GUILayout.Space(upperMargin);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(text, f);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(lowerMargin);
		}

		public static void VersionLabel(string text, int fontSize, float upperMargin, float offset){
			GUIStyle f = new GUIStyle(EditorStyles.boldLabel);
			f.fontSize = fontSize;
			float iw = GetInspectorWidth()+offset;
			GUILayout.Space(upperMargin);
			Rect r = EditorGUILayout.GetControlRect();
			r.x = iw/2.0f;
			
			GUI.Label(r, text, f);
		}

		// Label for the third property in TexturePropertySingleLine
		public static void TexPropLabel(string text, int offset){
			GUILayout.Space(-22);
			Rect rm = EditorGUILayout.GetControlRect();
			rm.x += GetInspectorWidth()-offset;
			EditorGUI.LabelField(rm, text);
		}

		public static void TexPropLabel(GUIContent text, int offset){
			GUILayout.Space(-22);
			Rect rm = EditorGUILayout.GetControlRect();
			rm.x += GetInspectorWidth()-offset;
			EditorGUI.LabelField(rm, text);
		}

		public static void PropLabel(string text, int offset){
			SpaceN20();
			Rect rm = EditorGUILayout.GetControlRect();
			rm.x += EditorGUIUtility.labelWidth+offset+14.0f;
			EditorGUI.LabelField(rm, text);
		}

		// Need this because the provided parameter doesn't include the width of the scrollbar
		public static float GetInspectorWidth(){
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			return GUILayoutUtility.GetLastRect().width;
		}

		public static float GetPropertyWidth(){
			float lw = EditorGUIUtility.labelWidth;
			float iw = GetInspectorWidth();
			return iw - lw;
		}

		// Check if the name of the shader contains a specified string
		static bool CheckName(string name, Material mat){
			return mat.shader.name.Contains(name);
		}

		// Shorthand Scale Offset func with fixed spacing
		public static void TextureSO(MaterialEditor me, MaterialProperty prop){
			me.TextureScaleOffsetProperty(prop);
		}

		// Scale offset property with added scrolling x/y
		public static void TextureSOScroll(MaterialEditor me, MaterialProperty tex, MaterialProperty vec){
			me.TextureScaleOffsetProperty(tex);
			SpaceN2();
			Vector2Field(vec, "Scrolling");
		}

		public static void TextureSOScroll(MaterialEditor me, MaterialProperty tex, MaterialProperty vec, bool shouldDisplay){
			if (shouldDisplay){
				me.TextureScaleOffsetProperty(tex);
				SpaceN2();
				Vector2Field(vec, "Scrolling");
			}
		}

		// Shorthand Scale Offset func with fixed spacing
		public static void TextureSO(MaterialEditor me, MaterialProperty prop, bool shouldDisplay){
			if (shouldDisplay){
				me.TextureScaleOffsetProperty(prop);
			}
		}

		// Shorthand for displaying an error window
		public static void ErrorBox(string message){
			EditorUtility.DisplayDialog("Error", message, "Close");
		}

		public static void PropertyGroup(Action action){
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			Space1();
			action();
			Space1();
			EditorGUILayout.EndVertical();
			Space2();
		}

		public static void PropertyGroup(bool shouldDisplay, Action action){
			if (shouldDisplay){
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				Space2();
				action();
				Space2();
				EditorGUILayout.EndVertical();
				Space2();
			}
		}

		public static void PropertyGroupLayer(Action action){
			Color col = GUI.backgroundColor;
			GUI.backgroundColor = new Color(col.r * 0.3f, col.g * 0.3f, col.b * 0.3f);
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			GUI.backgroundColor = col;
			Space4();
			action();
			Space4();
			EditorGUILayout.EndVertical();
		}

		// Replace invalid windows characters with underscores
		public static string ReplaceInvalidChars(string filename) {
			string updated = string.Join("_", filename.Split(Path.GetInvalidFileNameChars())); 
			updated = updated.Replace(" ", "_");
			if (updated == "")
				updated = "_";
			return updated;
		}

		// Shorthand disable group stuff
		public static void ToggleGroup(bool isToggled){
			EditorGUI.BeginDisabledGroup(isToggled);
		}
		public static void ToggleGroupEnd(){
			EditorGUI.EndDisabledGroup();
		}

		public static void BoldLabel(string text){
			EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
		}

		// Mimics the normal map import warning - written by Orels1
		static bool TextureImportWarningBox(string message){
			GUILayout.BeginVertical(new GUIStyle(EditorStyles.helpBox));
			EditorGUILayout.LabelField(message, new GUIStyle(EditorStyles.label) {
				fontSize = 11, wordWrap = true
			});
			EditorGUILayout.BeginHorizontal(new GUIStyle() {
				alignment = TextAnchor.MiddleRight
			}, GUILayout.Height(24));
			EditorGUILayout.Space();
			bool buttonPress = GUILayout.Button("Fix Now", new GUIStyle("button") {
				stretchWidth = false,
				margin = new RectOffset(0, 0, 0, 0),
				padding = new RectOffset(8, 8, 0, 0)
			}, GUILayout.Height(22));
			EditorGUILayout.EndHorizontal();
			GUILayout.EndVertical();
			return buttonPress;
		}

		public static void sRGBWarning(MaterialProperty tex){
			if (tex.textureValue){
				string sRGBWarning = "This texture is marked as sRGB, but should not contain color information.";
				string texPath = AssetDatabase.GetAssetPath(tex.textureValue);
				TextureImporter texImporter;
				var importer = TextureImporter.GetAtPath(texPath) as TextureImporter;
				if (importer != null){
					texImporter = (TextureImporter)importer;
					if (texImporter.sRGBTexture){
						if (TextureImportWarningBox(sRGBWarning)){
							texImporter.sRGBTexture = false;
							texImporter.SaveAndReimport();
						}
					}
				}
			}
		}

		// Shorthand spacing funcs
		public static void SpaceN24(){ GUILayout.Space(-24); }
		public static void SpaceN22(){ GUILayout.Space(-22); }
		public static void SpaceN20(){ GUILayout.Space(-20); }
		public static void SpaceN18(){ GUILayout.Space(-18); }
		public static void SpaceN16(){ GUILayout.Space(-16); }
		public static void SpaceN14(){ GUILayout.Space(-14); }
		public static void SpaceN12(){ GUILayout.Space(-12); }
		public static void SpaceN10(){ GUILayout.Space(-10); }
		public static void SpaceN8(){ GUILayout.Space(-8); }
		public static void SpaceN6(){ GUILayout.Space(-6); }
		public static void SpaceN5(){ GUILayout.Space(-5); }
		public static void SpaceN4(){ GUILayout.Space(-4); }
		public static void SpaceN3(){ GUILayout.Space(-3); }
		public static void SpaceN2(){ GUILayout.Space(-2); }
		public static void SpaceN1(){ GUILayout.Space(-1); }
		public static void Space1(){ GUILayout.Space(1); }
		public static void Space2(){ GUILayout.Space(2); }
		public static void Space3(){ GUILayout.Space(3); }
		public static void Space4(){ GUILayout.Space(4); }
		public static void Space5(){ GUILayout.Space(5); }
		public static void Space6(){ GUILayout.Space(6); }
		public static void Space8(){ GUILayout.Space(8); }
		public static void Space10(){ GUILayout.Space(10); }
		public static void Space12(){ GUILayout.Space(12); }
		public static void Space14(){ GUILayout.Space(14); }
		public static void Space16(){ GUILayout.Space(16); }
		public static void Space18(){ GUILayout.Space(18); }
		public static void Space20(){ GUILayout.Space(20); }
		public static void Space22(){ GUILayout.Space(22); }
		public static void Space24(){ GUILayout.Space(24); }

		public static void SetKeyword(Material mat, string keyword, bool state){
			if (state) mat.EnableKeyword(keyword);
			else mat.DisableKeyword(keyword);
		}
	}
}