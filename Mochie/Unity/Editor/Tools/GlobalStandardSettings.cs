using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.SceneManagement;
using System.IO;

namespace Mochie {

    public class GlobalStandardSettings : EditorWindow {

        enum HueMode {HSV, Oklab}
        enum ToggleOnOff {Off, On}

        bool applyToScene = true;
        bool inactive = true;

        Shader standardShader;
        Shader standardLiteShader;
        Shader standardMobileShader;

        List<Material> projectMaterials = new List<Material>();
        List<Material> sceneMaterials = new List<Material>();
        List<Material> standardMaterials = new List<Material>();
        List<Material> standardLiteMaterials = new List<Material>();
        List<Material> standardMobileMaterials = new List<Material>();
        List<Material> standardUnityMaterials = new List<Material>();

        // Bakery settings
        enum BakeryMode {None, SH, RNM, MonoSH}
        BakeryMode dirMode;
        bool bicubicSampling = false;
        bool nonLinearSH = false;
        bool lightmapSpecular = false;

        // Workflow settings
        // enum Workflow {Standard, Packed}
        // enum SampleMode {Default, Stochastic, Supersampled, Triplanar}
        // enum ColorChannel {Red, Green, Blue, Alpha}

        // Workflow workflowMode;
        // SampleMode sampleMode;
        // ColorChannel metallicChannel = ColorChannel.Blue;
        // ColorChannel roughnessChannel = ColorChannel.Green;
        // ColorChannel occlusionChannel = ColorChannel.Red;
        // ColorChannel heightChannel = ColorChannel.Alpha;
        // ToggleOnOff smoothnessToggle;

        // Filtering settings
        HueMode hueMode = HueMode.HSV;
        bool filteringToggle;
        float filteringHue = 0f;
        float filteringSat = 1f;
        float filteringBright = 1f;
        float filteringCont = 1f;
        float filteringACES = 0f;

        // Specularity Settings
        enum SpecularityShadingModel {Unity_Standard, Google_Filament}
        SpecularityShadingModel shadingModel;
        bool reflToggle = true;
        bool specToggle = true;

