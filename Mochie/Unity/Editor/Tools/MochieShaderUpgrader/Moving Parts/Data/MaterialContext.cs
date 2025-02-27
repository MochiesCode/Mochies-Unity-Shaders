using System;
using UnityEditor;
using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    public class MaterialContext
    {
        public Material Material { get; private set; }
        
        SerializedProperty textureProperties;
        SerializedProperty intProperties;
        SerializedProperty floatProperties;
        SerializedProperty colorProperties;

        private MaterialContext() {}
        
        public MaterialContext(Material material)
        {
            if(material == null)
                throw new NullReferenceException("Material can't be null");
            
            Material = material;
                    
            var serializedMaterial = new SerializedObject(Material);
            var savedProps = serializedMaterial.FindProperty("m_SavedProperties"); 
            textureProperties = savedProps.FindPropertyRelative("m_TexEnvs");
            intProperties = savedProps.FindPropertyRelative("m_Ints");
            floatProperties = savedProps.FindPropertyRelative("m_Floats");
            colorProperties = savedProps.FindPropertyRelative("m_Colors");
        }

        /// <summary>
        /// Try to get the float value of <paramref name="propertyName"/>
        /// </summary>
        /// <param name="propertyName">Property to get value off</param>
        /// <param name="value">Value if true returned, otherwise default</param>
        /// <returns>True if value was found</returns>
        public bool TryGetFloat(string propertyName, out float value)
        {
            value = default;
            foreach(SerializedProperty child in floatProperties)
            {
                if(child.displayName != propertyName)
                    continue;
                
                value = child.type == "pair" ? child.FindPropertyRelative("second").floatValue : child.floatValue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Try to get the texture value of <paramref name="propertyName"/>
        /// </summary>
        /// <param name="propertyName">Property to get value off</param>
        /// <param name="value">Value if true returned, otherwise default</param>
        /// <returns>True if value was found</returns>
        public bool TryGetTexture(string propertyName, out TextureContainer value)
        {
            value = default;
            foreach(SerializedProperty child in textureProperties)
            {
                if(child.displayName != propertyName)
                    continue;
                
                var valueContainer = child.FindPropertyRelative("second");
                value = new TextureContainer
                {
                    texture = valueContainer.FindPropertyRelative("m_Texture").objectReferenceValue as Texture,
                    offset = valueContainer.FindPropertyRelative("m_Offset").vector2Value,
                    scale = valueContainer.FindPropertyRelative("m_Scale").vector2Value
                };
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Try to get the int value of <paramref name="propertyName"/>
        /// </summary>
        /// <param name="propertyName">Property to get value off</param>
        /// <param name="value">Value if true returned, otherwise default</param>
        /// <returns>True if value was found</returns>
        public bool TryGetInt(string propertyName, out int value)
        {
            value = default;
            foreach(SerializedProperty child in intProperties)
            {
                if(child.displayName != propertyName)
                    continue;
                
                value = child.type == "pair" ? child.FindPropertyRelative("second").intValue : child.intValue;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Try to get the Color value of <paramref name="propertyName"/>. Also used for Vectors
        /// </summary>
        /// <param name="propertyName">Property to get value off</param>
        /// <param name="value">Value if true returned, otherwise default</param>
        /// <returns>True if value was found</returns>
        public bool TryGetColorOrVector(string propertyName, out Color value)
        {
            value = default;
            foreach(SerializedProperty child in colorProperties)
            {
                if(child.displayName != propertyName)
                    continue;
                
                value = child.type == "pair" ? child.FindPropertyRelative("second").colorValue : child.colorValue;
                return true;
            }
            return false;
        }
    }
}