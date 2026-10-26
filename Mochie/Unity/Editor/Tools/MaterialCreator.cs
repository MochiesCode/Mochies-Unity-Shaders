using UnityEngine;
using UnityEditor;

namespace Mochie {

    public class MaterialCreator {

        [MenuItem("Assets/Create/Material (Mochie Standard Mobile)", priority = 301)]
        public static void CreateMochieStandardMobileMaterial(){
            CreateMaterialWithShader("Mochie/Standard Mobile");
        }

        [MenuItem("Assets/Create/Material (Mochie Standard Lite)", priority = 301)]
        public static void CreateMochieStandardLiteMaterial(){
            CreateMaterialWithShader("Mochie/Standard Lite");
        }

        [MenuItem("Assets/Create/Material (Mochie Standard)", priority = 301)]
        public static void CreateMochieStandardMaterial(){
            CreateMaterialWithShader("Mochie/Standard");
        }

        // Same path Unity's own Create > Material takes: unique name in the active folder,
        // inline rename in the Project window, and nothing is written to disk if the rename is cancelled
        private static void CreateMaterialWithShader(string shaderName){
            Shader s = Shader.Find(shaderName);
            if (s != null){
                ProjectWindowUtil.CreateAsset(new Material(s), "New Material.mat");
            }
            else {
                Debug.LogError("Shader not found: " + shaderName);
            }
        }
    }
}
