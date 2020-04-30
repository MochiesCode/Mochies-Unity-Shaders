using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEditor;

public class Foldouts {

	private static bool foldoutClicked = false;
	private static Color errorCol = new Color(1f,0.3f,0.3f,1f);

	public static bool DoFoldout(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, string header){
		foldouts[mat].SetState(header, Foldout(header, foldouts[mat].GetState(header), me));
		return foldouts[mat].GetState(header);
	}

	public static bool DoFoldoutShading(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, MaterialProperty[] props, string header){
		foldouts[mat].SetState(header, FoldoutShading(header, foldouts[mat].GetState(header), me, mat, props));
		return foldouts[mat].GetState(header);
	}

	public static bool DoFoldoutSpecial(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, MaterialProperty[] props, string header){
		foldouts[mat].SetState(header, FoldoutSpecial(header, foldouts[mat].GetState(header), me, mat, props));
		return foldouts[mat].GetState(header);
	}

	public static bool DoMediumFoldout(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, MaterialProperty prop, string header){
		foldouts[mat].SetState(header, MediumFoldout(header, foldouts[mat].GetState(header), me));
		me.ShaderProperty(prop, " ");
		MGUI.Space4();
		return foldouts[mat].GetState(header);
	}

	public static bool DoMediumFoldout(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, string header){
		foldouts[mat].SetState(header, MediumFoldout(header, foldouts[mat].GetState(header), me));
		GUILayout.Space(24);
		return foldouts[mat].GetState(header);
	}

	public static bool DoMediumFoldoutSSR(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, MaterialProperty[] props, string header){
		foldouts[mat].SetState(header, MediumFoldoutSSR(header, foldouts[mat].GetState(header), me, mat, props));
		me.ShaderProperty(props[0], " ");
		MGUI.Space4();
		return foldouts[mat].GetState(header);
	}

	public static bool DoMediumFoldoutSpecial(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, MaterialProperty[] props, string header){
		foldouts[mat].SetState(header, MediumFoldoutSpecial(header, foldouts[mat].GetState(header), me, mat, props));
		me.ShaderProperty(props[0], " ");
		MGUI.Space4();
		return foldouts[mat].GetState(header);
	}

	public static bool DoMediumFoldoutMatcap(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, MaterialProperty[] props, string header){
		foldouts[mat].SetState(header, MediumFoldoutMatcap(header, foldouts[mat].GetState(header), me, mat, props));
		me.ShaderProperty(props[0], " ");
		MGUI.Space4();
		return foldouts[mat].GetState(header);
	}

    public static bool Foldout(string header, bool display, MaterialEditor me){
        GUIStyle formatting = new GUIStyle("ShurikenModuleTitle");
		formatting.font = new GUIStyle(EditorStyles.boldLabel).font;
        formatting.contentOffset = new Vector2(20f, -3f);
		formatting.hover.textColor = Color.gray;
        formatting.fixedHeight = 28f;
		formatting.fontSize = 10;

        Rect rect = GUILayoutUtility.GetRect(MGUI.GetInspectorWidth(), formatting.fixedHeight, formatting);
		rect.width += 8f;
        rect.x -= 8f;

		Event evt = Event.current;
		Color bgCol = GUI.backgroundColor;
		bool mouseOver = rect.Contains(evt.mousePosition);
		
		if (evt.type == EventType.MouseDown && mouseOver){
			foldoutClicked = true;
			evt.Use();
		}
			
		else if (evt.type == EventType.Repaint && foldoutClicked && mouseOver){
			GUI.backgroundColor = Color.gray;
			me.Repaint();
		}
		
        GUI.Box(rect, header, formatting);
		GUI.backgroundColor = bgCol;

		return FoldoutToggle(rect, evt, mouseOver, display, 0);
    }

	public static bool MediumFoldout(string header, bool display, MaterialEditor me){
        GUIStyle formatting = new GUIStyle("ShurikenModuleTitle");
		float lw = EditorGUIUtility.labelWidth;
        formatting.contentOffset = new Vector2(20f, -2f);
        formatting.fixedHeight = 22f;
		formatting.fixedWidth = MGUI.GetInspectorWidth()+4f;
		formatting.fontSize = 10;

        Rect rect = GUILayoutUtility.GetRect(0f, 20f, formatting);
		rect.x -= 4f;
        rect.width = lw;

		Event evt = Event.current;
		Color bgCol = GUI.backgroundColor;
		bool mouseOver = rect.Contains(evt.mousePosition);
		
		if (evt.type == EventType.MouseDown && mouseOver){
			foldoutClicked = true;
			evt.Use();
		}
			
		else if (evt.type == EventType.Repaint && foldoutClicked && mouseOver){
			GUI.backgroundColor = Color.gray;
			me.Repaint();
		}

        GUI.Box(rect, header, formatting);
		GUI.backgroundColor = bgCol;
		
		return FoldoutToggle(rect, evt, mouseOver, display, 1);
	}

