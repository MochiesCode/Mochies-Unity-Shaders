// A collection of UI functions I've developed over the years to improve customization of editor scripts
// Full of lots of garbage duplicate stuff I'm too lazy to clean out
// By Mochie#8794

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace Mochie {
    public static class MGUI {
        
        public static Texture2D resetIcon = (Texture2D)Resources.Load("ResetIcon", typeof(Texture2D));
        public static Texture2D collapseIcon = (Texture2D)Resources.Load("CollapseIcon", typeof(Texture2D));
        public static Texture2D mochieLogo = (Texture2D)Resources.Load("MochieLogo", typeof(Texture2D));
        public static Texture2D mochieLogoSquare = (Texture2D)Resources.Load("MochieLogoSquare", typeof(Texture2D));
        public static Texture2D mochieLogoPro = (Texture2D)Resources.Load("MochieLogo_Pro", typeof(Texture2D));
        public static Texture2D patIconTex = (Texture2D)Resources.Load("Patreon_Icon", typeof(Texture2D));
        public static Texture2D cheeseIconTex = (Texture2D)Resources.Load("Cheese_Icon", typeof(Texture2D));
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

        public static string GetCurrentProjectFolderPath(){
            Type projectWindowUtilType = typeof(ProjectWindowUtil);
            MethodInfo getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
            object obj = getActiveFolderPath.Invoke(null, new object[0]);
            return obj.ToString();
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
        
        public static bool IsNewLiteVersion(Material mat){
            return mat.shader.name.Contains("Lite");
        }

        public static bool IsMobileVersion(Material mat){
            return mat.shader.name.Contains("Mobile");
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
        
        public static void DoHeader(string header){
            Rect headerRect = EditorGUILayout.GetControlRect();
            headerRect.x -= 10f;
            GUIStyle formatting = new GUIStyle();
            formatting.fontSize = 24;
            formatting.font = (Font)Resources.Load("Header_Font", typeof(Font));
            if (EditorGUIUtility.isProSkin){
                Color proCol = new Color(0.8f, 0.8f, 0.8f, 1);
                formatting.normal.textColor = proCol;
                formatting.hover.textColor = proCol;
            }
            GUI.Label(headerRect, header, formatting);
            GUILayout.Space(16);
        }

        public static void DoFooter(string versionLabel){
            GUILayout.Space(40);
            float buttonSize = 35f;
            float centerOffset = MGUI.GetInspectorWidth()/2f;
            Rect buttonRect = EditorGUILayout.GetControlRect();
            buttonRect.x += centerOffset-buttonSize*3f*0.5f;
            buttonRect.width = buttonSize;
            buttonRect.height = buttonSize;
            if (GUI.Button(buttonRect, MGUI.patIconTex))
                Application.OpenURL("https://www.patreon.com/mochieshaders");
            buttonRect.x += buttonSize;
            GUI.Label(buttonRect, MGUI.mochieLogoSquare);
            buttonRect.y -= 20f;
            GUIStyle formatting = new GUIStyle();
            formatting.fontSize = 15;
            formatting.fontStyle = FontStyle.Bold;
            if (EditorGUIUtility.isProSkin){
                Color proCol = new Color(0.8f, 0.8f, 0.8f, 1);
                formatting.normal.textColor = proCol;
                formatting.hover.textColor = proCol;
            }
            GUI.Label(buttonRect, versionLabel, formatting);
            buttonRect.y += 20f;
            buttonRect.x += buttonSize;
            if (GUI.Button(buttonRect, MGUI.cheeseIconTex)){
                string patrons = "Thank you for your support, past and present!\n";
                foreach (string s in cheeseList){
                    patrons += "\n" + s;
                }
                patrons += "\n\nPlease note that names are not removed from this list, so it does not reflect any individual user's current patreon subscription status.";
                EditorUtility.DisplayDialog("Skyrim Cheese Wheel Hall of Fame", patrons, "Close");
            }
            GUILayout.Space(90);
        }

        public static void DisplayError(string message){
            EditorGUILayout.HelpBox(message, MessageType.Error);
            Space2();
        }

        public static void DisplayWarning(string message){
            EditorGUILayout.HelpBox(message, MessageType.Warning);
            Space2();
        }

        public static void DisplayInfo(string message){
            EditorGUILayout.HelpBox(message, MessageType.Info);
            Space2();
        }
        
        public static void DisplayText(string message){
            EditorGUILayout.HelpBox(message, MessageType.None);
            Space2();
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

        public static bool SimpleButton(GUIContent content, float width, float xPos){
            Rect buttonRect = EditorGUILayout.GetControlRect();
            buttonRect.width = width;
            buttonRect.x += xPos;
            return GUI.Button(buttonRect, content);
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

        public static void ResetProperty(MaterialProperty prop){
            if (prop == null || prop.targets == null || prop.targets.Length == 0) return;
            Undo.RecordObjects(prop.targets, "Reset " + prop.displayName);
            foreach (var target in prop.targets){
                Material mat = target as Material;
                if (mat == null || mat.shader == null) continue;
                int propIndex = mat.shader.FindPropertyIndex(prop.name);
                if (propIndex < 0) continue;
                switch (prop.type){
                    case MaterialProperty.PropType.Float:
                    case MaterialProperty.PropType.Range:
                        prop.floatValue = mat.shader.GetPropertyDefaultFloatValue(propIndex);
                        break;
                    case MaterialProperty.PropType.Vector:
                        prop.vectorValue = mat.shader.GetPropertyDefaultVectorValue(propIndex);
                        break;
                    case MaterialProperty.PropType.Color:
                        prop.colorValue = mat.shader.GetPropertyDefaultVectorValue(propIndex);
                        break;
                    case MaterialProperty.PropType.Int:
                        prop.intValue = mat.shader.GetPropertyDefaultIntValue(propIndex);
                        break;
                    case MaterialProperty.PropType.Texture:
                        ShaderImporter shaderImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(mat.shader)) as ShaderImporter;
                        prop.textureValue = shaderImporter != null ? shaderImporter.GetDefaultTexture(prop.name) : null;
                        prop.textureScaleAndOffset = new Vector4(1f, 1f, 0f, 0f);
                        break;
                }
            }
        }

        public static void CopyProperty(MaterialProperty prop){
            if (prop == null) return;
            switch (prop.type){
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    EditorGUIUtility.systemCopyBuffer = prop.floatValue.ToString(CultureInfo.InvariantCulture);
                    break;
                case MaterialProperty.PropType.Int:
                    EditorGUIUtility.systemCopyBuffer = prop.intValue.ToString();
                    break;
                case MaterialProperty.PropType.Vector:
                    Vector4 v = prop.vectorValue;
                    EditorGUIUtility.systemCopyBuffer = string.Format(CultureInfo.InvariantCulture, "Vector4({0:g9},{1:g9},{2:g9},{3:g9})", v.x, v.y, v.z, v.w);
                    break;
                case MaterialProperty.PropType.Color:
                    Color c = prop.colorValue;
                    EditorGUIUtility.systemCopyBuffer = string.Format(CultureInfo.InvariantCulture, "Color({0:g9},{1:g9},{2:g9},{3:g9})", c.r, c.g, c.b, c.a);
                    break;
                case MaterialProperty.PropType.Texture:
                    if (prop.textureValue != null)
                        EditorGUIUtility.systemCopyBuffer = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prop.textureValue));
                    break;
            }
        }

        public static void PasteProperty(MaterialProperty prop){
            if (prop == null || prop.targets == null || prop.targets.Length == 0) return;
            string clip = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(clip)) return;
            Undo.RecordObjects(prop.targets, "Paste " + prop.displayName);
            switch (prop.type){
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    if (float.TryParse(clip, NumberStyles.Float, CultureInfo.InvariantCulture, out float fVal))
                        prop.floatValue = fVal;
                    break;
                case MaterialProperty.PropType.Int:
                    if (int.TryParse(clip, out int iVal))
                        prop.intValue = iVal;
                    else if (float.TryParse(clip, NumberStyles.Float, CultureInfo.InvariantCulture, out float fiVal))
                        prop.intValue = (int)fiVal;
                    break;
                case MaterialProperty.PropType.Vector:
                    if (TryParseVector4(clip, out Vector4 vVal))
                        prop.vectorValue = vVal;
                    break;
            }
        }

        public static bool CanPasteProperty(MaterialProperty prop){
            if (prop == null) return false;
            string clip = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(clip)) return false;
            switch (prop.type){
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    return float.TryParse(clip, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
                case MaterialProperty.PropType.Int:
                    return int.TryParse(clip, out _) || float.TryParse(clip, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
                case MaterialProperty.PropType.Vector:
                    return TryParseVector4(clip, out _);
                default:
                    return false;
            }
        }

        private static bool TryParseVector4(string text, out Vector4 result){
            result = Vector4.zero;
            if (string.IsNullOrEmpty(text)) return false;
            text = text.Trim();
            if (text.StartsWith("Vector4(", StringComparison.OrdinalIgnoreCase) && text.EndsWith(")"))
                text = text.Substring(8, text.Length - 9);
            string[] parts = text.Split(',');
            if (parts.Length == 4){
                if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                    float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z) &&
                    float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float w)){
                    result = new Vector4(x, y, z, w);
                    return true;
                }
            } else if (parts.Length == 3){
                if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                    float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z)){
                    result = new Vector4(x, y, z, 0f);
                    return true;
                }
            }
            return false;
        }

        public static void DoCompoundContextMenu(Rect totalRect, MaterialProperty copyProp, params MaterialProperty[] allProps){
            Event e = Event.current;
            Rect hitRect = new Rect(0f, totalRect.y, totalRect.xMax, totalRect.height);
            if (e.type != EventType.ContextClick || !hitRect.Contains(e.mousePosition))
                return;

            e.Use();
            GenericMenu menu = new GenericMenu();

            // Material Variant override handling
            bool isAnyOverridden = false;
            Material firstMat = null;
            foreach (var p in allProps){
                if (p != null && p.targets != null){
                    foreach (var t in p.targets){
                        Material m = t as Material;
                        if (m != null){
                            if (firstMat == null) firstMat = m;
                            if (m.isVariant && m.IsPropertyOverriden(p.name)){
                                isAnyOverridden = true;
                                break;
                            }
                        }
                    }
                }
                if (isAnyOverridden) break;
            }

            if (isAnyOverridden && firstMat != null){
                if (firstMat.parent != null){
                    menu.AddItem(new GUIContent("Apply to Material '" + firstMat.parent.name + "'"), false, () => {
                        foreach (var p in allProps){
                            if (p != null && p.targets != null){
                                foreach (var t in p.targets){
                                    Material m = t as Material;
                                    if (m != null && m.parent != null)
                                        m.ApplyPropertyOverride(m.parent, p.name, true);
                                }
                            }
                        }
                    });
                }
                menu.AddItem(new GUIContent("Revert"), false, () => {
                    foreach (var p in allProps){
                        if (p != null && p.targets != null){
                            foreach (var t in p.targets){
                                Material m = t as Material;
                                if (m != null)
                                    m.RevertPropertyOverride(p.name);
                            }
                        }
                    }
                });
                menu.AddSeparator("");
            }

            // Copy
            if (copyProp != null)
                menu.AddItem(new GUIContent("Copy"), false, () => CopyProperty(copyProp));
            else
                menu.AddDisabledItem(new GUIContent("Copy"));

            // Paste
            if (copyProp != null && CanPasteProperty(copyProp))
                menu.AddItem(new GUIContent("Paste"), false, () => PasteProperty(copyProp));
            else
                menu.AddDisabledItem(new GUIContent("Paste"));

            menu.AddSeparator("");

            // Reset (Resets ALL properties in the function)
            menu.AddItem(new GUIContent("Reset"), false, () => {
                foreach (var p in allProps){
                    if (p != null)
                        ResetProperty(p);
                }
            });

            menu.AddSeparator("");

            // Lock in children (Locks/unlocks ALL properties in the function)
            bool isAnyLocked = false;
            foreach (var p in allProps){
                if (p != null && p.targets != null){
                    foreach (var t in p.targets){
                        Material m = t as Material;
                        if (m != null && m.IsPropertyLocked(p.name)){
                            isAnyLocked = true;
                            break;
                        }
                    }
                }
                if (isAnyLocked) break;
            }

            bool newLock = !isAnyLocked;
            menu.AddItem(new GUIContent("Lock in children"), isAnyLocked, () => {
                foreach (var p in allProps){
                    if (p != null && p.targets != null){
                        Undo.RecordObjects(p.targets, (newLock ? "Lock " : "Unlock ") + p.displayName);
                        foreach (var t in p.targets){
                            Material m = t as Material;
                            if (m != null)
                                m.SetPropertyLock(p.name, newLock);
                        }
                    }
                }
            });

            menu.ShowAsContext();
        }

        // Regular shader property but the text is bold
        public static void ShaderPropertyBold(MaterialEditor me, MaterialProperty prop, string text){
            ShaderPropertyBold(me, prop, new GUIContent(text));
        }

        public static void ShaderPropertyBold(MaterialEditor me, MaterialProperty prop, GUIContent text){
            string textStr = text != null ? text.text : "";
            float height = (prop != null && me != null) ? me.GetPropertyHeight(prop, textStr) : EditorGUIUtility.singleLineHeight;
            Rect r = EditorGUILayout.GetControlRect(true, height);
            if (prop != null) MaterialEditor.BeginProperty(r, prop);
            if (me != null) me.ShaderProperty(r, prop, " ");
            Rect labelRect = new Rect(r.x, r.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, text, EditorStyles.boldLabel);
            if (prop != null) MaterialEditor.EndProperty();
        }

        // Slider with a toggle
        public static void ToggleSlider(MaterialEditor me, string label, MaterialProperty toggle, MaterialProperty slider){
            ToggleSlider(me, new GUIContent(label), toggle, slider);
        }

        public static void ToggleSlider(MaterialEditor me, GUIContent label, MaterialProperty toggle, MaterialProperty slider){
            float lw = EditorGUIUtility.labelWidth;
            float indent = lw + 25f;
            Rect totalRect = EditorGUILayout.GetControlRect();
            Rect toggleRect = new Rect(totalRect.x, totalRect.y, indent, totalRect.height);
            Rect sliderRect = new Rect(totalRect.x + indent, totalRect.y, totalRect.width - indent, totalRect.height);

            bool isCheckbox = Event.current.mousePosition.x > (totalRect.x + lw) && Event.current.mousePosition.x <= (totalRect.x + indent);
            MaterialProperty copyProp = isCheckbox ? toggle : slider;
            DoCompoundContextMenu(totalRect, copyProp, toggle, slider);

            Rect propRect = (Event.current.type == EventType.ContextClick || Event.current.rawType == EventType.ContextClick) ? Rect.zero : totalRect;
            if (toggle != null) MaterialEditor.BeginProperty(propRect, toggle);
            if (slider != null) MaterialEditor.BeginProperty(propRect, slider);

            EditorGUI.BeginChangeCheck();
            var tog = EditorGUI.Toggle(toggleRect, label, (toggle != null && toggle.floatValue == 1)) ? 1f : 0f;
            if (EditorGUI.EndChangeCheck() && toggle != null)
                toggle.floatValue = tog;

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(toggle != null && toggle.floatValue == 0);
            var slide = slider != null ? EditorGUI.Slider(sliderRect, slider.floatValue, slider.rangeLimits.x, slider.rangeLimits.y) : 0f;
            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck() && slider != null)
                slider.floatValue = slide;

            if (slider != null) MaterialEditor.EndProperty();
            if (toggle != null) MaterialEditor.EndProperty();
        }

        public static void ToggleIntSlider(MaterialEditor me, string label, MaterialProperty toggle, MaterialProperty slider){
            ToggleIntSlider(me, new GUIContent(label), toggle, slider);
        }

        public static void ToggleIntSlider(MaterialEditor me, GUIContent label, MaterialProperty toggle, MaterialProperty slider){
            float lw = EditorGUIUtility.labelWidth;
            float indent = lw + 25f;
            Rect totalRect = EditorGUILayout.GetControlRect();
            Rect toggleRect = new Rect(totalRect.x, totalRect.y, indent, totalRect.height);
            Rect sliderRect = new Rect(totalRect.x + indent, totalRect.y, totalRect.width - indent, totalRect.height);

            bool isCheckbox = Event.current.mousePosition.x > (totalRect.x + lw) && Event.current.mousePosition.x <= (totalRect.x + indent);
            MaterialProperty copyProp = isCheckbox ? toggle : slider;
            DoCompoundContextMenu(totalRect, copyProp, toggle, slider);

            Rect propRect = (Event.current.type == EventType.ContextClick || Event.current.rawType == EventType.ContextClick) ? Rect.zero : totalRect;
            if (toggle != null) MaterialEditor.BeginProperty(propRect, toggle);
            if (slider != null) MaterialEditor.BeginProperty(propRect, slider);

            EditorGUI.BeginChangeCheck();
            var tog = EditorGUI.Toggle(toggleRect, label, (toggle != null && toggle.floatValue == 1)) ? 1f : 0f;
            if (EditorGUI.EndChangeCheck() && toggle != null)
                toggle.floatValue = tog;

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(toggle != null && toggle.floatValue == 0);
            var slide = slider != null ? (int)EditorGUI.Slider(sliderRect, slider.floatValue, slider.rangeLimits.x, slider.rangeLimits.y) : 0;
            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck() && slider != null)
                slider.floatValue = slide;

            if (slider != null) MaterialEditor.EndProperty();
            if (toggle != null) MaterialEditor.EndProperty();
        }
        
        public static void CustomToggleSlider(string label, MaterialProperty toggle, MaterialProperty value, float min, float max){
            CustomToggleSlider(new GUIContent(label), toggle, value, min, max);
        }

        public static void CustomToggleSlider(GUIContent label, MaterialProperty toggle, MaterialProperty value, float min, float max){
            float lw = EditorGUIUtility.labelWidth;
            float indent = lw + 22f;
            Rect totalRect = EditorGUILayout.GetControlRect();
            Rect toggleRect = new Rect(totalRect.x, totalRect.y, indent, totalRect.height);
            Rect sliderRect = new Rect(totalRect.x + indent, totalRect.y, totalRect.width - indent, totalRect.height);

            bool isCheckbox = Event.current.mousePosition.x > (totalRect.x + lw) && Event.current.mousePosition.x <= (totalRect.x + indent);
            MaterialProperty copyProp = isCheckbox ? toggle : value;
            DoCompoundContextMenu(totalRect, copyProp, toggle, value);

            Rect propRect = (Event.current.type == EventType.ContextClick || Event.current.rawType == EventType.ContextClick) ? Rect.zero : totalRect;
            if (toggle != null) MaterialEditor.BeginProperty(propRect, toggle);
            if (value != null) MaterialEditor.BeginProperty(propRect, value);

            EditorGUI.BeginChangeCheck();
            var tog = EditorGUI.Toggle(toggleRect, label, (toggle != null && toggle.floatValue == 1)) ? 1f : 0f;
            if (EditorGUI.EndChangeCheck() && toggle != null)
                toggle.floatValue = tog;

            EditorGUI.BeginDisabledGroup(toggle != null && toggle.floatValue == 0);
            Rect r0 = new Rect(sliderRect.x, sliderRect.y, sliderRect.width - 55f, sliderRect.height);
            Rect r1 = new Rect(sliderRect.xMax - 50f, sliderRect.y, 50f, sliderRect.height);

            EditorGUI.BeginChangeCheck();
            float val = value != null ? value.floatValue : 0f;
            val = GUI.HorizontalSlider(r0, val, min, max);
            val = EditorGUI.IntField(r1, (int)val);
            if (EditorGUI.EndChangeCheck() && value != null)
                value.floatValue = val;
            EditorGUI.EndDisabledGroup();

            if (value != null) MaterialEditor.EndProperty();
            if (toggle != null) MaterialEditor.EndProperty();
        }

        // Float with a toggle
        public static void ToggleFloat(MaterialEditor me, string label, MaterialProperty toggle, MaterialProperty floatProp){
            ToggleFloat(me, new GUIContent(label), toggle, floatProp);
        }

        public static void ToggleFloat(MaterialEditor me, GUIContent label, MaterialProperty toggle, MaterialProperty floatProp){
            float lw = EditorGUIUtility.labelWidth;
            float indent = lw + 20f;
            Rect totalRect = EditorGUILayout.GetControlRect();
            Rect toggleRect = new Rect(totalRect.x, totalRect.y, indent, totalRect.height);
            Rect floatRect = new Rect(totalRect.x + indent, totalRect.y, totalRect.width - indent, totalRect.height);

            bool isCheckbox = Event.current.mousePosition.x > (totalRect.x + lw) && Event.current.mousePosition.x <= (totalRect.x + indent);
            MaterialProperty copyProp = isCheckbox ? toggle : floatProp;
            DoCompoundContextMenu(totalRect, copyProp, toggle, floatProp);

            Rect propRect = (Event.current.type == EventType.ContextClick || Event.current.rawType == EventType.ContextClick) ? Rect.zero : totalRect;
            if (toggle != null) MaterialEditor.BeginProperty(propRect, toggle);
            if (floatProp != null) MaterialEditor.BeginProperty(propRect, floatProp);

            EditorGUI.BeginChangeCheck();
            var tog = EditorGUI.Toggle(toggleRect, label, (toggle != null && toggle.floatValue == 1)) ? 1f : 0f;
            if (EditorGUI.EndChangeCheck() && toggle != null)
                toggle.floatValue = tog;

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(toggle != null && toggle.floatValue == 0);
            var floatVal = floatProp != null ? EditorGUI.FloatField(floatRect, floatProp.floatValue) : 0f;
            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck() && floatProp != null)
                floatProp.floatValue = floatVal;

            if (floatProp != null) MaterialEditor.EndProperty();
            if (toggle != null) MaterialEditor.EndProperty();
        }

        public static void Vector3FieldToggle(string label, MaterialProperty toggle, MaterialProperty vec){
            Vector3FieldToggle(new GUIContent(label), toggle, vec);
        }

        public static void Vector3FieldToggle(GUIContent label, MaterialProperty toggle, MaterialProperty vec){
            float origLabelWidth = EditorGUIUtility.labelWidth;
            float toggleWidth = origLabelWidth + 18f;

            Rect totalRect = EditorGUILayout.GetControlRect();
            Rect toggleRect = new Rect(totalRect.x, totalRect.y, toggleWidth, totalRect.height);
            Rect vecRect = new Rect(totalRect.x + toggleWidth, totalRect.y, totalRect.width - toggleWidth, totalRect.height);

            bool isCheckbox = Event.current.mousePosition.x > (totalRect.x + origLabelWidth) && Event.current.mousePosition.x <= (totalRect.x + toggleWidth);
            MaterialProperty copyProp = isCheckbox ? toggle : vec;
            DoCompoundContextMenu(totalRect, copyProp, toggle, vec);

            Rect propRect = (Event.current.type == EventType.ContextClick || Event.current.rawType == EventType.ContextClick) ? Rect.zero : totalRect;
            if (toggle != null) MaterialEditor.BeginProperty(propRect, toggle);
            if (vec != null) MaterialEditor.BeginProperty(propRect, vec);

            EditorGUI.BeginChangeCheck();
            var tog = EditorGUI.Toggle(toggleRect, label, (toggle != null && toggle.floatValue == 1)) ? 1f : 0f;
            if (EditorGUI.EndChangeCheck() && toggle != null)
                toggle.floatValue = tog;

            Vector4 newVec = vec != null ? vec.vectorValue : Vector4.zero;
            float fieldWidth = (vecRect.width / 3f) - 2f;
            EditorGUIUtility.labelWidth = 13f;
            EditorGUI.BeginDisabledGroup(toggle != null && toggle.floatValue == 0);

            EditorGUI.BeginChangeCheck();

            // X Field
            Rect fieldRect = new Rect(vecRect.x, vecRect.y, fieldWidth, vecRect.height);
            newVec.x = EditorGUI.FloatField(fieldRect, "X", newVec.x);

            // Y Field
            fieldRect.x += fieldWidth + 2f;
            newVec.y = EditorGUI.FloatField(fieldRect, "Y", newVec.y);

            // Z Field
            fieldRect.x += fieldWidth + 2f;
            newVec.z = EditorGUI.FloatField(fieldRect, "Z", newVec.z);

            if (EditorGUI.EndChangeCheck() && vec != null)
                vec.vectorValue = newVec;

            EditorGUI.EndDisabledGroup();
            EditorGUIUtility.labelWidth = origLabelWidth;

            if (vec != null) MaterialEditor.EndProperty();
            if (toggle != null) MaterialEditor.EndProperty();
            Space1();
        }

        public static void Vector3FieldToggleW(string label, int toggle, MaterialProperty vec){
            Vector3FieldToggleW(new GUIContent(label), toggle, vec);
        }

        public static void Vector3FieldToggleW(GUIContent label, int toggle, MaterialProperty vec){
            float origLabelWidth = EditorGUIUtility.labelWidth;
            float toggleWidth = origLabelWidth + 18f;

            Rect totalRect = EditorGUILayout.GetControlRect();
            if (vec != null) MaterialEditor.BeginProperty(totalRect, vec);

            Rect toggleRect = new Rect(totalRect.x, totalRect.y, toggleWidth, totalRect.height);
            Rect vecRect = new Rect(totalRect.x + toggleWidth, totalRect.y, totalRect.width - toggleWidth, totalRect.height);

            EditorGUI.BeginChangeCheck();
            var tog = EditorGUI.Toggle(toggleRect, label, toggle == 1) ? 1 : 0;
            if (EditorGUI.EndChangeCheck() && vec != null)
                vec.vectorValue = new Vector4(vec.vectorValue.x, vec.vectorValue.y, vec.vectorValue.z, tog);

            Vector4 newVec = vec != null ? vec.vectorValue : Vector4.zero;
            float fieldWidth = (vecRect.width / 3f) - 2f;
            EditorGUIUtility.labelWidth = 13f;
            EditorGUI.BeginDisabledGroup(toggle == 0);

            EditorGUI.BeginChangeCheck();

            // X Field
            Rect fieldRect = new Rect(vecRect.x, vecRect.y, fieldWidth, vecRect.height);
            newVec.x = EditorGUI.FloatField(fieldRect, "X", newVec.x);

            // Y Field
            fieldRect.x += fieldWidth + 2f;
            newVec.y = EditorGUI.FloatField(fieldRect, "Y", newVec.y);

            // Z Field
            fieldRect.x += fieldWidth + 2f;
            newVec.z = EditorGUI.FloatField(fieldRect, "Z", newVec.z);

            if (EditorGUI.EndChangeCheck() && vec != null)
                vec.vectorValue = new Vector4(newVec.x, newVec.y, newVec.z, tog);

            EditorGUI.EndDisabledGroup();
            EditorGUIUtility.labelWidth = origLabelWidth;

            if (vec != null) MaterialEditor.EndProperty();
            Space1();
        }

        // Vector3 property with corrected width scaling
        public static void Vector3Field(MaterialProperty vec, string label, bool needsIndent){
            Vector3Field(vec, new GUIContent(label), needsIndent);
        }

        public static void Vector3Field(MaterialProperty vec, GUIContent label, bool needsIndent){
            Rect totalRect = EditorGUILayout.GetControlRect();
            if (vec != null) MaterialEditor.BeginProperty(totalRect, vec);

            float origLabelWidth = EditorGUIUtility.labelWidth;
            float fieldWidth = (totalRect.width - origLabelWidth) / 3f;

            Rect labelRect = new Rect(totalRect.x, totalRect.y, origLabelWidth, totalRect.height);
            if (needsIndent) {
                label = new GUIContent("        " + label.text, label.tooltip);
            }
            EditorGUI.LabelField(labelRect, label);

            Vector4 newVec = vec != null ? vec.vectorValue : Vector4.zero;
            Rect fieldRect = new Rect(totalRect.x + origLabelWidth, totalRect.y, fieldWidth - 2f, totalRect.height);
            EditorGUIUtility.labelWidth = 13f;

            EditorGUI.BeginChangeCheck();

            // X Field
            newVec.x = EditorGUI.FloatField(fieldRect, "X", newVec.x);

            // Y Field
            fieldRect.x += fieldWidth + 2f;
            newVec.y = EditorGUI.FloatField(fieldRect, "Y", newVec.y);

            // Z Field
            fieldRect.x += fieldWidth + 2f;
            newVec.z = EditorGUI.FloatField(fieldRect, "Z", newVec.z);

            if (EditorGUI.EndChangeCheck() && vec != null)
                vec.vectorValue = newVec;

            EditorGUIUtility.labelWidth = origLabelWidth;

            if (vec != null) MaterialEditor.EndProperty();
        }

        // Vector3 property with corrected width scaling
        public static void Vector3FieldRGB(MaterialProperty vec, string label){
            Vector3FieldRGB(vec, new GUIContent(label));
        }

        public static void Vector3FieldRGB(MaterialProperty vec, GUIContent label){
            Rect totalRect = EditorGUILayout.GetControlRect();
            if (vec != null) MaterialEditor.BeginProperty(totalRect, vec);

            float origLabelWidth = EditorGUIUtility.labelWidth;
            float fieldWidth = (totalRect.width - origLabelWidth) / 3f;

            Rect labelRect = new Rect(totalRect.x, totalRect.y, origLabelWidth, totalRect.height);
            EditorGUI.LabelField(labelRect, label);

            Vector4 newVec = vec != null ? vec.vectorValue : Vector4.zero;
            Rect fieldRect = new Rect(totalRect.x + origLabelWidth, totalRect.y, fieldWidth - 2f, totalRect.height);
            EditorGUIUtility.labelWidth = 13f;

            EditorGUI.BeginChangeCheck();

            // R Field
            newVec.x = EditorGUI.FloatField(fieldRect, "R", newVec.x);

            // G Field
            fieldRect.x += fieldWidth + 2f;
            newVec.y = EditorGUI.FloatField(fieldRect, "G", newVec.y);

            // B Field
            fieldRect.x += fieldWidth + 2f;
            newVec.z = EditorGUI.FloatField(fieldRect, "B", newVec.z);

            if (EditorGUI.EndChangeCheck() && vec != null)
                vec.vectorValue = newVec;

            EditorGUIUtility.labelWidth = origLabelWidth;

            if (vec != null) MaterialEditor.EndProperty();
        }

        // Vector2 property with corrected width scaling
        public static void Vector2Field(MaterialProperty vec, string label){
            Vector2Field(vec, new GUIContent(label));
        }

        public static void Vector2Field(MaterialProperty vec, GUIContent label){
            Rect totalRect = EditorGUILayout.GetControlRect();
            if (vec != null) MaterialEditor.BeginProperty(totalRect, vec);

            float origLabelWidth = EditorGUIUtility.labelWidth;
            float fieldWidth = (totalRect.width - origLabelWidth) / 2f;

            Rect labelRect = new Rect(totalRect.x, totalRect.y, origLabelWidth, totalRect.height);
            EditorGUI.LabelField(labelRect, label);

            Vector4 newVec = vec != null ? vec.vectorValue : Vector4.zero;
            Rect fieldRect = new Rect(totalRect.x + origLabelWidth, totalRect.y, fieldWidth - 2f, totalRect.height);
            EditorGUIUtility.labelWidth = 13f;

            EditorGUI.BeginChangeCheck();

            // X Field
            newVec.x = EditorGUI.FloatField(fieldRect, "X", newVec.x);

            // Y Field
            fieldRect.x += fieldWidth + 2f;
            newVec.y = EditorGUI.FloatField(fieldRect, "Y", newVec.y);

            if (EditorGUI.EndChangeCheck() && vec != null)
                vec.vectorValue = newVec;

            EditorGUIUtility.labelWidth = origLabelWidth;

            if (vec != null) MaterialEditor.EndProperty();
        }

        public static void SliderMinMax(MaterialProperty minRange, MaterialProperty maxRange, float minLimit, float maxLimit, string label, int groupLayers){
            DoMinMaxSlider(minRange, maxRange, minLimit, maxLimit, new GUIContent(label), groupLayers);
        }

        public static void SliderMinMax(MaterialProperty minRange, MaterialProperty maxRange, float minLimit, float maxLimit, GUIContent label, int groupLayers){
            DoMinMaxSlider(minRange, maxRange, minLimit, maxLimit, label, groupLayers);
        }

        public static void SliderMinMax01(MaterialProperty minRange, MaterialProperty maxRange, string label, int groupLayers){
            DoMinMaxSlider(minRange, maxRange, 0f, 1f, new GUIContent(label), groupLayers);
        }

        public static void SliderMinMax01(MaterialProperty minRange, MaterialProperty maxRange, GUIContent label, int groupLayers){
            DoMinMaxSlider(minRange, maxRange, 0f, 1f, label, groupLayers);
        }
        
        private static void DoMinMaxSlider(MaterialProperty minRange, MaterialProperty maxRange, float minLimit, float maxLimit, GUIContent label, int groupLayers){
            float offset0 = 0f;
            switch (groupLayers){
                case 1: offset0 = 16f; break;
                case 2: offset0 = 20f; break;
                case 3: offset0 = 24f; break;
                default: break;
            }
            string numFormat = "F";
            float minR = minRange != null ? minRange.floatValue : 0f;
            float maxR = maxRange != null ? maxRange.floatValue : 1f;

            Rect totalRect = EditorGUILayout.GetControlRect();
            float sliderMidX = totalRect.x + EditorGUIUtility.labelWidth + (totalRect.width - EditorGUIUtility.labelWidth) * 0.5f;

            MaterialProperty copyProp = (Event.current.mousePosition.x > sliderMidX) ? maxRange : minRange;
            DoCompoundContextMenu(totalRect, copyProp, minRange, maxRange);

            Rect propRect = (Event.current.type == EventType.ContextClick || Event.current.rawType == EventType.ContextClick) ? Rect.zero : totalRect;
            if (minRange != null) MaterialEditor.BeginProperty(propRect, minRange);
            if (maxRange != null) MaterialEditor.BeginProperty(propRect, maxRange);

            float lw = EditorGUIUtility.labelWidth;
            Rect labelRect = new Rect(totalRect.x, totalRect.y, lw, totalRect.height);
            GUI.Label(labelRect, label);

            float propWidth = totalRect.width - lw;
            Rect minValRect = new Rect(totalRect.x + lw, totalRect.y, 45f, totalRect.height);
            GUI.Label(minValRect, minR.ToString(numFormat));

            float sliderX = minValRect.xMax + offset0;
            float sliderWidth = propWidth - 97f;
            Rect sliderRect = new Rect(sliderX, totalRect.y, sliderWidth, totalRect.height);

            EditorGUI.BeginChangeCheck();
            EditorGUI.MinMaxSlider(sliderRect, ref minR, ref maxR, minLimit, maxLimit);
            if (EditorGUI.EndChangeCheck()){
                if (minRange != null) minRange.floatValue = Mathf.Floor(minR * 100f) / 100f;
                if (maxRange != null) maxRange.floatValue = Mathf.Clamp(Mathf.Floor(maxR * 100f) / 100f, (minRange != null ? minRange.floatValue : 0f) + 0.01f, 2f);
            }

            Rect maxValRect = new Rect(totalRect.x + lw + propWidth - 45f, totalRect.y, 45f, totalRect.height);
            GUI.Label(maxValRect, maxR.ToString(numFormat));

            if (maxRange != null) MaterialEditor.EndProperty();
            if (minRange != null) MaterialEditor.EndProperty();
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

        public static void TexPropLabel(string text, int offset, bool isThirdProp){
            GUILayout.Space(-22);
            Rect rm = EditorGUILayout.GetControlRect();
            if (!isThirdProp)
                rm.x += GetInspectorWidth()-GetPropertyWidth()+20;
            else
                rm.x += GetInspectorWidth()-offset;
            EditorGUI.LabelField(rm, text);
        }

        public static void TexPropLabel(GUIContent text, int offset, bool isThirdProp){
            GUILayout.Space(-22);
            Rect rm = EditorGUILayout.GetControlRect();
            if (!isThirdProp)
                rm.x += GetInspectorWidth()-GetPropertyWidth()+20;
            else
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
            Space1();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            action();
            EditorGUILayout.EndVertical();
            Space1();
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

        public static void PropertyGroupParent(Action action){
            Space6();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            action();
            EditorGUILayout.EndVertical();
            Space6();
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

        public static void BoldLabel(GUIContent text){
            EditorGUILayout.LabelField(text);
        }
        
        public static void Label(string text){
            EditorGUILayout.LabelField(text);
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
            Space2();
            return buttonPress;
        }

        public static void sRGBWarning(MaterialProperty tex){
            if (tex.textureValue){
                string warningText = "This texture is marked as sRGB, but should be linear.";
                string texPath = AssetDatabase.GetAssetPath(tex.textureValue);
                TextureImporter texImporter;
                var importer = TextureImporter.GetAtPath(texPath) as TextureImporter;
                if (importer != null){
                    texImporter = (TextureImporter)importer;
                    if (texImporter.sRGBTexture){
                        if (TextureImportWarningBox(warningText)){
                            texImporter.sRGBTexture = false;
                            texImporter.SaveAndReimport();
                        }
                    }
                }
            }
        }

        public static void NormalWarning(MaterialProperty tex){
            if (tex.textureValue){
                string warningText = "This texture is not marked as a normal map.";
                string texPath = AssetDatabase.GetAssetPath(tex.textureValue);
                TextureImporter texImporter;
                var importer = TextureImporter.GetAtPath(texPath) as TextureImporter;
                if (importer != null){
                    texImporter = (TextureImporter)importer;
                    if (texImporter.textureType != TextureImporterType.NormalMap){
                        if (TextureImportWarningBox(warningText)){
                            texImporter.textureType = TextureImporterType.NormalMap;
                            texImporter.SaveAndReimport();
                        }
                    }
                }
            }
        }

        public static void CheckTrilinear(Texture tex, MaterialEditor me) {
            if(!tex)
                return;
            if(tex.mipmapCount <= 1) {
                me.HelpBoxWithButton(
                    EditorGUIUtility.TrTextContent("Mip maps are required, please enable them in the texture import settings."),
                    EditorGUIUtility.TrTextContent("OK"));
                return;
            }
            if(tex.filterMode != FilterMode.Trilinear) {
                if(me.HelpBoxWithButton(
                    EditorGUIUtility.TrTextContent("Trilinear filtering is required, and aniso is recommended."),
                    EditorGUIUtility.TrTextContent("Fix Now"))) {
                    tex.filterMode = FilterMode.Trilinear;
                    tex.anisoLevel = 1;
                    EditorUtility.SetDirty(tex);
                }
                return;
            }
        }

        /// <summary>
        /// Draws an enum dropdown control for the provided enum type. If the enum type
        /// has the <see cref="System.FlagsAttribute"/> attribute, a flags dropdown is
        /// drawn. Otherwise, a normal enum selector is shown.
        ///
        /// This function takes (and updates) a MaterialProperty.
        /// </summary>
        /// <param name="prop">The material property to read and write</param>
        /// <typeparam name="T">The enum type to use</typeparam>
        public static void EnumDropdown<T>(MaterialProperty prop, string label) where T : Enum
        {
            EnumDropdown<T>(prop, new GUIContent(label));
        }

        public static void EnumDropdown<T>(MaterialProperty prop, GUIContent label) where T : Enum
        {
            Rect r = EditorGUILayout.GetControlRect();
            if (prop != null) MaterialEditor.BeginProperty(r, prop);
            EditorGUI.BeginChangeCheck();

            int intValue = prop != null ? prop.intValue : 0;
            
            // a generic enum parameter can't be directly casted to an int,
            // so this is the most performant option (rather than casting
            // to object first, which would allocate some garbage)
            
            T enumValue = (T)Enum.ToObject(typeof(T), intValue);
            
            if (typeof(T).GetCustomAttribute<FlagsAttribute>() != null)
                enumValue = (T)EditorGUI.EnumFlagsField(r, label, enumValue);
            else
                enumValue = (T)EditorGUI.EnumPopup(r, label, enumValue);

            if (EditorGUI.EndChangeCheck() && prop != null)
                prop.intValue = Convert.ToInt32(enumValue);

            if (prop != null) MaterialEditor.EndProperty();
        }

        /// <summary>
        /// Draws an enum dropdown control for the provided enum type. If the enum type
        /// has the <see cref="System.FlagsAttribute"/> attribute, a flags dropdown is
        /// drawn. Otherwise, a normal enum selector is shown.
        ///
        /// This function uses an actual enum value directly.
        /// </summary>
        /// <param name="currentValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T EnumDropdown<T>(T currentValue, string label) where T : Enum
        {
            return EnumDropdown(currentValue, new GUIContent(label));
        }

        public static T EnumDropdown<T>(T currentValue, GUIContent label) where T : Enum
        {
            if (typeof(T).GetCustomAttribute<FlagsAttribute>() != null)
                return (T)EditorGUILayout.EnumFlagsField(label, currentValue);
            else
                return (T)EditorGUILayout.EnumPopup(label, currentValue);
        }

        // Shorthand spacing funcs
        public static void SpaceN24(){ GUILayout.Space(-24); }
        public static void SpaceN22(){ GUILayout.Space(-22); }
        public static void SpaceN20(){ GUILayout.Space(-20); }
        public static void SpaceN19(){ GUILayout.Space(-19); }
        public static void SpaceN18() { GUILayout.Space(-18); }
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

        static string[] cheeseList = {
            "EngineerIsaac",
            "Purriku",
            "BooneDoggy",
            "Danimals",
            "RealRewriteOfficial",
            "The_Jokester"
        };
    }
}