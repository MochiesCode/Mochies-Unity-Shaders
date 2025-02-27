using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    /// <summary>
    /// Texture value container, used for TexEnv serialized properties
    /// </summary>
    public struct TextureContainer
    {
        public Texture texture;
        public Vector2 scale;
        public Vector2 offset;
    }
}