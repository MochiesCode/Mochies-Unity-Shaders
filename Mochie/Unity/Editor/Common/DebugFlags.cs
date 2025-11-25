namespace Mochie
{
    // even though this lacks the Flags attribute, the shader
    // still expects a bitmask
    public enum DebugFlags
    {
        None = 0,
        BaseColor = 1 << 0,
        Alpha = 1 << 1,
        Normals = 1 << 2,
        TangentSpaceNormals = 1 << 3,
        VertexNormals = 1 << 4,
        Roughness = 1 << 5,
        Metallic = 1 << 6,
        Occlusion = 1 << 7,
        Height = 1 << 8,
        Lighting = 1 << 9,
        Attenuation = 1 << 10,
        Reflections = 1 << 11,
        SpecularHighlights = 1 << 12,
        VertexColors = 1 << 13,
        Wind = 1 << 14
    }
}