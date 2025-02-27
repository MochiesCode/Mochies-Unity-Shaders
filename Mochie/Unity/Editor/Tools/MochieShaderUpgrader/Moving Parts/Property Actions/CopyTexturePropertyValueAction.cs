using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    public class CopyTexturePropertyValueAction : CopyPropertyValueActionBase
    {
        public bool CopyTexture { get; private set; }
        public bool CopyTextureOffset { get; private set; }
        public bool CopyTextureScale { get; private set; }

        public CopyTexturePropertyValueAction(string sourcePropertyName, string targetPropertyName, bool copyTexture = true, bool copyTextureOffset = true,
            bool copyTextureScale = true) : base(sourcePropertyName, targetPropertyName, SerializedMaterialPropertyType.Texture)
        {
            CopyTexture = copyTexture;
            CopyTextureOffset = copyTextureOffset;
            CopyTextureScale = copyTextureScale;
        }

        public override void RunAction(MaterialContext materialContext)
        {
            if(materialContext.TryGetTexture(SourcePropertyName, out TextureContainer textureContainerValue))
            {
                if(CopyTexture)
                    materialContext.Material.SetTexture(TargetPropertyName, textureContainerValue.texture);
                if(CopyTextureOffset)
                    materialContext.Material.SetTextureOffset(TargetPropertyName, textureContainerValue.offset);
                if(CopyTextureScale)
                    materialContext.Material.SetTextureScale(TargetPropertyName, textureContainerValue.scale);
            }
            else
            {
                #if MOCHIE_DEV
                Debug.LogWarning($"Couldn't find <b>Texture</b> property with name <b>{SourcePropertyName}</b> in Material {materialContext.Material?.name} when running action {GetType().Name} ({SourcePropertyName} -> {TargetPropertyName})");
                #endif
            }
        }
    }
}