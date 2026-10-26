using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    public class MaterialContext
    {
        public Material Material { get; private set; }

        readonly Dictionary<string, float> _floats = new Dictionary<string, float>(StringComparer.Ordinal);
        readonly Dictionary<string, int> _ints = new Dictionary<string, int>(StringComparer.Ordinal);
        readonly Dictionary<string, Color> _colors = new Dictionary<string, Color>(StringComparer.Ordinal);
        readonly Dictionary<string, TextureContainer> _textures = new Dictionary<string, TextureContainer>(StringComparer.Ordinal);

        private MaterialContext() {}
        
        public MaterialContext(Material material)
        {
            if(material == null)
                throw new ArgumentNullException(nameof(material), "Material can't be null");
            
            Material = material;
                    
            var serializedMaterial = new SerializedObject(Material);
            var savedProps = serializedMaterial.FindProperty("m_SavedProperties");
            if(savedProps == null)
                return;

            ExtractFloats(savedProps.FindPropertyRelative("m_Floats"));
            ExtractInts(savedProps.FindPropertyRelative("m_Ints"));
            ExtractColors(savedProps.FindPropertyRelative("m_Colors"));
            ExtractTextures(savedProps.FindPropertyRelative("m_TexEnvs"));
        }

        void ExtractFloats(SerializedProperty floatProperties)
        {
            if(floatProperties == null || !floatProperties.isArray)
                return;

            for(int i = 0; i < floatProperties.arraySize; i++)
            {
                var child = floatProperties.GetArrayElementAtIndex(i);
                string key = GetPropertyKey(child);
                if(string.IsNullOrEmpty(key))
                    continue;

                float val = child.type == "pair" ? child.FindPropertyRelative("second").floatValue : child.floatValue;
                _floats[key] = val;
            }
        }

        void ExtractInts(SerializedProperty intProperties)
        {
            if(intProperties == null || !intProperties.isArray)
                return;

            for(int i = 0; i < intProperties.arraySize; i++)
            {
                var child = intProperties.GetArrayElementAtIndex(i);
                string key = GetPropertyKey(child);
                if(string.IsNullOrEmpty(key))
                    continue;

                int val = child.type == "pair" ? child.FindPropertyRelative("second").intValue : child.intValue;
                _ints[key] = val;
            }
        }

        void ExtractColors(SerializedProperty colorProperties)
        {
            if(colorProperties == null || !colorProperties.isArray)
                return;

            for(int i = 0; i < colorProperties.arraySize; i++)
            {
                var child = colorProperties.GetArrayElementAtIndex(i);
                string key = GetPropertyKey(child);
                if(string.IsNullOrEmpty(key))
                    continue;

                Color val = child.type == "pair" ? child.FindPropertyRelative("second").colorValue : child.colorValue;
                _colors[key] = val;
            }
        }

        void ExtractTextures(SerializedProperty textureProperties)
        {
            if(textureProperties == null || !textureProperties.isArray)
                return;

            for(int i = 0; i < textureProperties.arraySize; i++)
            {
                var child = textureProperties.GetArrayElementAtIndex(i);
                string key = GetPropertyKey(child);
                if(string.IsNullOrEmpty(key))
                    continue;

                var valueContainer = child.FindPropertyRelative("second");
                if(valueContainer != null)
                {
                    var texProp = valueContainer.FindPropertyRelative("m_Texture");
                    var offsetProp = valueContainer.FindPropertyRelative("m_Offset");
                    var scaleProp = valueContainer.FindPropertyRelative("m_Scale");

                    _textures[key] = new TextureContainer
                    {
                        texture = texProp?.objectReferenceValue as Texture,
                        offset = offsetProp != null ? offsetProp.vector2Value : Vector2.zero,
                        scale = scaleProp != null ? scaleProp.vector2Value : Vector2.one
                    };
                }
            }
        }

        static string GetPropertyKey(SerializedProperty element)
        {
            var keyProp = element.FindPropertyRelative("first");
            if(keyProp != null && !string.IsNullOrEmpty(keyProp.stringValue))
                return keyProp.stringValue;

            return element.displayName;
        }

        /// <summary>
        /// Try to get the float value of <paramref name="propertyName"/>
        /// </summary>
        /// <param name="propertyName">Property to get value off</param>
        /// <param name="value">Value if true returned, otherwise default</param>
        /// <returns>True if value was found</returns>
        public bool TryGetFloat(string propertyName, out float value)
        {
            if(_floats.TryGetValue(propertyName, out value))
                return true;

            // Fallback: Check if serialized as integer
            if(_ints.TryGetValue(propertyName, out int intVal))
            {
                value = intVal;
                return true;
            }

            value = default;
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
            return _textures.TryGetValue(propertyName, out value);
        }
        
        /// <summary>
        /// Try to get the int value of <paramref name="propertyName"/>
        /// </summary>
        /// <param name="propertyName">Property to get value off</param>
        /// <param name="value">Value if true returned, otherwise default</param>
        /// <returns>True if value was found</returns>
        public bool TryGetInt(string propertyName, out int value)
        {
            if(_ints.TryGetValue(propertyName, out value))
                return true;

            // Fallback: Check if serialized as float (common in Unity ShaderLab)
            if(_floats.TryGetValue(propertyName, out float floatVal))
            {
                value = Mathf.RoundToInt(floatVal);
                return true;
            }

            value = default;
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
            return _colors.TryGetValue(propertyName, out value);
        }
    }
}