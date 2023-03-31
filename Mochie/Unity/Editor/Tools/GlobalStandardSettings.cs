using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.SceneManagement;
using System.IO;

namespace Mochie {

    public class GlobalStandardSettings : EditorWindow {

        bool applyToScene = true;
        bool inactive = true;

        Shader standardShader;

        List<Material> projectMaterials = new List<Material>();
        List<Material> sceneMaterials = new List<Material>();
        List<Material> standardMaterials = new List<Material>();
        List<Material> standardLiteMaterials = new List<Material>();
        List<Material> standardUnityMaterials = new List<Material>();

        // Bakery Settings
        enum BakeryMode {None, SH, RNM, MonoSH}
        BakeryMode dirMode;
        bool bicubicSampling = true;
        bool nonLinearSH = false;
        bool lightmapSpecular = false;

        // Workflow settings
        enum Workflow {Standard, Packed}
        enum SampleMode {Default, Stochastic, Supersampled, Triplanar}
        enum ColorChannel {Red, Green, Blue, Alpha}
        enum ToggleOnOff {Off, On}
        Workflow workflowMode;
        SampleMode sampleMode;
        ColorChannel metallicChannel = ColorChannel.Blue;
        ColorChannel roughnessChannel = ColorChannel.Green;
        ColorChannel occlusionChannel = ColorChannel.Red;
        ColorChannel heightChannel = ColorChannel.Alpha;
        ToggleOnOff smoothnessToggle;

        [MenuItem("Mochie/Global Standard Settings")]
        static void Init(){
            GlobalStandardSettings window = (GlobalStandardSettings)EditorWindow.GetWindow(typeof(GlobalStandardSettings));
            window.titleContent = new GUIContent("Standard Shader Settings");
            window.minSize = new Vector2(300, 490);
            window.maxSize = new Vector2(300, 490);
            window.Show();
        }

        void Awake(){
            standardShader = Shader.Find("Mochie/Standard");
            RefreshMaterials();
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

            // MGUI.DisplayWarning("Please note that changes made with this utility cannot be automatically undone, pick your settings carefully!");
            
            string lol = applyToScene ? "material slots in scene" : "materials in project";
            MGUI.DisplayText("Found " + standardMaterials.Count + " Mochie Standard " + lol + "\nFound " + standardLiteMaterials.Count + " Mochie Standard Lite " + lol + "\nFound " + standardUnityMaterials.Count + " Unity Standard " + lol);

            if (MGUI.SimpleButton("Refresh Materials List", buttonWidth, 0f)){
                RefreshMaterials();
            }

            if (MGUI.SimpleButton("Migrate From Mochie Standard Lite", buttonWidth, 0f)){
                MigrateFromLite();
            }

            if (MGUI.SimpleButton("Migrate From Unity Standard", buttonWidth, 0f)){
                MigrateFromUnityStandard();
            }

            MGUI.Space8();
            MGUI.BoldLabel("Workflow Settings");
            MGUI.PropertyGroup(()=>{
                workflowMode = (Workflow)EditorGUILayout.EnumPopup("Workflow", workflowMode);
                sampleMode = (SampleMode)EditorGUILayout.EnumPopup("Sample Mode", sampleMode);
                smoothnessToggle = (ToggleOnOff)EditorGUILayout.EnumPopup("Smoothness", smoothnessToggle);
                MGUI.ToggleGroup(workflowMode != Workflow.Packed);
                    MGUI.PropertyGroup(()=>{
                        metallicChannel = (ColorChannel)EditorGUILayout.EnumPopup("Metallic Channel", metallicChannel);
                        roughnessChannel = (ColorChannel)EditorGUILayout.EnumPopup("Roughness Channel", roughnessChannel);
                        occlusionChannel = (ColorChannel)EditorGUILayout.EnumPopup("Occlusion Channel", occlusionChannel);
                        heightChannel = (ColorChannel)EditorGUILayout.EnumPopup("Height Channel", heightChannel);
                    });
                MGUI.ToggleGroupEnd();
            });
            if (MGUI.SimpleButton("Apply", buttonWidth, 0f)){
                ApplyWorkflowSettings();
            }

            MGUI.Space8();
            MGUI.BoldLabel("Bakery Settings");
            MGUI.PropertyGroup(()=>{
                dirMode = (BakeryMode)EditorGUILayout.EnumPopup("Directional Mode", dirMode);
                bicubicSampling = EditorGUILayout.Toggle("Bicubic Lightmapping", bicubicSampling);
                nonLinearSH = EditorGUILayout.Toggle("Non-Linear SH", nonLinearSH);
                lightmapSpecular = EditorGUILayout.Toggle("Lightmap Specular", lightmapSpecular);
            });
            if (MGUI.SimpleButton("Apply", buttonWidth, 0f)){
                ApplyBakerySettings();
            }
        }
        
