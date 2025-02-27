namespace Mochie.ShaderUpgrader
{
    /// <summary>
    /// Material property type supported in serialized objects
    /// </summary>
    public enum SerializedMaterialPropertyType
    {
        Float,
        /// <summary>
        ///   <para>An Integer Property.</para>
        /// </summary>
        Int,
        /// <summary>
        ///   <para>A Vector Property.</para>
        /// </summary>
        Vector,
        /// <summary>
        ///   <para>A Texture Property.</para>
        /// </summary>
        Texture
    }
}