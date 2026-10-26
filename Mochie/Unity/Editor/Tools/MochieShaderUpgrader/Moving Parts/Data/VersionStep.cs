namespace Mochie.ShaderUpgrader
{
    /// <summary>
    /// One migration in a <see cref="VersionedShaderUpgradeInfo"/>: brings materials saved with an older package up to the release that changed things.
    /// </summary>
    public class VersionStep
    {
        public string Version { get; private set; }
        public int ToVersion { get; private set; }
        public MochieMaterialUpgradeBase Upgrade { get; private set; }

        /// <param name="version">Release that introduced the breaking change, e.g. "1.79" or "1.79.1"</param>
        /// <param name="upgrade">Actions that move a material from the previous release to this one</param>
        public VersionStep(string version, MochieMaterialUpgradeBase upgrade)
        {
            Version = version;
            ToVersion = MaterialVersionStamp.Encode(version);
            Upgrade = upgrade;
        }
    }
}