    public static bool FoldoutShading(string header, bool display, MaterialEditor me, Material mat, MaterialProperty[] props){
        GUIStyle formatting = new GUIStyle("ShurikenModuleTitle");
		formatting.font = new GUIStyle(EditorStyles.boldLabel).font;
        formatting.contentOffset = new Vector2(20f, -3f);
		formatting.hover.textColor = Color.gray;
        formatting.fixedHeight = 28f;
		formatting.fontSize = 10;

        Rect rect = GUILayoutUtility.GetRect(MGUI.GetInspectorWidth(), formatting.fixedHeight, formatting);
		rect.width += 8f;
        rect.x -= 8f;

		Event evt = Event.current;
		Color bgCol = GUI.backgroundColor;
		
		bool ssrError = props[0].floatValue == 1 && props[1].floatValue == 1 && mat.renderQueue < 2501;
		bool matcapError = props[1].floatValue == 0 && props[2].floatValue == 0 && props[3].floatValue == 1 && props[4].floatValue == 1;
		if (ssrError || matcapError)
			GUI.backgroundColor = errorCol;

		bool mouseOver = rect.Contains(evt.mousePosition);
		if (evt.type == EventType.MouseDown && mouseOver){
			foldoutClicked = true;
			evt.Use();
		}
			
		else if (evt.type == EventType.Repaint && foldoutClicked && mouseOver){
			GUI.backgroundColor = Color.gray;
			me.Repaint();
		}
		
        GUI.Box(rect, header, formatting);
		GUI.backgroundColor = bgCol;

		return FoldoutToggle(rect, evt, mouseOver, display, 0);
    }

	public static bool MediumFoldoutSSR(string header, bool display, MaterialEditor me, Material mat, MaterialProperty[] props){
        GUIStyle formatting = new GUIStyle("ShurikenModuleTitle");
		float lw = EditorGUIUtility.labelWidth;
        formatting.contentOffset = new Vector2(20f, -2f);
        formatting.fixedHeight = 22f;
		formatting.fixedWidth = MGUI.GetInspectorWidth()+4f;
		formatting.fontSize = 10;

        Rect rect = GUILayoutUtility.GetRect(0f, 20f, formatting);
		rect.x -= 4f;
        rect.width = lw;

		Event evt = Event.current;
		Color bgCol = GUI.backgroundColor;

		if (props[0].floatValue == 1 && props[1].floatValue == 1 && mat.renderQueue < 2501)
			GUI.backgroundColor = errorCol;

		bool mouseOver = rect.Contains(evt.mousePosition);
		if (evt.type == EventType.MouseDown && mouseOver){
			foldoutClicked = true;
			evt.Use();
		}
			
		else if (evt.type == EventType.Repaint && foldoutClicked && mouseOver){
			GUI.backgroundColor = Color.gray;
			me.Repaint();
		}

        GUI.Box(rect, header, formatting);
		GUI.backgroundColor = bgCol;
		
		return FoldoutToggle(rect, evt, mouseOver, display, 1);
	}

	public static bool MediumFoldoutMatcap(string header, bool display, MaterialEditor me, Material mat, MaterialProperty[] props){
        GUIStyle formatting = new GUIStyle("ShurikenModuleTitle");
		float lw = EditorGUIUtility.labelWidth;
        formatting.contentOffset = new Vector2(20f, -2f);
        formatting.fixedHeight = 22f;
		formatting.fixedWidth = MGUI.GetInspectorWidth()+4f;
		formatting.fontSize = 10;

        Rect rect = GUILayoutUtility.GetRect(0f, 20f, formatting);
		rect.x -= 4f;
        rect.width = lw;

		Event evt = Event.current;
		Color bgCol = GUI.backgroundColor;

		if (props[0].floatValue == 1 && props[1].floatValue == 0 && props[2].floatValue == 0  && props[3].floatValue == 1)
			GUI.backgroundColor = errorCol;

		bool mouseOver = rect.Contains(evt.mousePosition);
		if (evt.type == EventType.MouseDown && mouseOver){
			foldoutClicked = true;
			evt.Use();
		}
			
		else if (evt.type == EventType.Repaint && foldoutClicked && mouseOver){
			GUI.backgroundColor = Color.gray;
			me.Repaint();
		}

        GUI.Box(rect, header, formatting);
		GUI.backgroundColor = bgCol;
		
		return FoldoutToggle(rect, evt, mouseOver, display, 1);
	}

