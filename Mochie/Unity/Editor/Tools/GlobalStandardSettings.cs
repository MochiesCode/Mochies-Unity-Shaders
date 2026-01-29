using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.SceneManagement;
using System.IO;

namespace Mochie {
    public class GlobalStandardSettings : EditorWindow {
        private static readonly int MaterialDebugModeID = Shader.PropertyToID("_MaterialDebugMode");
        private static readonly int DebugFlagID = Shader.PropertyToID("_DebugFlags");

        enum HueMode {HSV, Oklab, Unchanged}
        enum ToggleOffOn {Off, On, Unchanged}
        enum BakeryMode {None, SH, RNM, MonoSH, Unchanged}
        enum SpecularityShadingModel {Unity_Standard, Google_Filament, Unchanged}
        enum AreaLitOcclusionUVSet {UV0, UV1, UV2, UV3, UV4, LightmapUV, UV5, Unchanged}
        enum SrcShaderSelection {Unity_Standard, M_Standard, M_Standard_Lite, M_Standard_Mobile}
        enum DestShaderSelection {M_Standard, M_Standard_Lite, M_Standard_Mobile}
        bool applyToScene = true;
        bool inactive = true;

        Shader standardShader;
        Shader standardLiteShader;
        Shader standardMobileShader;

        SrcShaderSelection srcShader = SrcShaderSelection.M_Standard;
        DestShaderSelection destShader = DestShaderSelection.M_Standard_Lite;

        List<Material> projectMaterials = new List<Material>();
        List<Material> sceneMaterials = new List<Material>();
        List<Material> standardMaterials = new List<Material>();
        List<Material> standardLiteMaterials = new List<Material>();
        List<Material> standardMobileMaterials = new List<Material>();
        List<Material> standardUnityMaterials = new List<Material>();

        // Bakery settings
        BakeryMode dirMode = BakeryMode.Unchanged;
        ToggleOffOn bicubicSampling = ToggleOffOn.Unchanged;
        ToggleOffOn nonLinearSH = ToggleOffOn.Unchanged;
        ToggleOffOn lightmapSpecular = ToggleOffOn.Unchanged;
        ToggleOffOn additiveLightVolumes = ToggleOffOn.Unchanged;

        // Filtering settings
        HueMode hueMode = HueMode.Unchanged;
        ToggleOffOn filteringToggle = ToggleOffOn.Unchanged;
        float filteringHue = 0f;
        float filteringSat = 1f;
        float filteringBright = 1f;
        float filteringCont = 1f;
        float filteringACES = 0f;
        
        // Specularity Settings
        SpecularityShadingModel shadingModel = SpecularityShadingModel.Unchanged;
        ToggleOffOn reflToggle = ToggleOffOn.Unchanged;
        ToggleOffOn specToggle = ToggleOffOn.Unchanged;
        
        // AreaLit settings
        ToggleOffOn areaLitToggle = ToggleOffOn.Unchanged;
        ToggleOffOn areaLitSpecularOcclusion = ToggleOffOn.Unchanged;
        float areaLitStrength = 1f;
        float areaLitRoughnessMultiplier = 1f;
        Texture2D lightMesh;
        Texture2D lightTex0;
        Texture2D lightTex1;
        Texture2D lightTex2;
        Texture2D lightTex3;
        Texture2D areaLitOcclusion;
        AreaLitOcclusionUVSet areaLitOcclusionUVSet = AreaLitOcclusionUVSet.Unchanged;

        DebugFlags globalDebugFlags;
        Vector2 scrollPos;

        [MenuItem("Tools/Mochie/Global Standard Settings")]
        static void Init(){
            GlobalStandardSettings window = (GlobalStandardSettings)EditorWindow.GetWindow(typeof(GlobalStandardSettings));
            window.titleContent = new GUIContent("Standard Shader Settings");
            window.minSize = new Vector2(300, 600);
            window.maxSize = new Vector2(300, 800);
            window.Show();
        }

