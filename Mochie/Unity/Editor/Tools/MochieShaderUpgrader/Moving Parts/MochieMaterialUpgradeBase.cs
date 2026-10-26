using System.Collections.Generic;
using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    public abstract class MochieMaterialUpgradeBase
    {
        List<UpgradeActionBase> _propertyActions;

        public List<UpgradeActionBase> PropertyActions
        {
            get
            {
                if(_propertyActions == null)
                    _propertyActions = AddUpgradeActions() ?? new List<UpgradeActionBase>();
                return _propertyActions;
            }
        }

        /// <summary>
        /// Can this material be upgraded? Returning false here will skip this material.
        /// </summary>
        /// <param name="material">Material to be checked</param>
        /// <returns></returns>
        public virtual bool CanUpgradeMaterial(Material material) => true;

        /// <summary>
        /// Define property actions to be added to the propertyActions list
        /// </summary>
        /// <returns></returns>
        public abstract List<UpgradeActionBase> AddUpgradeActions();

        /// <summary>
        /// Iterate through all properties in propertyActions and run their actions 
        /// </summary>
        /// <param name="material"></param>
        public virtual bool RunUpgrade(Material material)
        {
            if(!CanUpgradeMaterial(material))
                return false;

            var context = new MaterialContext(material);
            return RunUpgrade(material, context);
        }

        /// <summary>
        /// Iterate through all properties in propertyActions and run their actions with an existing context
        /// </summary>
        /// <param name="material"></param>
        /// <param name="context"></param>
        public virtual bool RunUpgrade(Material material, MaterialContext context)
        {
            if(!CanUpgradeMaterial(material))
                return false;
            
            #if MOCHIE_DEV
            Debug.Log($"Running upgrade <b>{GetType().Name}</b> on Material <b>{material.name}</b>");
            #endif

            foreach(var action in PropertyActions)
                action.RunAction(context);

            return true;
        }
    }
}