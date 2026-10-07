using UnityEngine;
using UnityEditor;

namespace Mochie {

    // Grabpasses used to be tagged "LightMode"="Always" and are now tagged "LightMode"="GrabPass".
    // Materials saved before the change have "Always" in their disabled passes instead of "GrabPass",
    // so their grabpass keeps running with SSR/distortion off until the material is opened in the inspector.
    public static class GrabpassFixer {

        [MenuItem("Tools/Mochie/Fix Old Grabpass Settings")]
        public static void FixAllMaterials(){
            if (!EditorUtility.DisplayDialog("Fix Old Grabpass Settings",
                "Scans every material in the project that uses Mochie Standard, Splat Mapping, Glass, Water or Particles, and turns off grabpasses that are left running by settings from older versions.\n\nThis can take a while on large projects.",
                "Fix Materials", "Cancel"))
                return;

            string[] guids = AssetDatabase.FindAssets("t:Material", new[] {"Assets"});
            int fixedCount = 0;
            try {
                for (int i = 0; i < guids.Length; i++){
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (EditorUtility.DisplayCancelableProgressBar("Fix Old Grabpass Settings", path, (float)i / guids.Length))
                        break;
                    if (FixMaterial(AssetDatabase.LoadAssetAtPath<Material>(path)))
                        fixedCount++;
                }
            }
            finally {
                EditorUtility.ClearProgressBar();
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Fix Old Grabpass Settings", $"Fixed {fixedCount} material(s). See the console for the list.", "OK");
        }

        static bool FixMaterial(Material mat){
            if (mat == null || mat.shader == null)
                return false;

            #if UNITY_2022_1_OR_NEWER
            // Variants inherit the fix from their parent
            if (mat.parent != null)
                return false;
            #endif

            bool grabpass, always;
            switch (mat.shader.name){
                case "Mochie/Standard":
                case "Mochie/Splat Mapping":
                    grabpass = mat.GetInt("_SSRToggle") == 1;
                    always = true;
                    break;
                case "Mochie/Standard Lite":
                case "Mochie/Standard Mobile":
                    grabpass = false;
                    always = true;
                    break;
                case "Mochie/Glass":
                    grabpass = mat.GetInt("_BlendMode") == 0;
                    always = true;
                    break;
                case "Mochie/Water":
                case "Mochie/Water (Tessellated)":
                    grabpass = mat.GetInt("_TransparencyMode") == 2;
                    always = true;
                    break;
                case "Mochie/Particles":
                case "Mochie/Particles X":
                    // Particles X uses "Always" for its outline pass
                    int blendMode = mat.GetInt("_BlendMode");
                    grabpass = mat.GetInt("_Distortion") == 1 && blendMode != 6;
                    always = mat.GetInt("_Outlines") == 1 && blendMode == 6;
                    break;
                default:
                    return false;
            }

            if (mat.GetShaderPassEnabled("GrabPass") == grabpass && mat.GetShaderPassEnabled("Always") == always)
                return false;

            Undo.RecordObject(mat, "Fix Old Grabpass Settings");
            mat.SetShaderPassEnabled("GrabPass", grabpass);
            mat.SetShaderPassEnabled("Always", always);
            EditorUtility.SetDirty(mat);
            Debug.Log($"Fixed grabpass settings on {mat.name} ({mat.shader.name})", mat);
            return true;
        }
    }
}