        void Awake(){
            standardShader = Shader.Find("Mochie/Standard");
            standardLiteShader = Shader.Find("Mochie/Standard Lite");
            standardMobileShader = Shader.Find("Mochie/Standard Mobile");
            RefreshMaterials();
        }

        void OnGUI(){
            float buttonWidth = MGUI.GetInspectorWidth()-6f;
            float scrollButtonWidth = MGUI.GetInspectorWidth()-19f;
            float groupButtonWidth = MGUI.GetInspectorWidth()-27f;
            
            EditorGUI.BeginChangeCheck();
            applyToScene = EditorGUILayout.Toggle("Scene Materials Only", applyToScene);
            MGUI.ToggleGroup(!applyToScene);
            inactive = EditorGUILayout.Toggle("Inactive Objects", inactive);
            MGUI.ToggleGroupEnd();
            if (EditorGUI.EndChangeCheck()){
                RefreshMaterials();
            }
            
            string lol = applyToScene ? "material slots in scene" : "materials in project";
            MGUI.DisplayText("Found " + standardMaterials.Count + " Mochie Standard " + lol + "\nFound " + standardLiteMaterials.Count + " Mochie Standard Lite " + lol + "\nFound " + standardMobileMaterials.Count + " Mochie Standard Mobile "+ lol + "\nFound " + standardUnityMaterials.Count + " Unity Standard " + lol);

            if (MGUI.SimpleButton("Refresh Materials List", buttonWidth, 0f)){
                RefreshMaterials();
            }

            if (MGUI.SimpleButton("Restore Default Textures", buttonWidth, 0f)){
                RestoreDefaultTextures();
            }

            EditorGUI.BeginChangeCheck();
            globalDebugFlags = MGUI.EnumDropdown(globalDebugFlags, new GUIContent("Debug View"));
            if (EditorGUI.EndChangeCheck()){
                ApplyDebugView();
            }

            srcShader = (SrcShaderSelection)EditorGUILayout.EnumPopup("Change from:", srcShader);
            destShader = (DestShaderSelection)EditorGUILayout.EnumPopup("To:", destShader);
            if (MGUI.SimpleButton("Swap Shaders", buttonWidth, 0f)){
                if (srcShader == SrcShaderSelection.M_Standard && destShader == DestShaderSelection.M_Standard_Lite)
                    MigrateFromStandardToLite();
                else if (srcShader == SrcShaderSelection.M_Standard && destShader == DestShaderSelection.M_Standard_Mobile)
                    MigrateFromStandardToMobile();
                else if (srcShader == SrcShaderSelection.M_Standard_Lite && destShader == DestShaderSelection.M_Standard)
                    MigrateFromLiteToStandard();
                else if (srcShader == SrcShaderSelection.M_Standard_Lite && destShader == DestShaderSelection.M_Standard_Mobile)
                    MigrateFromLiteToMobile();
                else if (srcShader == SrcShaderSelection.M_Standard_Mobile && destShader == DestShaderSelection.M_Standard)
                    MigrateFromMobileToStandard();
                else if (srcShader == SrcShaderSelection.M_Standard_Mobile && destShader == DestShaderSelection.M_Standard_Lite)
                    MigrateFromMobileToLite();
                else if (srcShader == SrcShaderSelection.Unity_Standard && destShader == DestShaderSelection.M_Standard)
                    MigrateFromUnityStandardToStandard();
                else if (srcShader == SrcShaderSelection.Unity_Standard && destShader == DestShaderSelection.M_Standard_Lite)
                    MigrateFromUnityStandardToLite();
                else if (srcShader == SrcShaderSelection.Unity_Standard && destShader == DestShaderSelection.M_Standard_Mobile)
                    MigrateFromUnityStandardToMobile();
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            MGUI.Space8();
            MGUI.BoldLabel("Specularity Settings");
            MGUI.PropertyGroup(()=>{
                shadingModel = (SpecularityShadingModel)EditorGUILayout.EnumPopup("Shading Model", shadingModel);
                reflToggle = (ToggleOffOn)EditorGUILayout.EnumPopup("Reflections", reflToggle);
                specToggle = (ToggleOffOn)EditorGUILayout.EnumPopup("Specular Highlights", specToggle);
            });
            if (MGUI.SimpleButton("Apply", scrollButtonWidth, 0f)){
                ApplySpecSettings();
            }

            MGUI.Space8();
            MGUI.BoldLabel("Lightmapping Settings");
            MGUI.PropertyGroup(()=>{
                dirMode = (BakeryMode)EditorGUILayout.EnumPopup("Directional Mode", dirMode);
                bicubicSampling = (ToggleOffOn)EditorGUILayout.EnumPopup("Bicubic Sampling", bicubicSampling);
                nonLinearSH = (ToggleOffOn)EditorGUILayout.EnumPopup("Non-Linear SH", nonLinearSH);
                lightmapSpecular = (ToggleOffOn)EditorGUILayout.EnumPopup("Lightmap Specular", lightmapSpecular);
                additiveLightVolumes = (ToggleOffOn)EditorGUILayout.EnumPopup("Additive Light Volumes", additiveLightVolumes);
            });
            if (MGUI.SimpleButton("Apply", scrollButtonWidth, 0f)){
                ApplyBakerySettings();
            }

            MGUI.Space8();
            MGUI.BoldLabel("Filtering Settings");
            MGUI.PropertyGroup(()=>{
                filteringToggle = (ToggleOffOn)EditorGUILayout.EnumPopup("Enable", filteringToggle);
                hueMode = (HueMode)EditorGUILayout.EnumPopup("Hue Mode", hueMode);
                filteringHue = EditorGUILayout.Slider("Hue", filteringHue, 0f, 1f);
                filteringSat = EditorGUILayout.FloatField("Saturation", filteringSat);
                filteringBright = EditorGUILayout.FloatField("Brightness", filteringBright);
                filteringCont = EditorGUILayout.FloatField("Contrast", filteringCont);
                filteringACES = EditorGUILayout.FloatField("ACES", filteringACES);
            });
            if (MGUI.SimpleButton("Apply", scrollButtonWidth, 0f)){
                ApplyFilterSettings();
            }

            MGUI.Space8();
            MGUI.BoldLabel("AreaLit Settings");
            MGUI.PropertyGroup(()=>{
                areaLitToggle = (ToggleOffOn)EditorGUILayout.EnumPopup("Enable", areaLitToggle);
                areaLitSpecularOcclusion = (ToggleOffOn)EditorGUILayout.EnumPopup("Specular Occlusion", areaLitSpecularOcclusion);
                areaLitStrength = EditorGUILayout.FloatField("Strength", areaLitStrength);
                areaLitRoughnessMultiplier = EditorGUILayout.FloatField("Roughness Multiplier", areaLitRoughnessMultiplier);
                lightMesh = (Texture2D)EditorGUILayout.ObjectField("Light Mesh", lightMesh, typeof(Texture2D), true, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                lightTex0 = (Texture2D)EditorGUILayout.ObjectField("Light Texture 0", lightTex0, typeof(Texture2D), true, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                lightTex1 = (Texture2D)EditorGUILayout.ObjectField("Light Texture 1", lightTex1, typeof(Texture2D), true, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                lightTex2 = (Texture2D)EditorGUILayout.ObjectField("Light Texture 2", lightTex2, typeof(Texture2D), true, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                lightTex3 = (Texture2D)EditorGUILayout.ObjectField("Light Texture 3", lightTex3, typeof(Texture2D), true, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                areaLitOcclusion = (Texture2D)EditorGUILayout.ObjectField("Occlusion", areaLitOcclusion, typeof(Texture2D), true, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                areaLitOcclusionUVSet = (AreaLitOcclusionUVSet)EditorGUILayout.EnumPopup("Occlusion UV Set", areaLitOcclusionUVSet);
            });
            if (MGUI.SimpleButton("Apply", scrollButtonWidth, 0f)){
                ApplyAreaLitSettings();
            }
            EditorGUILayout.EndScrollView();
            MGUI.DisplayWarning("Please note that changes made with this utility cannot be undone, pick your settings carefully!");
        }
        
        void MigrateFromStandardToLite(){
            if (standardMaterials != null){
                foreach(Material m in standardMaterials){
                    m.shader = standardLiteShader;
                }
            }
            RefreshMaterials();
        }

        void MigrateFromStandardToMobile(){
            if (standardMaterials != null){
                foreach(Material m in standardMaterials){
                    m.shader = standardMobileShader;
                }
            }
            RefreshMaterials();
        }

        void MigrateFromLiteToStandard(){
            if (standardLiteMaterials != null){
                foreach(Material m in standardLiteMaterials){
                    m.shader = standardShader;
                }
            }
            RefreshMaterials();
        }

        void MigrateFromLiteToMobile(){
            if (standardLiteMaterials != null){
                foreach(Material m in standardLiteMaterials){
                    m.shader = standardMobileShader;
                }
            }
            RefreshMaterials();
        }

        void MigrateFromMobileToStandard(){
            if (standardMobileMaterials != null){
                foreach(Material m in standardMobileMaterials){
                    m.shader = standardShader;
                }
            }
            RefreshMaterials();
        }

        void MigrateFromMobileToLite(){
            if (standardMobileMaterials != null){
                foreach(Material m in standardMobileMaterials){
                    m.shader = standardLiteShader;
                }
            }
            RefreshMaterials();
        }

        void MigrateFromUnityStandardToStandard(){
            if (standardUnityMaterials != null){
                foreach (Material m in standardUnityMaterials){
                    m.shader = standardShader;
                }
            }
            RefreshMaterials();
        }

        void MigrateFromUnityStandardToLite(){
            if (standardUnityMaterials != null){
                foreach (Material m in standardUnityMaterials){
                    m.shader = standardLiteShader;
                }
            }
            RefreshMaterials();
        }

        void MigrateFromUnityStandardToMobile(){
            if (standardUnityMaterials != null){
                foreach (Material m in standardUnityMaterials){
                    m.shader = standardMobileShader;
                }
            }
            RefreshMaterials();
        }

        void ApplyBakerySettings(){
            List<Material> materials = new List<Material>();
            materials.AddRange(standardMaterials);
            materials.AddRange(standardLiteMaterials);
            materials.AddRange(standardMobileMaterials);
            foreach (Material m in materials){

                // Directional Mode
                if (dirMode != BakeryMode.Unchanged){
                    m.SetInt("_BakeryMode", (int)dirMode);
                    MGUI.SetKeyword(m, "BAKERY_SH", dirMode == BakeryMode.SH);
                    MGUI.SetKeyword(m, "BAKERY_RNM", dirMode == BakeryMode.RNM);
                    MGUI.SetKeyword(m, "BAKERY_MONOSH", dirMode == BakeryMode.MonoSH);
                }

                // Bicubic Lightmapping
                if (bicubicSampling != ToggleOffOn.Unchanged){
                    m.SetInt("_BicubicSampling", (int)bicubicSampling);
                    MGUI.SetKeyword(m, "_BICUBIC_SAMPLING_ON", (int)bicubicSampling == 1);
                }

                // Nonlinear SH
                if (nonLinearSH != ToggleOffOn.Unchanged){
                    m.SetInt("_BAKERY_SHNONLINEAR", (int)nonLinearSH);
                    MGUI.SetKeyword(m, "BAKERY_SHNONLINEAR", (int)nonLinearSH == 1);
                }

                // Lightmapped Specular
                if (lightmapSpecular != ToggleOffOn.Unchanged){
                    m.SetInt("_BAKERY_LMSPEC", (int)lightmapSpecular);
                    MGUI.SetKeyword(m, "BAKERY_LMSPEC", (int)lightmapSpecular == 1);
                }

                // Additive light volumes
                if (additiveLightVolumes != ToggleOffOn.Unchanged){
                    m.SetInt("_AdditiveLightVolumesToggle", (int)additiveLightVolumes);
                }
            }
        }

        void ApplyFilterSettings(){
            List<Material> materials = new List<Material>();
            materials.AddRange(standardMaterials);
            materials.AddRange(standardLiteMaterials);
            materials.AddRange(standardMobileMaterials);
            foreach (Material m in materials){
                if (filteringToggle != ToggleOffOn.Unchanged)
                    m.SetInt("_Filtering", (int)filteringToggle);
                m.SetFloat("_HuePost", filteringHue);
                if (hueMode != HueMode.Unchanged)
                    m.SetFloat("_HueMode", (int)hueMode);
                m.SetFloat("_SaturationPost", filteringSat);
                m.SetFloat("_BrightnessPost", filteringBright);
                m.SetFloat("_ContrastPost", filteringCont);
                m.SetFloat("_ACES", filteringACES);
            }
        }

        void ApplySpecSettings(){
            List<Material> materials = new List<Material>();
            materials.AddRange(standardMaterials);
            materials.AddRange(standardLiteMaterials);
            materials.AddRange(standardMobileMaterials);
            foreach (Material m in materials){
                if (shadingModel != SpecularityShadingModel.Unchanged)
                    m.SetInt("_ShadingModel", (int)shadingModel);
                if (reflToggle != ToggleOffOn.Unchanged){
                    m.SetInt("_ReflectionsToggle", (int)reflToggle);
                    MGUI.SetKeyword(m, "_REFLECTIONS_ON", (int)reflToggle == 1);
                }
                if (specToggle != ToggleOffOn.Unchanged){
                    m.SetInt("_SpecularHighlightsToggle", (int)specToggle);
                    MGUI.SetKeyword(m, "_SPECULARHIGHLIGHTS_ON", (int)specToggle == 1);
                }
            }
        }

        void ApplyAreaLitSettings(){
            List<Material> materials = new List<Material>();
            materials.AddRange(standardMaterials);
            materials.AddRange(standardLiteMaterials);
            materials.AddRange(standardMobileMaterials);
            foreach (Material m in materials){
                if (areaLitToggle != ToggleOffOn.Unchanged){
                    m.SetInt("_AreaLitToggle", (int)areaLitToggle);
                    MGUI.SetKeyword(m, "_AREALIT_ON", (int)areaLitToggle == 1);
                }
                if (areaLitSpecularOcclusion != ToggleOffOn.Unchanged)
                    m.SetInt("_AreaLitSpecularOcclusion", (int)areaLitSpecularOcclusion);
                m.SetFloat("_AreaLitStrength", areaLitStrength);
                m.SetFloat("_AreaLitRoughnessMultiplier", areaLitRoughnessMultiplier);
                m.SetTexture("_LightMesh", lightMesh);
                m.SetTexture("_LightTex0", lightTex0);
                m.SetTexture("_LightTex1", lightTex1);
                m.SetTexture("_LightTex2", lightTex2);
                m.SetTexture("_LightTex3", lightTex3);
                m.SetTexture("_AreaLitOcclusion", areaLitOcclusion);
                if (areaLitOcclusionUVSet != AreaLitOcclusionUVSet.Unchanged)
                    m.SetInt("_AreaLitOcclusionUVSet", (int)areaLitOcclusionUVSet);
            }
        }

        void ApplyDebugView(){
            List<Material> materials = new List<Material>();
            materials.AddRange(standardMaterials);
            materials.AddRange(standardLiteMaterials);
            materials.AddRange(standardMobileMaterials);
            foreach (Material m in materials){
                HandleDebugView(m);
            }
        }

        void RestoreDefaultTextures(){
            List<Material> materials = new List<Material>();
            materials.AddRange(standardMaterials);
            materials.AddRange(standardLiteMaterials);
            materials.AddRange(standardMobileMaterials);
            string texFolder = "Assets/Mochie/Unity/Textures/";
            Texture dfgTex = AssetDatabase.LoadAssetAtPath(texFolder + "dfg-multiscatter.exr", typeof(Texture)) as Texture;
            Texture rainSheetTex = AssetDatabase.LoadAssetAtPath(texFolder + "Glass_Rain_Texturesheet.png", typeof(Texture)) as Texture;
            Texture defaultTex = AssetDatabase.LoadAssetAtPath(texFolder + "White Swatch (Primary).png", typeof(Texture)) as Texture;
            Texture defaultDetailTex = AssetDatabase.LoadAssetAtPath(texFolder + "White Swatch (Detail).png", typeof(Texture)) as Texture;
            Texture dropletMaskTex = AssetDatabase.LoadAssetAtPath(texFolder + "Droplet Mask.tif", typeof(Texture)) as Texture;
            Texture ssrNoiseTex = AssetDatabase.LoadAssetAtPath(texFolder + "SSR Noise.png", typeof(Texture)) as Texture;
            foreach (Material m in materials){
                m.SetTexture("_DefaultSampler", defaultTex);
                m.SetTexture("_DefaultDetailSampler", defaultDetailTex);
                m.SetTexture("_DFG", dfgTex);
                m.SetTexture("_RainSheet", rainSheetTex);
                m.SetTexture("_DropletMask", dropletMaskTex);
                m.SetTexture("_NoiseTexSSR", ssrNoiseTex);
            }
        }

        void RefreshMaterials(){
            ClearLists();
            PopulateSceneMaterials();
            PopulateProjectMaterials();
            FilterMaterials();
        }

        void ClearLists(){
            projectMaterials.Clear();
            sceneMaterials.Clear();
            standardMaterials.Clear();
            standardLiteMaterials.Clear();
            standardMobileMaterials.Clear();
            standardUnityMaterials.Clear();
        }

        void PopulateSceneMaterials(){
            sceneMaterials = SceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(g => g.GetComponentsInChildren<Renderer>(inactive)).ToList()
                .SelectMany(r => r.sharedMaterials).ToList();
        }

        void PopulateProjectMaterials(){
            projectMaterials = FindAssetsByType<Material>();
        }

        void FilterMaterials(){
            List<Material> matsToFilter = applyToScene ? sceneMaterials : projectMaterials;
            foreach (Material m in matsToFilter){
                if (m != null && m.shader != null && m.shader.name != null){
                    string shaderName = m.shader.name;
                    if (shaderName == "Mochie/Standard"){
                        standardMaterials.Add(m);
                    }
                    else if (shaderName == "Mochie/Standard (Lite)" || shaderName == "Mochie/Standard Lite"){
                        standardLiteMaterials.Add(m);
                    }
                    else if (shaderName == "Mochie/Standard Mobile"){
                        standardMobileMaterials.Add(m);
                    }
                    else if (shaderName == "Standard" || shaderName == "Autodesk Interactive"){
                        standardUnityMaterials.Add(m);
                    }
                }
            }
        }

        static List<T> FindAssetsByType<T>() where T : UnityEngine.Object {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof (T).ToString().Replace("UnityEngine.", "")));
            for (int i = 0; i < guids.Length; i++){
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null){
                    assets.Add(asset);
                }
            }
            return assets;
        }

        void HandleDebugView(Material m){
            m.SetFloat(MaterialDebugModeID, globalDebugFlags == Mochie.DebugFlags.None ? 0 : 1);
            m.SetInteger(DebugFlagID, (int)globalDebugFlags);
        }
    }
}