    public static bool FoldoutSpecial(string header, bool display, MaterialEditor me, Material mat, MaterialProperty[] props){
        GUIStyle formatting = new GUIStyle("ShurikenModuleTitle");
		formatting.font = new GUIStyle(EditorStyles.boldLabel).font;
        formatting.contentOffset = new Vector2(20f, -3f);
		formatting.hover.textColor = Color.gray;
        formatting.fixedHeight = 28f;
		formatting.fontSize = 10;

        Rect rect = GUILayoutUtility.GetRect(MGUI.GetInspectorWidth(), formatting.fixedHeight, formatting);
		rect.width += 8f;
        rect.x -= 8f;

		Event evt = Event.current;
		Color bgCol = GUI.backgroundColor;

		if (props[0].floatValue == 0 && (props[1].floatValue > 0 || props[2].floatValue > 0))
			GUI.backgroundColor = errorCol;

		bool mouseOver = rect.Contains(evt.mousePosition);
		if (evt.type == EventType.MouseDown && mouseOver){
			foldoutClicked = true;
			evt.Use();
		}
			
		else if (evt.type == EventType.Repaint && foldoutClicked && mouseOver){
			GUI.backgroundColor = Color.gray;
			me.Repaint();
		}
		
        GUI.Box(rect, header, formatting);
		GUI.backgroundColor = bgCol;

		return FoldoutToggle(rect, evt, mouseOver, display, 0);
    }

	public static bool MediumFoldoutSpecial(string header, bool display, MaterialEditor me, Material mat, MaterialProperty[] props){
        GUIStyle formatting = new GUIStyle("ShurikenModuleTitle");
		float lw = EditorGUIUtility.labelWidth;
        formatting.contentOffset = new Vector2(20f, -2f);
        formatting.fixedHeight = 22f;
		formatting.fixedWidth = MGUI.GetInspectorWidth()+4f;
		formatting.fontSize = 10;

        Rect rect = GUILayoutUtility.GetRect(0f, 20f, formatting);
		rect.x -= 4f;
        rect.width = lw;

		Event evt = Event.current;
		Color bgCol = GUI.backgroundColor;

		if (props[0].floatValue > 0 && props[1].floatValue == 0)
			GUI.backgroundColor = errorCol;

		bool mouseOver = rect.Contains(evt.mousePosition);
		if (evt.type == EventType.MouseDown && mouseOver){
			foldoutClicked = true;
			evt.Use();
		}
			
		else if (evt.type == EventType.Repaint && foldoutClicked && mouseOver){
			GUI.backgroundColor = Color.gray;
			me.Repaint();
		}

        GUI.Box(rect, header, formatting);
		GUI.backgroundColor = bgCol;
		
		return FoldoutToggle(rect, evt, mouseOver, display, 1);
	}

	public static bool FoldoutToggle(Rect rect, Event evt, bool mouseOver, bool display, int size){

		float space = 0;
		float offset = 0;
		switch (size){
			case 0: 
				offset = 4f;
				space = -2f;
				break;
			case 1: 
				offset = 2f;
				space = -22f; 
				break;
			case 2: 
				space = -18f; 
				break;
			default: break;
		}

		Rect arrowRect = new Rect(rect.x+offset, rect.y+offset, 0f, 0f);
		switch(evt.type){

			case EventType.Repaint:
				EditorStyles.foldout.Draw(arrowRect, false, false, display, false);
				break;

			case EventType.MouseUp:
				if (mouseOver){
					display = !display;
					foldoutClicked = false;
					evt.Use();
				}
				break;

			case EventType.DragUpdated:
				if (mouseOver && !display){
					display = true;
					evt.Use();
				}
				break;

			default: break;
		}
		GUILayout.Space(space);
		return display;
	}

	public static bool DoSmallFoldout(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, string header){
		foldouts[mat].SetState(header, SmallFoldout(header, foldouts[mat].GetState(header)));
		return foldouts[mat].GetState(header);
	}

    public static bool SmallFoldout(string header, bool display){
        float lw = EditorGUIUtility.labelWidth-13;
        GUILayoutOption clickArea = GUILayout.MaxWidth(lw);
        Rect rect = GUILayoutUtility.GetRect(0, 18f, clickArea);
        GUILayout.Space(-20);
        header = "    " + header;
        EditorGUILayout.LabelField(header);
		GUILayout.Space(20);
        return DoSmallToggle(display, rect);
    }

    public static bool DoSmallToggle(bool display, Rect rect){
        Event evt = Event.current;
        Rect arrowRect = new Rect(rect.x+2f, rect.y, 0f, 0f);
        if (evt.rawType == EventType.Repaint)
            EditorStyles.foldout.Draw(arrowRect, false, false, display, false);
        if (evt.rawType == EventType.MouseDown && rect.Contains(evt.mousePosition)){
            display = !display;
            evt.Use();
        }
        GUILayout.Space(-18);
        return display;
    }
}