        [MenuItem("Tools/Mochie/Global Standard Settings")]
        static void Init(){
            GlobalStandardSettings window = (GlobalStandardSettings)EditorWindow.GetWindow(typeof(GlobalStandardSettings));
            window.titleContent = new GUIContent("Standard Shader Settings");
            window.minSize = new Vector2(300, 895);
            window.maxSize = new Vector2(300, 895);
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
            float groupButtonWidth = MGUI.GetInspectorWidth()-14f;
            
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

            if (MGUI.SimpleButton("Refresh Materials List", groupButtonWidth, 0f)){
                RefreshMaterials();
            }

            if (MGUI.SimpleButton("Restore Default Textures", groupButtonWidth, 0f)){
                RestoreDefaultTextures();
            }

            MGUI.Space8();
            MGUI.BoldLabel("Shader Swapper");
            MGUI.PropertyGroup(()=>{
                if (MGUI.SimpleButton("Standard > Standard Lite", groupButtonWidth, 0f)){
                    MigrateFromStandardToLite();
                }
                if (MGUI.SimpleButton("Standard > Standard Mobile", groupButtonWidth, 0f)){
                    MigrateFromStandardToMobile();
                }
            });
            MGUI.PropertyGroup(()=>{
                if (MGUI.SimpleButton("Standard Lite > Standard", groupButtonWidth, 0f)){
                    MigrateFromLiteToStandard();
                }
                if (MGUI.SimpleButton("Standard Lite > Standard Mobile", groupButtonWidth, 0f)){
                    MigrateFromLiteToMobile();
                }
            });
            MGUI.PropertyGroup(()=>{
                if (MGUI.SimpleButton("Standard Mobile > Standard", groupButtonWidth, 0f)){
                    MigrateFromMobileToStandard();
                }
                if (MGUI.SimpleButton("Standard Mobile > Standard Lite", groupButtonWidth, 0f)){
                    MigrateFromMobileToLite();
                }
            });
            MGUI.PropertyGroup(()=>{
                if (MGUI.SimpleButton("Unity Standard > Standard", groupButtonWidth, 0f)){
                    MigrateFromUnityStandardToStandard();
                }
                if (MGUI.SimpleButton("Unity Standard > Standard Lite", groupButtonWidth, 0f)){
                    MigrateFromUnityStandardToLite();
                }
                if (MGUI.SimpleButton("Unity Standard > Standard Mobile", groupButtonWidth, 0f)){
                    MigrateFromUnityStandardToMobile();
                }
            });

            // Removing this cuz honestly idk when you'd ever wanna apply settings like this on a project/scene level
            // MGUI.Space8();
            // MGUI.BoldLabel("Workflow Settings");
            // MGUI.PropertyGroup(()=>{
            //     workflowMode = (Workflow)EditorGUILayout.EnumPopup("Workflow", workflowMode);
            //     sampleMode = (SampleMode)EditorGUILayout.EnumPopup("Sample Mode", sampleMode);
            //     smoothnessToggle = (ToggleOnOff)EditorGUILayout.EnumPopup("Smoothness", smoothnessToggle);
            //     MGUI.ToggleGroup(workflowMode != Workflow.Packed);
            //         MGUI.PropertyGroup(()=>{
            //             metallicChannel = (ColorChannel)EditorGUILayout.EnumPopup("Metallic Channel", metallicChannel);
            //             roughnessChannel = (ColorChannel)EditorGUILayout.EnumPopup("Roughness Channel", roughnessChannel);
            //             occlusionChannel = (ColorChannel)EditorGUILayout.EnumPopup("Occlusion Channel", occlusionChannel);
            //             heightChannel = (ColorChannel)EditorGUILayout.EnumPopup("Height Channel", heightChannel);
            //         });
            //     MGUI.ToggleGroupEnd();
            // });
            // if (MGUI.SimpleButton("Apply", buttonWidth, 0f)){
            //     ApplyWorkflowSettings();
            // }

            MGUI.Space8();
            MGUI.BoldLabel("Specularity Settings");
            MGUI.PropertyGroup(()=>{
                shadingModel = (SpecularityShadingModel)EditorGUILayout.EnumPopup("Shading Model", shadingModel);
                reflToggle = EditorGUILayout.Toggle("Reflections", reflToggle);
                specToggle = EditorGUILayout.Toggle("Specular Highlights", specToggle);
            });
            if (MGUI.SimpleButton("Apply", buttonWidth, 0f)){
                ApplySpecSettings();
            }

            MGUI.Space8();
            MGUI.BoldLabel("Lightmapping Settings");
            MGUI.PropertyGroup(()=>{
                dirMode = (BakeryMode)EditorGUILayout.EnumPopup("Directional Mode", dirMode);
                bicubicSampling = EditorGUILayout.Toggle("Bicubic Sampling", bicubicSampling);
                nonLinearSH = EditorGUILayout.Toggle("Non-Linear SH", nonLinearSH);
                lightmapSpecular = EditorGUILayout.Toggle("Lightmap Specular", lightmapSpecular);
            });
            if (MGUI.SimpleButton("Apply", buttonWidth, 0f)){
                ApplyBakerySettings();
            }

            MGUI.Space8();
            MGUI.BoldLabel("Filtering Settings");
            MGUI.PropertyGroup(()=>{
                filteringToggle = EditorGUILayout.Toggle("Enable", filteringToggle);
                MGUI.ToggleGroup(!filteringToggle);
                filteringHue = EditorGUILayout.Slider("Hue", filteringHue, 0f, 1f);
                hueMode = (HueMode)EditorGUILayout.EnumPopup("Hue Mode", hueMode);
                filteringSat = EditorGUILayout.FloatField("Saturation", filteringSat);
                filteringBright = EditorGUILayout.FloatField("Brightness", filteringBright);
                filteringCont = EditorGUILayout.FloatField("Contrast", filteringCont);
                filteringACES = EditorGUILayout.FloatField("ACES", filteringACES);
                MGUI.ToggleGroupEnd();
            });
            if (MGUI.SimpleButton("Apply", buttonWidth, 0f)){
                ApplyFilterSettings();
            }
            MGUI.Space8();
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
                m.SetInt("_BakeryMode", (int)dirMode);
                MGUI.SetKeyword(m, "BAKERY_SH", dirMode == BakeryMode.SH);
                MGUI.SetKeyword(m, "BAKERY_RNM", dirMode == BakeryMode.RNM);
                MGUI.SetKeyword(m, "BAKERY_MONOSH", dirMode == BakeryMode.MonoSH);

                // Bicubic Lightmapping
                m.SetInt("_BicubicSampling", bicubicSampling ? 1 : 0);
                MGUI.SetKeyword(m, "_BICUBIC_SAMPLING_ON", bicubicSampling);

                // Nonlinear SH
                m.SetInt("_BAKERY_SHNONLINEAR", nonLinearSH ? 1 : 0);
                MGUI.SetKeyword(m, "BAKERY_SHNONLINEAR", nonLinearSH);

                // Lightmapped Specular
                m.SetInt("_BAKERY_LMSPEC", lightmapSpecular ? 1 : 0);
                MGUI.SetKeyword(m, "BAKERY_LMSPEC", lightmapSpecular);
            }
        }

