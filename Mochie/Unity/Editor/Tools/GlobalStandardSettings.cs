using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System.IO;

namespace Mochie {
    public class GlobalStandardSettings : EditorWindow {
        private static readonly int MaterialDebugModeID = Shader.PropertyToID("_MaterialDebugMode");
        private static readonly int DebugFlagID = Shader.PropertyToID("_DebugFlags");

        enum HueMode {HSV, Oklab}
        enum ToggleOffOn {Off, On}
        enum BakeryMode {None, SH, RNM, MonoSH}
        enum SpecularityShadingModel {Unity_Standard, Google_Filament}
        enum AreaLitOcclusionUVSet {UV0, UV1, UV2, UV3, UV4, LightmapUV}
        enum SrcShaderSelection {Unity_Standard, Filamented, M_Standard, M_Standard_Lite, M_Standard_Mobile}
        enum DestShaderSelection {M_Standard, M_Standard_Lite, M_Standard_Mobile}
        bool applyToScene = true;
        bool inactive = true;

        Shader standardShader;
        Shader standardLiteShader;
        Shader standardMobileShader;
        Shader filamentShader;

        SrcShaderSelection srcShader = SrcShaderSelection.M_Standard;
        DestShaderSelection destShader = DestShaderSelection.M_Standard_Lite;

        List<Material> projectMaterials = new List<Material>();
        List<Material> sceneMaterials = new List<Material>();
        List<Material> standardMaterials = new List<Material>();
        List<Material> standardLiteMaterials = new List<Material>();
        List<Material> standardMobileMaterials = new List<Material>();
        List<Material> standardUnityMaterials = new List<Material>();
        List<Material> filamentedMaterials = new List<Material>();

        // Lightmap settings
        BakeryMode dirMode = BakeryMode.None;
        ToggleOffOn bicubicSampling = ToggleOffOn.On;
        ToggleOffOn nonLinearSH = ToggleOffOn.Off;
        ToggleOffOn lightmapSpecular = ToggleOffOn.Off;
        ToggleOffOn additiveLightVolumes = ToggleOffOn.On;

        bool applyDirMode = false;
        bool applyBicubicSampling = false;
        bool applyNonLinearSH = false;
        bool applyLightmapSpecular = false;
        bool applyAdditiveLightVolumes = false;

        // Filtering settings
        HueMode hueMode = HueMode.HSV;
        ToggleOffOn filteringToggle = ToggleOffOn.Off;
        float filteringHue = 0f;
        float filteringSat = 1f;
        float filteringBright = 1f;
        float filteringCont = 1f;
        float filteringACES = 0f;

        bool applyFilteringToggle = false;
        bool applyHueMode = false;
        bool applyFilteringHue = false;
        bool applyFilteringSat = false;
        bool applyFilteringBright = false;
        bool applyFilteringCont = false;
        bool applyFilteringACES = false;
        
        // Specularity Settings
        SpecularityShadingModel shadingModel = SpecularityShadingModel.Unity_Standard;
        ToggleOffOn reflToggle = ToggleOffOn.On;
        ToggleOffOn specToggle = ToggleOffOn.On;

        bool applyShadingModel = false;
        bool applyReflToggle = false;
        bool applySpecToggle = false;
        
        // AreaLit settings
        ToggleOffOn areaLitToggle = ToggleOffOn.Off;
        ToggleOffOn areaLitSpecularOcclusion = ToggleOffOn.Off;
        float areaLitStrength = 1f;
        float areaLitRoughnessMultiplier = 1f;
        Texture lightMesh;
        Texture lightTex0;
        Texture lightTex1;
        Texture lightTex2;
        Texture lightTex3;
        Texture areaLitOcclusion;
        AreaLitOcclusionUVSet areaLitOcclusionUVSet = AreaLitOcclusionUVSet.UV0;

        bool applyAreaLitToggle = false;
        bool applyAreaLitSpecularOcclusion = false;
        bool applyAreaLitStrength = false;
        bool applyAreaLitRoughnessMultiplier = false;
        bool applyLightMesh = false;
        bool applyLightTex0 = false;
        bool applyLightTex1 = false;
        bool applyLightTex2 = false;
        bool applyLightTex3 = false;
        bool applyAreaLitOcclusion = false;
        bool applyAreaLitOcclusionUVSet = false;

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

