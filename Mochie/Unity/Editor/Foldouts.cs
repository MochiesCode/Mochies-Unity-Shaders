using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Mochie {
	public class Foldouts {

		#if UNITY_2019_1_OR_NEWER
			static float versionOffset = 7f;
		#else
			static float versionOffset = 0f;
		#endif

		private static bool foldoutClicked = false;
		private static Color errorCol = new Color(1f,0.3f,0.3f,1f);
		private static float[] foldoutOffsets = {-8f, 20f, 48f, 76f};
		private static float[] medFoldoutOffsets = {-4f, 21f, 45f};

		public static bool ContainsDigit(string input){
			return input.Any(c => char.IsDigit(c));
		}

		public static bool DoFoldout(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, int buttonCount, string header){
			foldouts[mat].SetState(header, Foldout(header, foldouts[mat].GetState(header), buttonCount, me));
			return foldouts[mat].GetState(header);
		}

		public static bool DoFoldoutError(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, bool[] errorConds, int buttonCount, string header){
			foldouts[mat].SetState(header, FoldoutError(header, foldouts[mat].GetState(header), me, mat, errorConds, buttonCount));
			return foldouts[mat].GetState(header);
		}

		public static bool DoMediumFoldout(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, MaterialProperty prop, int buttonCount, string header){
			foldouts[mat].SetState(header, MediumFoldout(header, foldouts[mat].GetState(header), buttonCount, me));
			FoldoutProperty(me, prop);
			MGUI.Space6();
			return foldouts[mat].GetState(header);
		}

		public static bool DoMediumFoldout(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, int buttonCount, string header){
			foldouts[mat].SetState(header, MediumFoldout(header, foldouts[mat].GetState(header), buttonCount, me));
			GUILayout.Space(24);
			return foldouts[mat].GetState(header);
		}

		public static bool DoMediumFoldoutError(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, MaterialProperty toggleProp, bool errorCond, int buttonCount, string header){
			foldouts[mat].SetState(header, MediumFoldoutError(header, foldouts[mat].GetState(header), errorCond, buttonCount, me, mat));
			FoldoutProperty(me, toggleProp);
			MGUI.Space6();
			return foldouts[mat].GetState(header);
		}

		public static bool DoMediumFoldoutError(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, bool errorCond, int buttonCount, string header){
			foldouts[mat].SetState(header, MediumFoldoutError(header, foldouts[mat].GetState(header), errorCond, buttonCount, me, mat));
			GUILayout.Space(24);
			return foldouts[mat].GetState(header);
		}

		public static void FoldoutProperty(MaterialEditor me, MaterialProperty prop){
			float lw = EditorGUIUtility.labelWidth;
			GUILayoutOption clickArea = GUILayout.MaxWidth(lw+15f);
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;
			var tog = EditorGUILayout.Toggle(" ", prop.floatValue==1, clickArea) ? 1 : 0;
			if (EditorGUI.EndChangeCheck())
				prop.floatValue = tog;
			EditorGUI.showMixedValue = false;
		}

		public static bool Foldout(string header, bool display, int buttonCount, MaterialEditor me){
			GUIStyle formatting = new GUIStyle("ShurikenModuleTitle");
			formatting.font = new GUIStyle(EditorStyles.boldLabel).font;
			formatting.contentOffset = new Vector2(20f, -3f);
			formatting.hover.textColor = Color.gray;
			formatting.fixedHeight = 28f;
			formatting.fontSize = 10;

			Rect rect = GUILayoutUtility.GetRect(MGUI.GetInspectorWidth(), formatting.fixedHeight, formatting);
			rect.width -= foldoutOffsets[buttonCount];
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

			if (ContainsDigit(header))
				header = header.Substring(0, header.Length-2);

			GUI.Box(rect, header, formatting);
			GUI.backgroundColor = bgCol;

			return FoldoutToggle(rect, evt, mouseOver, display, 0);
		}

		public static bool MediumFoldout(string header, bool display, int buttonCount, MaterialEditor me){
			GUIStyle formatting = new GUIStyle("ShurikenModuleTitle");
			float lw = EditorGUIUtility.labelWidth;
			formatting.contentOffset = new Vector2(20f, -2f);
			formatting.fixedHeight = 22f;
			formatting.fixedWidth = MGUI.GetInspectorWidth() - medFoldoutOffsets[buttonCount];
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

			if (ContainsDigit(header))
				header = header.Substring(0, header.Length-2);

			GUI.Box(rect, header, formatting);
			GUI.backgroundColor = bgCol;
			
			return FoldoutToggle(rect, evt, mouseOver, display, 1);
		}

		public static bool FoldoutError(string header, bool display, MaterialEditor me, Material mat, bool[] errorConds, int buttonCount){
			GUIStyle formatting = new GUIStyle("ShurikenModuleTitle");
			formatting.font = new GUIStyle(EditorStyles.boldLabel).font;
			formatting.contentOffset = new Vector2(20f, -3f);
			formatting.hover.textColor = Color.gray;
			formatting.fixedHeight = 28f;
			formatting.fontSize = 10;

			Rect rect = GUILayoutUtility.GetRect(MGUI.GetInspectorWidth(), formatting.fixedHeight, formatting);
			rect.width -= foldoutOffsets[buttonCount];
			rect.x -= 8f;

			Event evt = Event.current;
			Color bgCol = GUI.backgroundColor;
			
			foreach (bool b in errorConds){
				if (b){
					GUI.backgroundColor = errorCol;
					break;
				}
			}
			bool mouseOver = rect.Contains(evt.mousePosition);
			if (evt.type == EventType.MouseDown && mouseOver){
				foldoutClicked = true;
				evt.Use();
			}
				
			else if (evt.type == EventType.Repaint && foldoutClicked && mouseOver){
				GUI.backgroundColor = Color.gray;
				me.Repaint();
			}

			if (ContainsDigit(header))
				header = header.Substring(0, header.Length-2);

			GUI.Box(rect, header, formatting);
			GUI.backgroundColor = bgCol;

			return FoldoutToggle(rect, evt, mouseOver, display, 0);
		}

		public static bool MediumFoldoutError(string header, bool display, bool errorCond, int buttonCount, MaterialEditor me, Material mat){
			GUIStyle formatting = new GUIStyle("ShurikenModuleTitle");
			float lw = EditorGUIUtility.labelWidth;
			formatting.contentOffset = new Vector2(20f, -2f);
			formatting.fixedHeight = 22f;
			formatting.fixedWidth = MGUI.GetInspectorWidth()-medFoldoutOffsets[buttonCount];
			formatting.fontSize = 10;

			Rect rect = GUILayoutUtility.GetRect(0f, 20f, formatting);
			rect.x -= 4f;
			rect.width = lw;

			Event evt = Event.current;
			Color bgCol = GUI.backgroundColor;

			if (errorCond)
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
			
			if (ContainsDigit(header))
				header = header.Substring(0, header.Length-2);
				
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

			Rect arrowRect = new Rect(rect.x+offset, rect.y+offset+versionOffset, 0f, 0f);
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

		public static bool DoSmallFoldoutBold(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, string header){
			foldouts[mat].SetState(header, SmallFoldoutBold(header, foldouts[mat].GetState(header)));
			return foldouts[mat].GetState(header);
		}
		
		public static bool SmallFoldoutBold(string header, bool display){
			float lw = EditorGUIUtility.labelWidth-13;
			GUILayoutOption clickArea = GUILayout.MaxWidth(lw);
			Rect rect = GUILayoutUtility.GetRect(0, 18f, clickArea);
			GUILayout.Space(-20);
			header = "    " + header;
			EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
			GUILayout.Space(20);
			return DoSmallToggle(display, rect);
		}

		public static bool DoSmallToggle(bool display, Rect rect){
			Event evt = Event.current;
			Rect arrowRect = new Rect(rect.x+2f, rect.y+versionOffset, 0f, 0f);
			if (evt.rawType == EventType.Repaint)
				EditorStyles.foldout.Draw(arrowRect, false, false, display, false);
			if (evt.rawType == EventType.MouseDown && rect.Contains(evt.mousePosition)){
				display = !display;
				evt.Use();
			}
			GUILayout.Space(-18);
			return display;
		}

		public static bool DoMaskFoldout(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, GUIContent dispHeader, string header){
			foldouts[mat].SetState(header, MaskFoldout(dispHeader.text, foldouts[mat].GetState(header)));
			return foldouts[mat].GetState(header);
		}

		public static bool MaskFoldout(string header, bool display){
			float lw = EditorGUIUtility.labelWidth-13;
			GUILayoutOption clickArea = GUILayout.MaxWidth(lw);
			Rect rect = GUILayoutUtility.GetRect(0, 18f, clickArea);
			GUILayout.Space(-20);
			header = "    " + header;
			EditorGUILayout.LabelField(header);
			GUILayout.Space(20);
			return DoMaskToggle(display, rect);
		}

		public static bool DoMaskToggle(bool display, Rect rect){
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
}