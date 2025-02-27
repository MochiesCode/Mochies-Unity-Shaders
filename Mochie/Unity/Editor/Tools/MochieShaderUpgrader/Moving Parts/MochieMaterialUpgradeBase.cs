using System.Collections.Generic;
using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    public abstract class MochieMaterialUpgradeBase
    {
        protected MochieMaterialUpgradeBase()
        {
            propertyActions.AddRange(AddPropertyActions());
        }
        
        readonly List<PropertyActionBase> propertyActions = new List<PropertyActionBase>();

        /// <summary>
        /// Can this material be upgraded? Returning false here will skip this material.
        /// </summary>
        /// <param name="material">Material to be checked</param>
        /// <returns></returns>
        public abstract bool CanUpgradeMaterial(Material material);

        /// <summary>
        /// Define property actions to be added to the propertyActions list
        /// </summary>
        /// <returns></returns>
        public abstract List<PropertyActionBase> AddPropertyActions();
        
        /// <summary>
        /// Iterate through all properties in propertyActions and run their actions 
        /// </summary>
        /// <param name="material"></param>
        public void RunUpgrade(Material material)
        {
            if(!CanUpgradeMaterial(material))
                return;
            
            Debug.Log($"Running upgrade <b>{GetType().Name}</b> on Material <b>{material.name}</b>");
            var context = new MaterialContext(material);
            
            foreach(var action in propertyActions)
                action.RunAction(context);
        }
    }
}