using System.IO;
using UnityEngine;
using UnityEditor;

namespace Mochie {

    public class MaterialCreator {

        private static string assetPath;

        [MenuItem("Assets/Create/Material (Mochie Standard Mobile)", priority = 301)]
        public static void CreateMochieStandardMobileMaterial(){
            Shader s = Shader.Find("Mochie/Standard Mobile");
            CreateMaterialWithShader(s);
        }

        [MenuItem("Assets/Create/Material (Mochie Standard Lite)", priority = 301)]
        public static void CreateMochieStandardLiteMaterial(){
            Shader s = Shader.Find("Mochie/Standard Lite");
            CreateMaterialWithShader(s);
        }

        [MenuItem("Assets/Create/Material (Mochie Standard)", priority = 301)]
        public static void CreateMochieStandardMaterial(){
            Shader s = Shader.Find("Mochie/Standard");
            CreateMaterialWithShader(s);
        }

        private static void CreateMaterialWithShader(Shader s){
            if (s != null){
                Material mat = new Material(s);
                string projFolderPath = MGUI.GetCurrentProjectFolderPath();
                string savePath = projFolderPath + "/New Material";
                int fileNumber = 1;
                while (File.Exists(savePath + ".mat")){
                    savePath = projFolderPath + "/New Material " + fileNumber;
                    fileNumber++;
                }
                savePath += ".mat";
                AssetDatabase.CreateAsset(mat, savePath);
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(savePath);
                assetPath = savePath;
                EditorApplication.delayCall += RenameAsset;
            }
            else {
                Debug.Log("Shader not found!");
            }
        }

        private static void RenameAsset() {
            Selection.activeObject = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
            EditorApplication.delayCall += ()=>{
                #if UNITY_EDITOR_WIN
                    EditorWindow.focusedWindow.SendEvent(new Event { keyCode = KeyCode.F2, type = EventType.KeyDown });
                #endif
                #if UNITY_EDITOR_OSX
                    EditorWindow.focusedWindow.SendEvent(new Event { keyCode = KeyCode.Return, type = EventType.KeyDown });
                #endif
                assetPath = null;
            };
        }
    }
}