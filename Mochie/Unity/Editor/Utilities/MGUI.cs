using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class MGUI {
    
	private static int chanOfs = 105;
	public static bool IsXVersion(Material mat){
		return mat.shader.name.Contains(" X") || mat.shader.name.Contains(" X ");
	}

	// TODO: automatically find queue line
	public static void EditShader(Material mat){
		int line = 1;
		string path = AssetDatabase.GetAssetPath(mat.shader);
		bool isUber = mat.shader.name.Contains("Uber");
		bool isParticle = mat.shader.name.Contains("Particle");
		bool isMSFX = mat.shader.name.Contains("Screen FX");
		if (isUber)
			line = 267;
		else if (isParticle)
			line = 58;
		else if (isMSFX)
			line = 197;
		UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(path, line);
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
		me.TexturePropertySingleLine(new GUIContent(label), mask, hasTex ? maskStr : null, hasTex ? maskChannel : null);
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
		bool showProps = isToggled && mask.textureValue;
		me.TexturePropertySingleLine(new GUIContent(label), mask, showProps ? maskChannel : null);
		if (showProps){
			TexPropLabel("Channel", chanOfs);
		}
	}

	public static bool LinkButton(Texture2D tex, float width, float height, float xPos){
		Rect buttonRect = EditorGUILayout.GetControlRect();
		buttonRect.width = width;
		buttonRect.height = height;
		buttonRect.x += ((GetInspectorWidth()/2.0f)-width/2.0f)-xPos;
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

	public static void DoCollapseAllButton(Dictionary<Material, Toggles> foldouts, Material mat){
        float lw = EditorGUIUtility.labelWidth;
        float iw = GetInspectorWidth();
        float width = (iw-lw)/3;

		if (SimpleButton("All", width, lw)){
			for (int i = 0; i <= foldouts[mat].GetToggles().Length-1; i++)
				foldouts[mat].SetState(i, false);
		}
	}

	public static void DoCollapseMainButton(Dictionary<Material, Toggles> foldouts, Material mat){
		GUILayout.Space(-20);
		float lw = EditorGUIUtility.labelWidth;
        float iw = GetInspectorWidth();
        float width = (iw-lw)/3;
		lw += width;

		if (SimpleButton("Main", width, lw)){
			int[] mains = foldouts[mat].GetMain();
			for (int i = 0; i < mains.Length; i++){
				foldouts[mat].SetState(mains[i], false);
			}
		}
	}

	public static void DoCollapseSubButton(Dictionary<Material, Toggles> foldouts, Material mat){
		GUILayout.Space(-20);
        float lw = EditorGUIUtility.labelWidth;
        float iw = GetInspectorWidth();
        float width = (iw-lw)/3;
		lw += width*2;

		if (SimpleButton("Sub", width, lw)){
			int[] subs = foldouts[mat].GetSub();
			for (int i = 0; i < subs.Length; i++){
				foldouts[mat].SetState(subs[i], false);
			}
		}
	}

	public static void DoCollapseButtons(Dictionary<Material, Toggles> foldouts, Material mat){
		DoCollapseAllButton(foldouts, mat);
		DoCollapseMainButton(foldouts, mat);
		DoCollapseSubButton(foldouts, mat);
		GUILayout.Space(-17);
		GUILayout.Label("Collapse Tabs");
		Space4();
	}

	public static bool ResetButton(){
		return SimpleButton("Reset", GetPropertyWidth(), EditorGUIUtility.labelWidth);
	}

	public static void DoCloneResetButton(MaterialProperty[] props){
		if (ResetButton()){
			int i = 0;
			foreach (MaterialProperty p in props){
				Vector4 v = USEditor.clonePositions[i];
				props[i].vectorValue = v;
				i++;
			};
		}
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
	
	public static bool DoFoldout(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, string header){
		foldouts[mat].SetState(header, Foldout(header, foldouts[mat].GetState(header)));
		return foldouts[mat].GetState(header);
	}

    public static bool Foldout(string header, bool display){
        GUIStyle formatting = new GUIStyle("ShurikenModuleTitle");
        formatting.contentOffset = new Vector2(20.0f, -3.0f);
        formatting.fixedHeight = 28.0f;
        formatting.font = new GUIStyle(EditorStyles.boldLabel).font;
		formatting.fontSize = 10;
        Rect rect = GUILayoutUtility.GetRect(5.0f, 28.0f, formatting);
        rect.x -= 8.0f;
        rect.width = GetInspectorWidth()+8.0f;
        GUI.Box(rect, header, formatting);
        return DoToggle(display, rect);
    }

    public static bool DoToggle(bool show, Rect rect){
        Event currentEvent = Event.current;
        Rect arrowRect = new Rect(rect.x+4f, rect.y+4f, 0.0f, 0.0f);
        if (currentEvent.type == EventType.Repaint)
            EditorStyles.foldout.Draw(arrowRect, false, false, show, false);
        if (currentEvent.type == EventType.MouseDown && rect.Contains(currentEvent.mousePosition)){
            show = !show;
            currentEvent.Use();
        }
		GUILayout.Space(-2);
        return show;
    }

	public static bool DoMediumFoldout(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, MaterialProperty prop, string header){
		foldouts[mat].SetState(header, MediumFoldout(header, foldouts[mat].GetState(header)));
		me.ShaderProperty(prop, " ");
		Space4();
		return foldouts[mat].GetState(header);
	}

	public static bool DoMediumFoldout(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, string header){
		foldouts[mat].SetState(header, MediumFoldout(header, foldouts[mat].GetState(header)));
		GUILayout.Space(24);
		return foldouts[mat].GetState(header);
	}

	public static bool MediumFoldout(string header, bool display){
        GUIStyle formatting = new GUIStyle("ShurikenModuleTitle");
		float lw = EditorGUIUtility.labelWidth;
        formatting.contentOffset = new Vector2(20f, -3f);
        formatting.fixedHeight = 22.0f;
		formatting.fixedWidth = GetInspectorWidth()+4;
        formatting.font = new GUIStyle(EditorStyles.boldLabel).font;
		formatting.fontSize = 11;
        Rect rect = GUILayoutUtility.GetRect(5.0f, 20.0f, formatting);
		rect.x -= 4.0f;
        rect.width = lw;
        GUI.Box(rect, header, formatting);
        return DoMedToggle(display, rect);
	}

	public static bool DoMedToggle(bool show, Rect rect){
        Event currentEvent = Event.current;
        Rect arrowRect = new Rect(rect.x+2.0f, rect.y+2.0f, 0.0f, 0.0f);
        if (currentEvent.type == EventType.Repaint)
            EditorStyles.foldout.Draw(arrowRect, false, false, show, false);
        if (currentEvent.type == EventType.MouseDown && rect.Contains(currentEvent.mousePosition)){
            show = !show;
            currentEvent.Use();
        }
        GUILayout.Space(-22);
        return show;
    }

	public static bool DoSmallFoldout(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, string header){
		foldouts[mat].SetState(header, SmallFoldout(header, foldouts[mat].GetState(header)));
		return foldouts[mat].GetState(header);
	}

    public static bool SmallFoldout(string header, bool display){
        float lw = EditorGUIUtility.labelWidth-13;
        GUILayoutOption clickArea = GUILayout.MaxWidth(lw);
        Rect rect = GUILayoutUtility.GetRect(0, 18.0f, clickArea);
        GUILayout.Space(-20);
        header = "    " + header;
        EditorGUILayout.LabelField(header);
		GUILayout.Space(20);
        return DoSmallToggle(display, rect);
    }

    public static bool DoSmallToggle(bool show, Rect rect){
        Event currentEvent = Event.current;
        Rect arrowRect = new Rect(rect.x+2.0f, rect.y, 0.0f, 0.0f);
        if (currentEvent.type == EventType.Repaint)
            EditorStyles.foldout.Draw(arrowRect, false, false, show, false);
        if (currentEvent.type == EventType.MouseDown && rect.Contains(currentEvent.mousePosition)){
            show = !show;
            currentEvent.Use();
        }
        GUILayout.Space(-18);
        return show;
    }

    // Slider with a toggle
    public static void ToggleSlider(MaterialEditor me, string label, MaterialProperty toggle, MaterialProperty slider){
        float lw = EditorGUIUtility.labelWidth;
        float indent = lw + 25.0f;
        GUILayoutOption clickArea = GUILayout.MaxWidth(lw+13.0f);
        toggle.floatValue = EditorGUILayout.Toggle(label, toggle.floatValue==1, clickArea)?1:0;
        GUILayout.Space(-18);
        Rect r = EditorGUILayout.GetControlRect();
        r.x += indent;
        r.width -= indent;
        EditorGUI.BeginDisabledGroup(toggle.floatValue == 0);
        me.RangeProperty(r, slider, "");
        EditorGUI.EndDisabledGroup();
    }

	public static void ToggleVector3(string label, MaterialProperty toggle, MaterialProperty vec){
		GUILayout.Space(-2);
        Vector4 newVec = vec.vectorValue;
        float labelWidth = EditorGUIUtility.labelWidth;
        float fieldWidth = (GetPropertyWidth()/3)-6f;

        Rect r = EditorGUILayout.GetControlRect();
        r.x += labelWidth+18.0f;

		GUILayout.Space(-19);
		GUILayoutOption clickArea = GUILayout.MaxWidth(labelWidth+7.0f);
		toggle.floatValue = EditorGUILayout.Toggle(label, toggle.floatValue==1, clickArea)?1:0;
		EditorGUIUtility.labelWidth = 13.0f;
		EditorGUI.BeginDisabledGroup(toggle.floatValue == 0);

			// X Field
			r.width = fieldWidth-1.0f;
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
		GUILayout.Space(-2);
        Vector4 newVec = vec.vectorValue;
        float labelWidth = EditorGUIUtility.labelWidth;
        float fieldWidth = GetPropertyWidth()/3;

		EditorGUILayout.LabelField(label);
		GUILayout.Space(-18);
        Rect r = EditorGUILayout.GetControlRect();
        r.x += labelWidth;
		EditorGUIUtility.labelWidth = 13.0f;

		// X Field
		r.width = fieldWidth-1.0f;
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
		GUILayout.Space(-2);
        Vector4 newVec = vec.vectorValue;
        float labelWidth = EditorGUIUtility.labelWidth;
        float fieldWidth = GetPropertyWidth()/2;

		EditorGUILayout.LabelField(label);
		GUILayout.Space(-18);
        Rect r = EditorGUILayout.GetControlRect();
        r.x += labelWidth;
		EditorGUIUtility.labelWidth = 13.0f;

		// X Field
		r.width = fieldWidth-1.0f;
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

	// Draws a tinted box behind properties
    public static void ContentBox(int boxSize){
        Rect pos = GUILayoutUtility.GetRect(0f, boxSize);
        pos.width = GetInspectorWidth()+6.0f;
        pos.x -= 4.0f;
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
		pos.x -= 5.0f;
		GUI.Box(pos, "");
		GUILayout.Space(-8);
	}

    // Need this because the provided parameter doesn't include the width of the scrollbar
    static float GetInspectorWidth(){
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        return GUILayoutUtility.GetLastRect().width;
    }

	static float GetPropertyWidth(){
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
        GUILayout.Space(-2);
		Vector2Field("Scrolling", vec);
	}

	public static string QueueText(int q){
		string text = "";
		if (q < 1000)
			text += "Background-" + (1000-q);
		else if (q == 1000)
			text += "Background";
		else if (q > 1000 && q < 1500)
			text += "Background+" + (q-1000);
		else if (q >= 1500 && q < 2000)
			text += "Geometry-" + (2000-q);
		else if (q == 2000)
			text += "Geometry";
		else if (q > 2000 && q < 2225)
			text += "Geometry+" + (q-2000);
		else if (q >= 2225 && q < 2450)
			text += "AlphaTest-" + (2450-q);
		else if (q == 2450)
			text += "AlphaTest";
		else if (q > 2450 && q < 2725)
			text += "AlphaTest+" + (q-2450);
		else if (q >= 2725 && q < 3000)
			text += "Transparent-" + (3000-q);
		else if (q == 3000)
			text += "Transparent";
		else if (q > 3000 && q < 3500)
			text += "Transparent+" + (q-3000);
		else if (q >= 3500 && q < 4000)
			text += "Overlay-" + (4000-q);
		else if (q == 4000)
			text += "Overlay";
		else if (q > 4000 && q < 5000)
			text += "Overlay+" + (q-4000);
		else if (q >= 5000)
			text += "Okay then buddy";

		if (q < 5000)
			text += " (" + q + ")";
		return text;
	}

	// Generate text for render queue display
    public static void RenderQueueLabel(Material mat){
		string text = QueueText(mat.renderQueue);
        DummyProperty("Queue: ", text);
		float lw = EditorGUIUtility.labelWidth;
        float iw = GetInspectorWidth();
		GUILayout.Space(-20);
		if (SimpleButton("Edit", 50, iw-50)){
			EditShader(mat);
		}
    }

	// Shorthand disable group stuff
	public static void ToggleGroup(bool isToggled){
		EditorGUI.BeginDisabledGroup(isToggled);
	}
	public static void ToggleGroupEnd(){
		EditorGUI.EndDisabledGroup();
	}

	public static void SpaceN2(){ GUILayout.Space(-2); }
	public static void Space2(){ GUILayout.Space(2); }
	public static void Space4(){ GUILayout.Space(4); }
	public static void Space6(){ GUILayout.Space(6); }
	public static void Space8(){ GUILayout.Space(8); }

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

    // bool SmallFoldout(string header, bool display, float height){
    //     GUIStyle formatting = new GUIStyle("ShurikenModuleTitle");
    //     GUILayoutOption clickArea = GUILayout.MaxWidth(EditorGUIUtility.labelWidth-13);
    //     formatting.contentOffset = new Vector2(14.0f, -2.0f);
    //     formatting.fixedHeight = height;
    //     formatting.fontSize = 9;
    //     formatting.wordWrap = true;
    //     Rect boxRect = GUILayoutUtility.GetRect(5.0f, height, formatting);
    //     GUI.Box(boxRect, header, formatting);
    //     GUILayout.Space(-height);
    //     Rect toggleRect = GUILayoutUtility.GetRect(5.0f, height, formatting, clickArea);
    //     return DoSmallToggle(display, toggleRect);
    // }

    // bool DoSmallToggle(bool show, Rect rect){
    //     Event currentEvent = Event.current;
    //     Rect arrowRect = new Rect(rect.x+1.0f, rect.y+1.5f, 0.0f, 0.0f);
    //     if (currentEvent.type == EventType.Repaint)
    //         EditorStyles.foldout.Draw(arrowRect, false, false, show, false);
    //     if (currentEvent.type == EventType.MouseDown && rect.Contains(currentEvent.mousePosition)){
    //         show = !show;
    //         currentEvent.Use();
    //     }
    //     GUILayout.Space(-22.0f);
    //     return show;
    // }

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
	// 	pos.width = GetPropertyWidth()-24.0f;
	// 	pos.height = 0.5f;
	// 	pos.x += EditorGUIUtility.labelWidth+24.0f;
	// 	GUI.Box(pos, "");
	// 	GUILayout.Space(-8);
	// }