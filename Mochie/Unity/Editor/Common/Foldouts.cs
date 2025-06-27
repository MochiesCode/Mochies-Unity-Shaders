using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Mochie {
    public class Foldouts {

        public enum Style {
            Standard,
            StandardToggle,
            StandardButton,
            Thin,
            ThinToggle,
            ThinShort,
            ThinShortToggle,
            ThinLong,
            ThinLongToggle
        };

        public static bool DoFoldout(Dictionary<Material, Toggles> foldouts, Material mat, string header, Style style){
            bool state = false;
            switch(style){
                case Style.Standard: state = Foldout(header, foldouts[mat].GetState(header)); break;
                case Style.Thin: state = FoldoutThin(header, foldouts[mat].GetState(header)); break;
                case Style.ThinShort: state = FoldoutThinShort(header, foldouts[mat].GetState(header)); break;
                case Style.ThinLong: state = FoldoutThinLong(header, foldouts[mat].GetState(header)); break;
                default: MGUI.DisplayError("INVALID STYLE SELECTION"); break;
            }

            foldouts[mat].SetState(header, state);
            return foldouts[mat].GetState(header);
        }

        public static bool DoFoldout(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, MaterialProperty toggleProp, string header, Style style){
            bool state = false;
            switch(style){
                case Style.StandardToggle: state = FoldoutToggle(header, foldouts[mat].GetState(header), me, toggleProp); break;
                case Style.ThinToggle: state = FoldoutThinToggle(header, foldouts[mat].GetState(header), me, toggleProp); break;
                case Style.ThinShortToggle: state = FoldoutThinShortToggle(header, foldouts[mat].GetState(header), me, toggleProp); break;
                case Style.ThinLongToggle: state = FoldoutThinLongToggle(header, foldouts[mat].GetState(header), me, toggleProp); break;
                default: MGUI.DisplayError("INVALID STYLE SELECTION"); break;
            }

            foldouts[mat].SetState(header, state);
            return foldouts[mat].GetState(header);
        }

        public static bool DoFoldout(Dictionary<Material, Toggles> foldouts, Material mat, string header, int buttonCount, Style style){
            bool state = false;
            switch(style){
                case Style.StandardButton: state = FoldoutButton(header, foldouts[mat].GetState(header), buttonCount); break;
                default: MGUI.DisplayError("INVALID STYLE SELECTION"); break;
            }

            foldouts[mat].SetState(header, state);
            return foldouts[mat].GetState(header);
        }

        public static bool DoFoldoutButton(GUIContent content, float offset){
            float buttonSize = 25f;
            GUILayout.Space(-buttonSize);
            Rect buttonRect = EditorGUILayout.GetControlRect();
            buttonRect.x = MGUI.GetInspectorWidth()+offset;
            buttonRect.width = buttonSize;
            buttonRect.height = buttonSize;
            bool button = GUI.Button(buttonRect, content);
            GUILayout.Space(3);
            return button;
        }

        public static void DrawHeaderRect(Rect headerRect){
            if (Event.current.type == EventType.Repaint){
                EditorGUI.DrawRect(headerRect, new Color(1,1,1,0.16f));
            }
        }

        public static void DrawHeaderRectThin(Rect headerRect){
            if (Event.current.type == EventType.Repaint){
                EditorGUI.DrawRect(headerRect, new Color(1,1,1,0.16f));
            }
        }

        public static Rect GetHeaderRect(){
            Rect headerRect = EditorGUILayout.GetControlRect();
            headerRect.x -= 10f;
            headerRect.width += 12f;
            headerRect.height += 5f;
            return headerRect;
        }

        public static Rect GetHeaderRectButton(float shortenAmount){
            Rect headerRect = EditorGUILayout.GetControlRect();
            headerRect.x -= 10f;
            headerRect.width -= shortenAmount;
            headerRect.height += 5f;
            return headerRect;
        }

        public static Rect GetHeaderRectThin(){
            Rect headerRect = EditorGUILayout.GetControlRect();
            headerRect.height += 1f;
            headerRect.x -= 6f;
            headerRect.width += 6f;
            return headerRect;
        }

        public static Rect GetHeaderRectThinWide(){
            Rect headerRect = EditorGUILayout.GetControlRect();
            headerRect.height += 1f;
            headerRect.x -= 10f;
            headerRect.width += 12f;
            return headerRect;
        }

        public static Rect GetHeaderRectThinShort(){
            Rect headerRect = EditorGUILayout.GetControlRect();
            headerRect.height += 1f;
            return headerRect;
        }

        public static bool ToggleFoldout(bool display, Rect rect, int space){
            Event evt = Event.current;
            // Arrow feels like visual noise, it's pretty obvious they're tabs that can fold out, don't need an indicator for that.
            // Rect arrowRect = new Rect(rect.x-2f, rect.y+10f, 0f, 0f);
            // if (evt.rawType == EventType.Repaint)
            //     EditorStyles.foldout.Draw(arrowRect, false, false, display, false);
            if (evt.rawType == EventType.MouseDown && rect.Contains(evt.mousePosition)){
                display = !display;
                evt.Use();
            }
            GUILayout.Space(space);
            return display;
        }

        public static bool Foldout(string header, bool display){
            Rect headerRect = GetHeaderRect();
            DrawHeaderRect(headerRect);
            GUILayout.Space(-18);
            EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
            GUILayout.Space(25);
            return ToggleFoldout(display, headerRect, -23);
        }

        public static bool FoldoutButton(string header, bool display, int buttonCount){
            Rect headerRect;
            switch (buttonCount){
                case 1: headerRect = GetHeaderRectButton(12f); break;
                case 2: headerRect = GetHeaderRectButton(36f); break;
                default: headerRect = GetHeaderRect(); break;
            }
            DrawHeaderRect(headerRect);
            GUILayout.Space(-18);
            EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
            GUILayout.Space(25);
            return ToggleFoldout(display, headerRect, -23);
        }

        public static bool FoldoutToggle(string header, bool display, MaterialEditor me, MaterialProperty toggleProp){
            Rect headerRect = GetHeaderRect();
            DrawHeaderRect(headerRect);
            GUILayout.Space(-18);
            EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
            GUILayout.Space(-19);
            me.ShaderProperty(toggleProp, " ");
            GUILayout.Space(24);
            return ToggleFoldout(display, headerRect, -23);
        }

        public static bool FoldoutThin(string header, bool display){
            Rect headerRect = GetHeaderRectThin();
            DrawHeaderRectThin(headerRect);
            GUILayout.Space(-20);
            EditorGUILayout.LabelField(header);
            GUILayout.Space(24);
            return ToggleFoldout(display, headerRect, -24);
        }

        public static bool FoldoutThinToggle(string header, bool display, MaterialEditor me, MaterialProperty toggleProp){
            Rect headerRect = GetHeaderRectThin();
            DrawHeaderRectThin(headerRect);
            GUILayout.Space(-20);
            EditorGUILayout.LabelField(header);
            GUILayout.Space(-20);
            me.ShaderProperty(toggleProp, " ");
            GUILayout.Space(24);
            return ToggleFoldout(display, headerRect, -24);
        }

        public static bool FoldoutThinShort(string header, bool display){
            Rect headerRect = GetHeaderRectThinShort();
            DrawHeaderRectThin(headerRect);
            GUILayout.Space(-20);
            header = "  " + header;
            EditorGUILayout.LabelField(header);
            GUILayout.Space(24);
            return ToggleFoldout(display, headerRect, -24);
        }

        public static bool FoldoutThinShortToggle(string header, bool display, MaterialEditor me, MaterialProperty toggleProp){
            Rect headerRect = GetHeaderRectThinShort();
            DrawHeaderRectThin(headerRect);
            GUILayout.Space(-20);
            header = "  " + header;
            EditorGUILayout.LabelField(header);
            GUILayout.Space(-20);
            me.ShaderProperty(toggleProp, " ");
            GUILayout.Space(24);
            return ToggleFoldout(display, headerRect, -24);
        }

        public static bool FoldoutThinLong(string header, bool display){
            Rect headerRect = GetHeaderRectThinWide();
            DrawHeaderRectThin(headerRect);
            GUILayout.Space(-20);
            EditorGUILayout.LabelField(header);
            GUILayout.Space(24);
            return ToggleFoldout(display, headerRect, -24);
        }

        public static bool FoldoutThinLongToggle(string header, bool display, MaterialEditor me, MaterialProperty toggleProp){
            Rect headerRect = GetHeaderRectThinWide();
            DrawHeaderRectThin(headerRect);
            GUILayout.Space(-20);
            EditorGUILayout.LabelField(header);
            GUILayout.Space(-20);
            // toggleProp.floatValue = GUI.Toggle(headerRect, toggleProp.floatValue == 1, "") ? 1 : 0;
            // GUILayout.Space(44);
            me.ShaderProperty(toggleProp, " ");
            GUILayout.Space(24);
            return ToggleFoldout(display, headerRect, -24);
        }

        // Legacy stuff, only used for one thing in the Uber shader, still handy to have around even if the code is ugly
        public static bool DoSmallFoldout(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, string header){
            foldouts[mat].SetState(header, SmallFoldout(header, foldouts[mat].GetState(header)));
            return foldouts[mat].GetState(header);
        }

        public static bool DoSmallFoldoutBold(Dictionary<Material, Toggles> foldouts, Material mat, MaterialEditor me, string header){
            foldouts[mat].SetState(header, SmallFoldoutBold(header, foldouts[mat].GetState(header)));
            return foldouts[mat].GetState(header);
        }

        public static bool SmallFoldout(string header, bool display){
            MGUI.Space4();
            float lw = EditorGUIUtility.labelWidth-13;
            GUILayoutOption clickArea = GUILayout.MaxWidth(lw);
            Rect rect = GUILayoutUtility.GetRect(0, 18f, clickArea);
            MGUI.SpaceN24();
            header = "    " + header;
            EditorGUILayout.LabelField(header);
            MGUI.Space22();
            return DoSmallToggle(display, rect);
        }

        public static bool SmallFoldoutBold(string header, bool display){
            float lw = EditorGUIUtility.labelWidth-13;
            GUILayoutOption clickArea = GUILayout.MaxWidth(lw);
            Rect rect = GUILayoutUtility.GetRect(0, 18f, clickArea);
            MGUI.SpaceN24();
            header = "    " + header;
            EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
            MGUI.Space22();
            return DoSmallToggle(display, rect);
        }

        public static bool DoSmallToggle(bool display, Rect rect){
            Event evt = Event.current;
            Rect arrowRect = new Rect(rect.x-1f, rect.y+5f, 0f, 0f);
            if (evt.rawType == EventType.Repaint)
                EditorStyles.foldout.Draw(arrowRect, false, false, display, false);
            if (evt.rawType == EventType.MouseDown && rect.Contains(evt.mousePosition)){
                display = !display;
                evt.Use();
            }
            MGUI.SpaceN20();
            return display;
        }
        
    }
}