namespace Mochie.ShaderUpgrader
{
    public interface IPropertyAction
    {
        public string SourcePropertyName { get; }
        public string TargetPropertyName { get; }
        public SerializedMaterialPropertyType PropertyType { get; }
    }
}