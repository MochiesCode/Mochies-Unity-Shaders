#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using VRC.SDKBase.Editor.BuildPipeline;

namespace Mochie {
    /// <summary>
    /// Strips unused textures on the copy of an avatar that the VRChat SDK builds.
    /// The SDK saves that copy as a prefab before bundling it, and a prefab can't reference in-memory materials,
    /// so the stripped copies are saved as temporary assets and deleted once the build is done.
    /// </summary>
    public class BuildTextureStripperAvatarHook : IVRCSDKPreprocessAvatarCallback, IVRCSDKPostprocessAvatarCallback {
        const string TempFolderParent = "Assets";
        const string TempFolderName = "_MochieTextureStripperTemp";
        const string TempFolder = TempFolderParent + "/" + TempFolderName;

        // Runs after VRCFury (-10000) and Modular Avatar / NDMF (-11000), so the animator controllers they merge
        // into the avatar are included in the animation scan.
        public int callbackOrder => -100;

        // Only clean up after builds this hook actually touched, so it does nothing at all while disabled
        static bool createdTempAssets;

        public bool OnPreprocessAvatar(GameObject avatarGameObject) {
            if (!BuildTextureStripperSettings.instance.StripOnBuild) {
                return true;
            }

            // Leftovers from a build that failed before OnPostprocessAvatar
            DeleteTempAssets();

            try {
                TextureStripperReport report = BuildTextureStripper.StripUnusedTextures(new[] { avatarGameObject }, $"avatar '{avatarGameObject.name}'");
                // Play mode tools (e.g. Av3Emulator) can also run avatar preprocessing; nothing gets saved as a prefab there.
                if (!EditorApplication.isPlaying) {
                    SaveTempAssets(report.CreatedClones);
                }
                return true;
            }
            catch (Exception ex) {
                // The avatar may already reference unsaved copies, which would upload as missing materials
                Debug.LogError($"[Mochie Texture Stripper] Error while processing avatar '{avatarGameObject.name}', cancelling the build. Disable \"Strip Unused Textures on Build\" in Tools > Mochie > Build Texture Stripper to build without it.\n{ex}");
                return false;
            }
        }

        public void OnPostprocessAvatar() {
            if (createdTempAssets) {
                DeleteTempAssets();
            }
        }

        static void SaveTempAssets(List<Material> clones) {
            if (clones.Count == 0) return;
            createdTempAssets = true;
            if (!AssetDatabase.IsValidFolder(TempFolder)) {
                AssetDatabase.CreateFolder(TempFolderParent, TempFolderName);
            }
            for (int i = 0; i < clones.Count; i++) {
                AssetDatabase.CreateAsset(clones[i], $"{TempFolder}/{i}_{MakeSafeFileName(clones[i].name)}.mat");
            }
        }

        static void DeleteTempAssets() {
            if (AssetDatabase.IsValidFolder(TempFolder)) {
                AssetDatabase.DeleteAsset(TempFolder);
            }
            createdTempAssets = false;
        }

        static string MakeSafeFileName(string name) {
            foreach (char c in Path.GetInvalidFileNameChars()) {
                name = name.Replace(c, '_');
            }
            return name;
        }
    }
}
#endif
