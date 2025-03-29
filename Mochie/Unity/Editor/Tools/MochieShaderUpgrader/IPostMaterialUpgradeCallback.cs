using UnityEngine;

namespace Mochie
{
    public interface IPostMaterialUpgradeCallback
    {
        public void OnAfterMaterialUpgraded(Material mat);
    }
}