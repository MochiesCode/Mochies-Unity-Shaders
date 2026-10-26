using UnityEngine;

namespace Mochie
{
    public interface IPostMaterialUpgradeCallback
    {
        void OnAfterMaterialUpgraded(Material mat);
    }
}