        bool DrawToggleProperty(bool toggleState, System.Action drawControl) {
            EditorGUILayout.BeginHorizontal();
            bool newToggleState = EditorGUILayout.Toggle(toggleState, GUILayout.Width(15));
            float oldLabelWidth = EditorGUIUtility.labelWidth;
            float currentLw = oldLabelWidth > 0 ? oldLabelWidth : (EditorGUIUtility.currentViewWidth > 0 ? EditorGUIUtility.currentViewWidth * 0.4f : 120f);
            EditorGUIUtility.labelWidth = currentLw - 20f;
            EditorGUI.BeginDisabledGroup(!newToggleState);
            drawControl();
            EditorGUI.EndDisabledGroup();
            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorGUILayout.EndHorizontal();
            return newToggleState;
        }

        // Accepts any texture class (Texture2D, RenderTexture, etc.) as long as its dimension matches the shader property
        Texture DrawTextureField(string label, Texture tex, TextureDimension dimension){
            Texture newTex = (Texture)EditorGUILayout.ObjectField(label, tex, typeof(Texture), true, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            if (newTex != null && newTex.dimension != dimension){
                Debug.LogWarning(label + " only accepts " + (dimension == TextureDimension.Tex2DArray ? "texture arrays" : "2D textures") + ", but " + newTex.name + " is " + newTex.dimension + ".");
                return tex;
            }
            return newTex;
        }

        void OnGUI(){
            float buttonWidth = MGUI.GetInspectorWidth()-6f;
            
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
                else if (srcShader == SrcShaderSelection.Filamented && destShader == DestShaderSelection.M_Standard)
                    MigrateFromFilamentedToStandard();
                else if (srcShader == SrcShaderSelection.Filamented && destShader == DestShaderSelection.M_Standard_Lite)
                    MigrateFromFilamentedToLite();
                else if (srcShader == SrcShaderSelection.Filamented && destShader == DestShaderSelection.M_Standard_Mobile)
                    MigrateFromFilamentedToMobile();
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            MGUI.Space8();
            MGUI.BoldLabel("Specularity Settings");
            MGUI.PropertyGroup(()=>{
                applyShadingModel = DrawToggleProperty(applyShadingModel, () => {
                    shadingModel = (SpecularityShadingModel)EditorGUILayout.EnumPopup("Shading Model", shadingModel);
                });
                applyReflToggle = DrawToggleProperty(applyReflToggle, () => {
                    reflToggle = (ToggleOffOn)EditorGUILayout.EnumPopup("Reflections", reflToggle);
                });
                applySpecToggle = DrawToggleProperty(applySpecToggle, () => {
                    specToggle = (ToggleOffOn)EditorGUILayout.EnumPopup("Specular Highlights", specToggle);
                });
            });

            MGUI.Space8();
            MGUI.BoldLabel("Lightmapping Settings");
            MGUI.PropertyGroup(()=>{
                applyDirMode = DrawToggleProperty(applyDirMode, () => {
                    dirMode = (BakeryMode)EditorGUILayout.EnumPopup("Directional Mode", dirMode);
                });
                applyBicubicSampling = DrawToggleProperty(applyBicubicSampling, () => {
                    bicubicSampling = (ToggleOffOn)EditorGUILayout.EnumPopup("Bicubic Sampling", bicubicSampling);
                });
                applyNonLinearSH = DrawToggleProperty(applyNonLinearSH, () => {
                    nonLinearSH = (ToggleOffOn)EditorGUILayout.EnumPopup("Non-Linear SH", nonLinearSH);
                });
                applyLightmapSpecular = DrawToggleProperty(applyLightmapSpecular, () => {
                    lightmapSpecular = (ToggleOffOn)EditorGUILayout.EnumPopup("Lightmap Specular", lightmapSpecular);
                });
                applyAdditiveLightVolumes = DrawToggleProperty(applyAdditiveLightVolumes, () => {
                    additiveLightVolumes = (ToggleOffOn)EditorGUILayout.EnumPopup("Additive Light Volumes", additiveLightVolumes);
                });
            });

            MGUI.Space8();
            MGUI.BoldLabel("Filtering Settings");
            MGUI.PropertyGroup(()=>{
                applyFilteringToggle = DrawToggleProperty(applyFilteringToggle, () => {
                    filteringToggle = (ToggleOffOn)EditorGUILayout.EnumPopup("Enable", filteringToggle);
                });
                applyHueMode = DrawToggleProperty(applyHueMode, () => {
                    hueMode = (HueMode)EditorGUILayout.EnumPopup("Hue Mode", hueMode);
                });
                applyFilteringHue = DrawToggleProperty(applyFilteringHue, () => {
                    filteringHue = EditorGUILayout.Slider("Hue", filteringHue, 0f, 1f);
                });
                applyFilteringSat = DrawToggleProperty(applyFilteringSat, () => {
                    filteringSat = EditorGUILayout.FloatField("Saturation", filteringSat);
                });
                applyFilteringBright = DrawToggleProperty(applyFilteringBright, () => {
                    filteringBright = EditorGUILayout.FloatField("Brightness", filteringBright);
                });
                applyFilteringCont = DrawToggleProperty(applyFilteringCont, () => {
                    filteringCont = EditorGUILayout.FloatField("Contrast", filteringCont);
                });
                applyFilteringACES = DrawToggleProperty(applyFilteringACES, () => {
                    filteringACES = EditorGUILayout.FloatField("ACES", filteringACES);
                });
            });

            MGUI.Space8();
            MGUI.BoldLabel("AreaLit Settings");
            MGUI.PropertyGroup(()=>{
                applyAreaLitToggle = DrawToggleProperty(applyAreaLitToggle, () => {
                    areaLitToggle = (ToggleOffOn)EditorGUILayout.EnumPopup("Enable", areaLitToggle);
                });
                applyAreaLitSpecularOcclusion = DrawToggleProperty(applyAreaLitSpecularOcclusion, () => {
                    areaLitSpecularOcclusion = (ToggleOffOn)EditorGUILayout.EnumPopup("Specular Occlusion", areaLitSpecularOcclusion);
                });
                applyAreaLitStrength = DrawToggleProperty(applyAreaLitStrength, () => {
                    areaLitStrength = EditorGUILayout.FloatField("Strength", areaLitStrength);
                });
                applyAreaLitRoughnessMultiplier = DrawToggleProperty(applyAreaLitRoughnessMultiplier, () => {
                    areaLitRoughnessMultiplier = EditorGUILayout.FloatField("Roughness Multiplier", areaLitRoughnessMultiplier);
                });
                applyLightMesh = DrawToggleProperty(applyLightMesh, () => {
                    lightMesh = DrawTextureField("Light Mesh", lightMesh, TextureDimension.Tex2D);
                });
                applyLightTex0 = DrawToggleProperty(applyLightTex0, () => {
                    lightTex0 = DrawTextureField("Light Texture 0", lightTex0, TextureDimension.Tex2D);
                });
                applyLightTex1 = DrawToggleProperty(applyLightTex1, () => {
                    lightTex1 = DrawTextureField("Light Texture 1", lightTex1, TextureDimension.Tex2D);
                });
                applyLightTex2 = DrawToggleProperty(applyLightTex2, () => {
                    lightTex2 = DrawTextureField("Light Texture 2", lightTex2, TextureDimension.Tex2D);
                });
                applyLightTex3 = DrawToggleProperty(applyLightTex3, () => {
                    lightTex3 = DrawTextureField("Light Texture 3", lightTex3, TextureDimension.Tex2DArray);
                });
                applyAreaLitOcclusion = DrawToggleProperty(applyAreaLitOcclusion, () => {
                    areaLitOcclusion = DrawTextureField("Occlusion", areaLitOcclusion, TextureDimension.Tex2D);
                });
                applyAreaLitOcclusionUVSet = DrawToggleProperty(applyAreaLitOcclusionUVSet, () => {
                    areaLitOcclusionUVSet = (AreaLitOcclusionUVSet)EditorGUILayout.EnumPopup("Occlusion UV Set", areaLitOcclusionUVSet);
                });
            });
            EditorGUILayout.EndScrollView();

            MGUI.Space4();
            Rect applyBtnRect = EditorGUILayout.GetControlRect(false, 36f);
            applyBtnRect.width = buttonWidth;
            if (GUI.Button(applyBtnRect, "Apply Changes")){
                ApplyAllSettings();
            }
            MGUI.Space6();
        }

        void ApplyAllSettings(){
            List<Material> materials = new List<Material>();
            materials.AddRange(standardMaterials);
            materials.AddRange(standardLiteMaterials);
            materials.AddRange(standardMobileMaterials);
            materials = materials.Where(m => m != null).Distinct().ToList();

            if (materials.Count > 0)
                Undo.RecordObjects(materials.ToArray(), "Apply Standard Shader Settings");

            foreach (Material m in materials){
                // Specular Settings
                if (applyShadingModel)
                    m.SetInt("_ShadingModel", (int)shadingModel);
                if (applyReflToggle){
                    m.SetInt("_ReflectionsToggle", (int)reflToggle);
                    MGUI.SetKeyword(m, "_REFLECTIONS_ON", (int)reflToggle == 1);
                }
                if (applySpecToggle){
                    m.SetInt("_SpecularHighlightsToggle", (int)specToggle);
                    MGUI.SetKeyword(m, "_SPECULAR_HIGHLIGHTS_ON", (int)specToggle == 1);
                }

                // Bakery Settings
                if (applyDirMode){
                    m.SetInt("_BakeryMode", (int)dirMode);
                    MGUI.SetKeyword(m, "BAKERY_SH", dirMode == BakeryMode.SH);
                    MGUI.SetKeyword(m, "BAKERY_RNM", dirMode == BakeryMode.RNM);
                    MGUI.SetKeyword(m, "BAKERY_MONOSH", dirMode == BakeryMode.MonoSH);
                }
                if (applyBicubicSampling){
                    m.SetInt("_BicubicSampling", (int)bicubicSampling);
                    MGUI.SetKeyword(m, "_BICUBIC_SAMPLING_ON", (int)bicubicSampling == 1);
                }
                if (applyNonLinearSH){
                    m.SetInt("_BAKERY_SHNONLINEAR", (int)nonLinearSH);
                    MGUI.SetKeyword(m, "BAKERY_SHNONLINEAR", (int)nonLinearSH == 1);
                }
                if (applyLightmapSpecular){
                    m.SetInt("_BAKERY_LMSPEC", (int)lightmapSpecular);
                    MGUI.SetKeyword(m, "BAKERY_LMSPEC", (int)lightmapSpecular == 1);
                }
                if (applyAdditiveLightVolumes){
                    m.SetInt("_AdditiveLightVolumesToggle", (int)additiveLightVolumes);
                }

                // Filtering Settings
                if (applyFilteringToggle)
                    m.SetInt("_Filtering", (int)filteringToggle);
                if (applyFilteringHue)
                    m.SetFloat("_HuePost", filteringHue);
                if (applyHueMode)
                    m.SetFloat("_HueMode", (int)hueMode);
                if (applyFilteringSat)
                    m.SetFloat("_SaturationPost", filteringSat);
                if (applyFilteringBright)
                    m.SetFloat("_BrightnessPost", filteringBright);
                if (applyFilteringCont)
                    m.SetFloat("_ContrastPost", filteringCont);
                if (applyFilteringACES)
                    m.SetFloat("_ACES", filteringACES);

                // AreaLit Settings
                if (applyAreaLitToggle){
                    m.SetInt("_AreaLitToggle", (int)areaLitToggle);
                    MGUI.SetKeyword(m, "_AREALIT_ON", (int)areaLitToggle == 1);
                }
                if (applyAreaLitSpecularOcclusion)
                    m.SetInt("_AreaLitSpecularOcclusion", (int)areaLitSpecularOcclusion);
                if (applyAreaLitStrength)
                    m.SetFloat("_AreaLitStrength", areaLitStrength);
                if (applyAreaLitRoughnessMultiplier)
                    m.SetFloat("_AreaLitRoughnessMultiplier", areaLitRoughnessMultiplier);
                if (applyLightMesh)
                    m.SetTexture("_LightMesh", lightMesh);
                if (applyLightTex0)
                    m.SetTexture("_LightTex0", lightTex0);
                if (applyLightTex1)
                    m.SetTexture("_LightTex1", lightTex1);
                if (applyLightTex2)
                    m.SetTexture("_LightTex2", lightTex2);
                if (applyLightTex3)
                    m.SetTexture("_LightTex3", lightTex3);
                if (applyAreaLitOcclusion)
                    m.SetTexture("_AreaLitOcclusion", areaLitOcclusion);
                if (applyAreaLitOcclusionUVSet)
                    m.SetInt("_AreaLitOcclusionUVSet", (int)areaLitOcclusionUVSet);
            }
        }
        
        void MigrateFromStandardToLite(){
            if (standardMaterials != null && standardMaterials.Count > 0){
                Undo.RecordObjects(standardMaterials.ToArray(), "Swap Shaders");
                foreach(Material m in standardMaterials){
                    m.shader = standardLiteShader;
                }
            }
            RefreshMaterials();
        }

        void MigrateFromStandardToMobile(){
            if (standardMaterials != null && standardMaterials.Count > 0){
                Undo.RecordObjects(standardMaterials.ToArray(), "Swap Shaders");
                foreach(Material m in standardMaterials){
                    m.shader = standardMobileShader;
                }
            }
            RefreshMaterials();
        }

        void MigrateFromLiteToStandard(){
            if (standardLiteMaterials != null && standardLiteMaterials.Count > 0){
                Undo.RecordObjects(standardLiteMaterials.ToArray(), "Swap Shaders");
                foreach(Material m in standardLiteMaterials){
                    m.shader = standardShader;
                }
            }
            RefreshMaterials();
        }

        void MigrateFromLiteToMobile(){
            if (standardLiteMaterials != null && standardLiteMaterials.Count > 0){
                Undo.RecordObjects(standardLiteMaterials.ToArray(), "Swap Shaders");
                foreach(Material m in standardLiteMaterials){
                    m.shader = standardMobileShader;
                }
            }
            RefreshMaterials();
        }

        void MigrateFromMobileToStandard(){
            if (standardMobileMaterials != null && standardMobileMaterials.Count > 0){
                Undo.RecordObjects(standardMobileMaterials.ToArray(), "Swap Shaders");
                foreach(Material m in standardMobileMaterials){
                    m.shader = standardShader;
                }
            }
            RefreshMaterials();
        }

        void MigrateFromMobileToLite(){
            if (standardMobileMaterials != null && standardMobileMaterials.Count > 0){
                Undo.RecordObjects(standardMobileMaterials.ToArray(), "Swap Shaders");
                foreach(Material m in standardMobileMaterials){
                    m.shader = standardLiteShader;
                }
            }
            RefreshMaterials();
        }

        void MigrateFromUnityStandardToStandard(){
            if (standardUnityMaterials != null && standardUnityMaterials.Count > 0){
                Undo.RecordObjects(standardUnityMaterials.ToArray(), "Swap Shaders");
                foreach (Material m in standardUnityMaterials){
                    m.shader = standardShader;
                }
            }
            RefreshMaterials();
        }

        void MigrateFromUnityStandardToLite(){
            if (standardUnityMaterials != null && standardUnityMaterials.Count > 0){
                Undo.RecordObjects(standardUnityMaterials.ToArray(), "Swap Shaders");
                foreach (Material m in standardUnityMaterials){
                    m.shader = standardLiteShader;
                }
            }
            RefreshMaterials();
        }

        void MigrateFromUnityStandardToMobile(){
            if (standardUnityMaterials != null && standardUnityMaterials.Count > 0){
                Undo.RecordObjects(standardUnityMaterials.ToArray(), "Swap Shaders");
                foreach (Material m in standardUnityMaterials){
                    m.shader = standardMobileShader;
                }
            }
            RefreshMaterials();
        }

        void MigrateFromFilamentedToStandard(){
            if (filamentedMaterials != null && filamentedMaterials.Count > 0){
                Undo.RecordObjects(filamentedMaterials.ToArray(), "Swap Shaders");
                foreach (Material m in filamentedMaterials){
                    m.shader = standardShader;
                }
            }
            RefreshMaterials();
        }

        void MigrateFromFilamentedToLite(){
            if (filamentedMaterials != null && filamentedMaterials.Count > 0){
                Undo.RecordObjects(filamentedMaterials.ToArray(), "Swap Shaders");
                foreach (Material m in filamentedMaterials){
                    m.shader = standardLiteShader;
                }
            }
            RefreshMaterials();
        }

        void MigrateFromFilamentedToMobile(){
            if (filamentedMaterials != null && filamentedMaterials.Count > 0){
                Undo.RecordObjects(filamentedMaterials.ToArray(), "Swap Shaders");
                foreach (Material m in filamentedMaterials){
                    m.shader = standardMobileShader;
                }
            }
            RefreshMaterials();
        }

        void ApplyDebugView(){
            List<Material> materials = new List<Material>();
            materials.AddRange(standardMaterials);
            materials.AddRange(standardLiteMaterials);
            materials.AddRange(standardMobileMaterials);
            if (materials.Count > 0)
                Undo.RecordObjects(materials.ToArray(), "Apply Debug View");
            foreach (Material m in materials){
                HandleDebugView(m);
            }
        }

        void RestoreDefaultTextures(){
            List<Material> materials = new List<Material>();
            materials.AddRange(standardMaterials);
            materials.AddRange(standardLiteMaterials);
            materials.AddRange(standardMobileMaterials);
            if (materials.Count > 0)
                Undo.RecordObjects(materials.ToArray(), "Restore Default Textures");
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
            filamentedMaterials.Clear();
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
                    else if (shaderName == "Silent/Filamented"){
                        filamentedMaterials.Add(m);
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