        // void ApplyWorkflowSettings(){
        //     List<Material> materials = new List<Material>();
        //     materials.AddRange(standardMaterials);
        //     materials.AddRange(standardLiteMaterials);
        //     materials.AddRange(standardMobileMaterials);
        //     foreach (Material m in materials){

        //         // Workflow
        //         m.SetInt("_PrimaryWorkflow", (int)workflowMode);
        //         MGUI.SetKeyword(m, "_WORKFLOW_PACKED_ON", workflowMode == Workflow.Packed);

        //         // Sample Mode
        //         m.SetInt("_PrimarySampleMode", (int)sampleMode);
        //         MGUI.SetKeyword(m, "_STOCHASTIC_ON", sampleMode == SampleMode.Stochastic);
        //         MGUI.SetKeyword(m, "_SUPERSAMPLING_ON", sampleMode == SampleMode.Supersampled);
        //         MGUI.SetKeyword(m, "_TRIPLANAR_ON", sampleMode == SampleMode.Triplanar);

        //         // Smoothness Toggle
        //         m.SetInt("_SmoothnessToggle", (int)smoothnessToggle);

        //         // Channel Settings
        //         if (workflowMode == Workflow.Packed){
        //             m.SetInt("_RoughnessChannel", (int)roughnessChannel);
        //             m.SetInt("_MetallicChannel", (int)metallicChannel);
        //             m.SetInt("_OcclusionChannel", (int)occlusionChannel);
        //             m.SetInt("_HeightChannel", (int)heightChannel);

        //             // Since packed texture is a separate slot, move an existing PBR texture into it when there is no existing packed map
        //             if (m.GetTexture("_PackedMap") == null){
        //                 if (m.GetTexture("_RoughnessMap") != null)
        //                     m.SetTexture("_PackedMap", m.GetTexture("_RoughnessMap"));
        //                 else if (m.GetTexture("_MetallicMap") != null)
        //                     m.SetTexture("_PackedMap", m.GetTexture("_MetallicMap"));
        //                 else if (m.GetTexture("_OcclusionMap") != null)
        //                     m.SetTexture("_PackedMap", m.GetTexture("_OcclusionMap"));
        //                 else if (m.GetTexture("_HeightMap") != null)
        //                     m.SetTexture("_PackedMap", m.GetTexture("_HeightMap"));
        //             }
        //         }       
        //     }
        // }

        void ApplyFilterSettings(){
            List<Material> materials = new List<Material>();
            materials.AddRange(standardMaterials);
            materials.AddRange(standardLiteMaterials);
            foreach (Material m in materials){
                m.SetInt("_Filtering", filteringToggle ? 1 : 0);
                if (filteringToggle){
                    m.SetFloat("_HuePost", filteringHue);
                    m.SetFloat("_HueMode", (int)hueMode);
                    m.SetFloat("_SaturationPost", filteringSat);
                    m.SetFloat("_BrightnessPost", filteringBright);
                    m.SetFloat("_ContrastPost", filteringCont);
                    m.SetFloat("_ACES", filteringACES);
                }
            }
        }

        void ApplySpecSettings(){
            List<Material> materials = new List<Material>();
            materials.AddRange(standardMaterials);
            materials.AddRange(standardLiteMaterials);
            materials.AddRange(standardMobileMaterials);
            foreach (Material m in materials){
                MGUI.SetKeyword(m, "_REFLECTIONS_ON", reflToggle);
                MGUI.SetKeyword(m, "_SPECULARHIGHLIGHTS_ON", specToggle);
                m.SetInt("_ReflectionsToggle", reflToggle ? 1 : 0);
                m.SetInt("_SpecularHighlightsToggle", specToggle ? 1 : 0);
                m.SetInt("_ShadingModel", (int)shadingModel);
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
            Texture defaultTex = AssetDatabase.LoadAssetAtPath(texFolder + "Default White Swatch.png", typeof(Texture)) as Texture;
            Texture dropletMaskTex = AssetDatabase.LoadAssetAtPath(texFolder + "Droplet Mask.tif", typeof(Texture)) as Texture;
            Texture ssrNoiseTex = AssetDatabase.LoadAssetAtPath(texFolder + "SSR Noise.png", typeof(Texture)) as Texture;
            foreach (Material m in materials){
                m.SetTexture("_DefaultSampler", defaultTex);
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
    }
}