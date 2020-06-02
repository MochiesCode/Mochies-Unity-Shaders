// A collection of UI functions I've developed over the years to improve customization of editor scripts
// By Mochie

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

public static class MGUI {

    public enum BlendMode {Opaque, Cutout, Fade, Transparent}

	private static int chanOfs = 105;
	public static string parentPath = "Assets/Mochie/Unity/Presets";
	public static string presetPath = "Assets/Mochie/Unity";

	public static bool IsXVersion(Material mat){
		return mat.shader.name.Contains(" X") || mat.shader.name.Contains(" X ");
	}

	public static void SetBlendMode(Material material, BlendMode blendMode){
		switch (blendMode){
			
			case BlendMode.Opaque:
				material.SetOverrideTag("RenderType", "");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_ZWrite", 1);
				material.DisableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = -1;
				break;

			case BlendMode.Cutout:
				material.SetOverrideTag("RenderType", "TransparentCutout");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_ZWrite", 1);
				material.EnableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
				break;

			case BlendMode.Fade:
				material.SetOverrideTag("RenderType", "Transparent");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_ALPHATEST_ON");
				material.EnableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
				break;

			case BlendMode.Transparent:
				material.SetOverrideTag("RenderType", "Transparent");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
				break;

			default: break;
		}
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

	public static void FramebufferSection(MaterialEditor me, MaterialProperty[] toggles, MaterialProperty gs){
		for (int i = 0; i < toggles.Length; i++){
			if (i == 0)
				me.ShaderProperty(toggles[i], toggles[i].displayName);
			else 
				ToggleSlider(me, "Ghosting", toggles[1], gs);
			if (toggles[i].floatValue == 1){
				for (int j = 0; j < toggles.Length; j++){
					if (j != i)
						toggles[j].floatValue = 0;
				}
			}
		}
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
	
	public static void MaskProperty(MaterialEditor me, MaterialProperty mask, MaterialProperty maskChannel){
		bool hasTex = mask.textureValue;
		me.TexturePropertySingleLine(new GUIContent("Mask"), mask, hasTex ? maskChannel : null);
		if (hasTex){
			TexPropLabel("Channel", chanOfs);
		}
	}

	public static void MaskProperty(MaterialEditor me, string label, MaterialProperty mask, MaterialProperty maskStr, MaterialProperty maskChannel){
		bool hasTex = mask.textureValue;
		me.TexturePropertySingleLine(new GUIContent(label), mask, !hasTex ? maskStr : null, hasTex ? maskChannel : null);
		if (hasTex){
			TexPropLabel("Channel", chanOfs);
		}
	}

	public static void MaskProperty(MaterialEditor me, string label, MaterialProperty mask, MaterialProperty maskChannel){
		bool hasTex = mask.textureValue;
		me.TexturePropertySingleLine(new GUIContent(label), mask, hasTex ? maskChannel : null);
		if (hasTex){
			TexPropLabel("Channel", chanOfs);
		}
	}

	public static void MaskProperty(MaterialEditor me, string label, MaterialProperty mask, MaterialProperty maskChannel, bool isToggled){
		bool displayProps = isToggled && mask.textureValue;
		me.TexturePropertySingleLine(new GUIContent(label), mask, displayProps ? maskChannel : null);
		if (displayProps){
			TexPropLabel("Channel", chanOfs);
		}
	}

	public static bool LinkButton(Texture2D tex, float width, float height, float xPos){
		Rect buttonRect = EditorGUILayout.GetControlRect();
		buttonRect.width = width;
		buttonRect.height = height;
		buttonRect.x += ((GetInspectorWidth()/2f)-width/2f)-xPos;
		return GUI.Button(buttonRect, tex);
	}

	public static void DummyProperty(string label, string property){
		Rect r = EditorGUILayout.GetControlRect();
		GUI.Label(r, label);
		r.x += EditorGUIUtility.labelWidth;
		GUI.Label(r, property);
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

	public static bool MedTabButton(Texture2D tex, float offset){
		GUILayout.Space(-25);
		Rect buttonRect = EditorGUILayout.GetControlRect();
		buttonRect.width = 23;
		buttonRect.height = 19;
		buttonRect.x += GetInspectorWidth()-offset;
		return GUI.Button(buttonRect, tex);
	}


    // Slider with a toggle
    public static void ToggleSlider(MaterialEditor me, string label, MaterialProperty toggle, MaterialProperty slider){
        float lw = EditorGUIUtility.labelWidth;
        float indent = lw + 25f;
        GUILayoutOption clickArea = GUILayout.MaxWidth(lw+13f);
        toggle.floatValue = EditorGUILayout.Toggle(label, toggle.floatValue==1, clickArea)?1:0;
        GUILayout.Space(-18);
        Rect r = EditorGUILayout.GetControlRect();
        r.x += indent;
        r.width -= indent;
        EditorGUI.BeginDisabledGroup(toggle.floatValue == 0);
        me.RangeProperty(r, slider, "");
        EditorGUI.EndDisabledGroup();
    }

    public static void ToggleIntSlider(MaterialEditor me, string label, MaterialProperty toggle, MaterialProperty slider){
        float lw = EditorGUIUtility.labelWidth;
        float indent = lw + 25f;
        GUILayoutOption clickArea = GUILayout.MaxWidth(lw+13f);
        toggle.floatValue = EditorGUILayout.Toggle(label, toggle.floatValue==1, clickArea)?1:0;
        GUILayout.Space(-18);
        Rect r = EditorGUILayout.GetControlRect();
        r.x += indent;
        r.width -= indent;
        EditorGUI.BeginDisabledGroup(toggle.floatValue == 0);

        slider.floatValue = (int)EditorGUI.Slider(r, slider.floatValue, slider.rangeLimits.x, slider.rangeLimits.y);
        EditorGUI.EndDisabledGroup();
    }

    // Float with a toggle
    public static void ToggleFloat(MaterialEditor me, string label, MaterialProperty toggle, MaterialProperty floatProp){
        float lw = EditorGUIUtility.labelWidth;
        float indent = lw + 20f;
        GUILayoutOption clickArea = GUILayout.MaxWidth(lw+13f);
        toggle.floatValue = EditorGUILayout.Toggle(label, toggle.floatValue==1, clickArea)?1:0;
        GUILayout.Space(-18);
        Rect r = EditorGUILayout.GetControlRect();
        r.x += indent;
        r.width -= indent;
        EditorGUI.BeginDisabledGroup(toggle.floatValue == 0);
        me.FloatProperty(r, floatProp, "");
        EditorGUI.EndDisabledGroup();
    }

	public static void ToggleVector3(string label, MaterialProperty toggle, MaterialProperty vec){
		SpaceN2();
        Vector4 newVec = vec.vectorValue;
        float labelWidth = EditorGUIUtility.labelWidth;
        float fieldWidth = (GetPropertyWidth()/3)-6f;

        Rect r = EditorGUILayout.GetControlRect();
        r.x += labelWidth+18f;

		GUILayout.Space(-19);
		GUILayoutOption clickArea = GUILayout.MaxWidth(labelWidth+7f);
		toggle.floatValue = EditorGUILayout.Toggle(label, toggle.floatValue==1, clickArea)?1:0;
		EditorGUIUtility.labelWidth = 13f;
		EditorGUI.BeginDisabledGroup(toggle.floatValue == 0);

			// X Field
			r.width = fieldWidth-1f;
			newVec.x = EditorGUI.FloatField(r, "X", newVec.x);
			
			// Y Field
			r.x += fieldWidth+1;
			newVec.y = EditorGUI.FloatField(r, "Y", newVec.y);

			// Z Field
			r.x += fieldWidth;
			newVec.z = EditorGUI.FloatField(r, "Z", newVec.z);

			EditorGUIUtility.labelWidth = labelWidth;
			vec.vectorValue = newVec;
		EditorGUI.EndDisabledGroup();
		GUILayout.Space(1);
	}

    // Vector3 property with corrected width scaling
    public static void Vector3Field(MaterialProperty vec, string label){
		SpaceN2();
        Vector4 newVec = vec.vectorValue;
        float labelWidth = EditorGUIUtility.labelWidth;
        float fieldWidth = GetPropertyWidth()/3;

		EditorGUILayout.LabelField(label);
		GUILayout.Space(-18);
        Rect r = EditorGUILayout.GetControlRect();
        r.x += labelWidth;
		EditorGUIUtility.labelWidth = 13f;

		// X Field
		r.width = fieldWidth-1f;
        newVec.x = EditorGUI.FloatField(r, "X", newVec.x);
		
		// Y Field
		r.x += fieldWidth+1;
		newVec.y = EditorGUI.FloatField(r, "Y", newVec.y);

		// Z Field
		r.x += fieldWidth;
		newVec.z = EditorGUI.FloatField(r, "Z", newVec.z);

		EditorGUIUtility.labelWidth = labelWidth;
        vec.vectorValue = newVec;
    }

    // Vector2 property with corrected width scaling
    public static void Vector2Field(string label, MaterialProperty vec){
		SpaceN2();
        Vector4 newVec = vec.vectorValue;
        float labelWidth = EditorGUIUtility.labelWidth;
        float fieldWidth = GetPropertyWidth()/2;

		EditorGUILayout.LabelField(label);
		GUILayout.Space(-18);
        Rect r = EditorGUILayout.GetControlRect();
        r.x += labelWidth;
		EditorGUIUtility.labelWidth = 13f;

		// X Field
		r.width = fieldWidth-1f;
        newVec.x = EditorGUI.FloatField(r, "X", newVec.x);
		
		// Y Field
		r.x += fieldWidth+1;
		newVec.y = EditorGUI.FloatField(r, "Y", newVec.y);

		EditorGUIUtility.labelWidth = labelWidth;
        vec.vectorValue = newVec;
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

    // Label for the third property in TexturePropertySingleLine
    public static void TexPropLabel(string text, int offset){
        GUILayout.Space(-20);
        Rect rm = EditorGUILayout.GetControlRect();
        rm.x += GetInspectorWidth()-offset;
        EditorGUI.LabelField(rm, text);
    }

    public static void PropLabel(string text, int offset){
        GUILayout.Space(-18);
        Rect rm = EditorGUILayout.GetControlRect();
        rm.x += EditorGUIUtility.labelWidth+offset+14.0f;
        EditorGUI.LabelField(rm, text);
    }
	
	// Draws a tinted box behind properties
    public static void ContentBox(int boxSize){
        Rect pos = GUILayoutUtility.GetRect(0f, boxSize);
        pos.width = GetInspectorWidth()+6f;
        pos.x -= 4f;
        Space4();
        GUI.Box(pos, "");
        GUILayout.Space(-boxSize);
    }
	
	// Draws a line across the inspector window
	static public void Divider(){
		Space4();
		Rect pos = EditorGUILayout.GetControlRect();
		pos.width = GetInspectorWidth();
		pos.height = 0.5f;
		pos.x -= 5f;
		GUI.Box(pos, "");
		GUILayout.Space(-8);
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
        Space2();
    }

	// Scale offset property with added scrolling x/y
	public static void TextureSOScroll(MaterialEditor me, MaterialProperty tex, MaterialProperty vec){
		me.TextureScaleOffsetProperty(tex);
        SpaceN2();
		Vector2Field("Scrolling", vec);
	}

	public static void TextureSOScroll(MaterialEditor me, MaterialProperty tex, MaterialProperty vec, bool shouldDisplay){
		if (shouldDisplay){
			me.TextureScaleOffsetProperty(tex);
			SpaceN2();
			Vector2Field("Scrolling", vec);
		}
	}

    // Shorthand Scale Offset func with fixed spacing
    public static void TextureSO(MaterialEditor me, MaterialProperty prop, bool shouldDisplay){
		if (shouldDisplay){
			me.TextureScaleOffsetProperty(prop);
			Space2();
		}
    }

	// Shorthand for displaying an error window
	public static void ErrorBox(string message){
		EditorUtility.DisplayDialog("Error", message, "Close");
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

	// Shorthand spacing funcs
	public static void SpaceN16(){ GUILayout.Space(-16); }
	public static void SpaceN14(){ GUILayout.Space(-14); }
	public static void SpaceN12(){ GUILayout.Space(-12); }
	public static void SpaceN10(){ GUILayout.Space(-10); }
	public static void SpaceN8(){ GUILayout.Space(-8); }
	public static void SpaceN6(){ GUILayout.Space(-6); }
	public static void SpaceN4(){ GUILayout.Space(-4); }
	public static void SpaceN2(){ GUILayout.Space(-2); }
	public static void Space2(){ GUILayout.Space(2); }
	public static void Space4(){ GUILayout.Space(4); }
	public static void Space6(){ GUILayout.Space(6); }
	public static void Space8(){ GUILayout.Space(8); }
	public static void Space10(){ GUILayout.Space(10); }
	public static void Space12(){ GUILayout.Space(12); }
	public static void Space14(){ GUILayout.Space(14); }
	public static void Space16(){ GUILayout.Space(16); }

	public static void SetKeyword(Material mat, string keyword, bool state){
		if (state) mat.EnableKeyword(keyword);
		else mat.DisableKeyword(keyword);
	}

	public static void CustomToggleSlider(string label, MaterialProperty toggle, MaterialProperty value, float min, float max){
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

		GUILayout.Space(-18);
		toggle.floatValue = EditorGUILayout.Toggle(" ", toggle.floatValue==1, clickArea)?1:0;
		EditorGUI.BeginDisabledGroup(toggle.floatValue == 0);
		value.floatValue = GUI.HorizontalSlider(r0, value.floatValue, min, max);
		value.floatValue = EditorGUI.IntField(r1, (int)value.floatValue);
		EditorGUI.EndDisabledGroup();
	}
}

// public public void FoldoutDivider(){
// 	Rect pos = EditorGUILayout.GetControlRect();
// 	pos.width = GetPropertyWidth();
// 	pos.height = 0.5f;
// 	pos.x += EditorGUIUtility.labelWidth;
// 	GUI.Box(pos, "");
// }

// public public void FoldoutDividerToggle(){
// 	GUILayout.Space(-10);
// 	Rect pos = EditorGUILayout.GetControlRect();
// 	pos.width = GetPropertyWidth()-24f;
// 	pos.height = 0.5f;
// 	pos.x += EditorGUIUtility.labelWidth+24f;
// 	GUI.Box(pos, "");
// 	GUILayout.Space(-8);
// }