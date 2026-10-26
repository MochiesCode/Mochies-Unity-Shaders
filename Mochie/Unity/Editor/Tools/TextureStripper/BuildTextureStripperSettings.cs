using UnityEngine;
using UnityEditor;

namespace Mochie {
    /// <summary>
    /// Project-wide settings for the Mochie Build Pipeline Texture Stripper.
    /// Stored in ProjectSettings so every machine and teammate building the project gets the same result.
    /// </summary>
    [FilePath("ProjectSettings/MochieTextureStripper.asset", FilePathAttribute.Location.ProjectFolder)]
    public class BuildTextureStripperSettings : ScriptableSingleton<BuildTextureStripperSettings> {
        [SerializeField] bool stripOnBuild = false;
        [SerializeField] bool checkAnimations = true;
        [SerializeField] bool logToConsole = true;
        [SerializeField] bool stripZeroValueFeatures = false;

        /// <summary>Strip unused textures from build copies of Mochie materials. Off by default so it's always opt-in.</summary>
        public bool StripOnBuild {
            get => stripOnBuild;
            set => SetValue(ref stripOnBuild, value);
        }

        /// <summary>Protect properties animated by AnimationClips (Animators, Timeline, avatar playable layers, etc).</summary>
        public bool CheckAnimations {
            get => checkAnimations;
            set => SetValue(ref checkAnimations, value);
        }

        public bool LogToConsole {
            get => logToConsole;
            set => SetValue(ref logToConsole, value);
        }

        /// <summary>
        /// Also strip textures whose feature is only disabled by a zero strength or black colour.
        /// Off by default because scripts (e.g. Udon) and MaterialPropertyBlocks can change those values at runtime.
        /// </summary>
        public bool StripZeroValueFeatures {
            get => stripZeroValueFeatures;
            set => SetValue(ref stripZeroValueFeatures, value);
        }

        void SetValue(ref bool field, bool value) {
            if (field == value) return;
            field = value;
            Save(true);
        }
    }
}
