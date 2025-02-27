using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    public class CopyFloatPropertyValueAction : CopyPropertyValueActionBase
    {
        public CopyFloatPropertyValueAction(string sourcePropertyName, string targetPropertyName) : base(sourcePropertyName, targetPropertyName, SerializedMaterialPropertyType.Float) {}

        public override void RunAction(MaterialContext materialContext)
        {
            if(materialContext.TryGetFloat(SourcePropertyName, out float floatValue))
            {
                materialContext.Material.SetFloat(TargetPropertyName, floatValue);
            }
            else
            {
                #if MOCHIE_DEV
                Debug.LogWarning($"Couldn't find <b>Float</b> property with name <b>{SourcePropertyName}</b> in Material {materialContext.Material?.name} when running action {GetType().Name} ({SourcePropertyName} -> {TargetPropertyName})");
                #endif
            }
        }
    }
}