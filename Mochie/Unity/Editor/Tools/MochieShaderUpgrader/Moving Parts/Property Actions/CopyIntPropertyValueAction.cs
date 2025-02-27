using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    public class CopyIntPropertyValueAction : CopyPropertyValueActionBase
    {
        public CopyIntPropertyValueAction(string sourcePropertyName, string targetPropertyName) : base(sourcePropertyName, targetPropertyName, SerializedMaterialPropertyType.Int) {}


        public override void RunAction(MaterialContext materialContext)
        {
            if(materialContext.TryGetInt(SourcePropertyName, out int intValue))
            {
                materialContext.Material.SetInt(TargetPropertyName, intValue);
            }
            else
            {
                #if MOCHIE_DEV
                Debug.LogWarning($"Couldn't find <b>Int</b> property with name <b>{SourcePropertyName}</b> in Material {materialContext.Material?.name} when running action {GetType().Name} ({SourcePropertyName} -> {TargetPropertyName})");
                #endif
            }
        }
    }
}