namespace Mochie.ShaderUpgrader
{
    public abstract class CopyPropertyValueActionBase : PropertyActionBase
    {
        protected CopyPropertyValueActionBase(string sourcePropertyName, string targetPropertyName, SerializedMaterialPropertyType propertyType) : base(sourcePropertyName, targetPropertyName, propertyType) {}
    }
}