        void MigrateFromLite(){
            if (standardLiteMaterials != null){
                foreach(Material m in standardLiteMaterials){
                    m.shader = standardShader;
                }
            }
            RefreshMaterials();
        }

        void MigrateFromUnityStandard(){
            if (standardUnityMaterials != null){
                foreach (Material m in standardUnityMaterials){
                    m.shader = standardShader;
                }
            }
            RefreshMaterials();
        }

        void ApplyBakerySettings(){
            foreach (Material m in standardMaterials){

                // Directional Mode
                m.SetInt("_BakeryMode", (int)dirMode);
                MGUI.SetKeyword(m, "BAKERY_SH", dirMode == BakeryMode.SH);
                MGUI.SetKeyword(m, "BAKERY_RNM", dirMode == BakeryMode.RNM);
                MGUI.SetKeyword(m, "BAKERY_MONOSH", dirMode == BakeryMode.MonoSH);

                // Bicubic Lightmapping
                m.SetInt("_BicubicLightmap", bicubicSampling ? 1 : 0);
                MGUI.SetKeyword(m, "_BICUBIC_SAMPLING_ON", bicubicSampling);

                // Nonlinear SH
                m.SetInt("_BAKERY_SHNONLINEAR", nonLinearSH ? 1 : 0);
                MGUI.SetKeyword(m, "BAKERY_SHNONLINEAR", nonLinearSH);

                // Lightmapped Specular
                m.SetInt("_BAKERY_LMSPEC", lightmapSpecular ? 1 : 0);
                MGUI.SetKeyword(m, "BAKERY_LMSPEC", lightmapSpecular);
            }
        }

        void ApplyWorkflowSettings(){
            foreach (Material m in standardMaterials){

                // Workflow
                m.SetInt("_Workflow", (int)workflowMode);
                MGUI.SetKeyword(m, "_WORKFLOW_PACKED_ON", workflowMode == Workflow.Packed);

                // Sample Mode
                m.SetInt("_SamplingMode", (int)sampleMode);
                MGUI.SetKeyword(m, "_STOCHASTIC_ON", sampleMode == SampleMode.Stochastic);
                MGUI.SetKeyword(m, "_TSS_ON", sampleMode == SampleMode.Supersampled);
                MGUI.SetKeyword(m, "_TRIPLANAR_ON", sampleMode == SampleMode.Triplanar);

                // Smoothness Toggle
                m.SetInt("_UseSmoothness", (int)smoothnessToggle);

                // Channel Settings
                if (workflowMode == Workflow.Packed){
                    m.SetInt("_RoughnessChannel", (int)roughnessChannel);
                    m.SetInt("_MetallicChannel", (int)metallicChannel);
                    m.SetInt("_OcclusionChannel", (int)occlusionChannel);
                    m.SetInt("_HeightChannel", (int)heightChannel);

                    // Since packed texture is a separate slot, move an existing PBR texture into it
                    if (m.GetTexture("_SpecGlossMap") != null)
                        m.SetTexture("_PackedMap", m.GetTexture("_SpecGlossMap"));
                    else if (m.GetTexture("_MetallicGlossMap") != null)
                        m.SetTexture("_PackedMap", m.GetTexture("_MetallicGlossMap"));
                    else if (m.GetTexture("_OcclusionMap") != null)
                        m.SetTexture("_PackedMap", m.GetTexture("_OcclusionMap"));
                    else if (m.GetTexture("_ParallaxMap") != null)
                        m.SetTexture("_PackedMap", m.GetTexture("_ParallaxMap"));
                }       
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
                    else if (shaderName == "Mochie/Standard (Lite)"){
                        standardLiteMaterials.Add(m);
                    }
                    else if (shaderName == "Hidden/InternalErrorShader"){
                        if (File.ReadAllText(AssetDatabase.GetAssetPath(m)).Contains("610b05107fb18e34a8bb23f82f253b50")){
                            standardLiteMaterials.Add(m);
                        }
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