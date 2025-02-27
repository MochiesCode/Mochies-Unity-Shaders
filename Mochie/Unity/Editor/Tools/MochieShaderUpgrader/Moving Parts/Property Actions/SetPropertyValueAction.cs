using System;
using UnityEditor;
using UnityEngine;

namespace Mochie.ShaderUpgrader
{
    /// <summary>
    /// Sets a material's property value based on TargetPropertyName
    /// </summary>
    public class SetPropertyValueAction : PropertyActionBase
    {
        bool SkipIfNonDefault { get; }
        float FloatValue { get; }
        int IntValue { get; }
        Vector4 VectorValue { get; }
        GUID GuidValue { get; }

        /// <summary>
        /// Sets a material's Float property value based on TargetPropertyName 
        /// </summary>
        /// <param name="targetPropertyName">Target property name</param>
        /// <param name="skipIfNonDefault">Skip this action if value of <paramref name="targetPropertyName"/> is not default.</param>
        /// <param name="floatValue">Float value to set</param>
        public SetPropertyValueAction(string targetPropertyName, bool skipIfNonDefault, float floatValue) : base(null, targetPropertyName, SerializedMaterialPropertyType.Float)
        {
            SkipIfNonDefault = skipIfNonDefault;
            FloatValue = floatValue;
        }
        
        /// <summary>
        /// Sets a material's Int property value based on TargetPropertyName 
        /// </summary>
        /// <param name="targetPropertyName">Target property name</param>
        /// <param name="skipIfNonDefault">Skip this action if value of <paramref name="targetPropertyName"/> is not default.</param>
        /// <param name="intValue">Int value to set</param>
        public SetPropertyValueAction(string targetPropertyName, bool skipIfNonDefault, int intValue) : base(null, targetPropertyName, SerializedMaterialPropertyType.Int)
        {
            SkipIfNonDefault = skipIfNonDefault;
            IntValue = intValue;
        }
        
        /// <summary>
        /// Sets a material's Texture based on TargetPropertyName. Guid is used to load the texture from the AssetDatabase. 
        /// </summary>
        /// <param name="targetPropertyName">Target property name</param>
        /// <param name="skipIfNonDefault">Skip this action if value of <paramref name="targetPropertyName"/> is not default.</param>
        /// <param name="guidValue">Guid value to load texture from</param>
        public SetPropertyValueAction(string targetPropertyName, bool skipIfNonDefault, GUID guidValue) : base(null, targetPropertyName, SerializedMaterialPropertyType.Texture)
        {
            SkipIfNonDefault = skipIfNonDefault;
            GuidValue = guidValue;
        }
        
        /// <summary>
        /// Sets a material's Vector or Color property value based on TargetPropertyName 
        /// </summary>
        /// <param name="targetPropertyName">Target property name</param>
        /// <param name="skipIfNonDefault">Skip this action if value of <paramref name="targetPropertyName"/> is not default.</param>
        /// <param name="vectorValue">Vector value to set. Can be Vector2, Vector3, Vector4 or Color</param>
        public SetPropertyValueAction(string targetPropertyName, bool skipIfNonDefault, Vector4 vectorValue) : base(null, targetPropertyName, SerializedMaterialPropertyType.Vector)
        {
            SkipIfNonDefault = skipIfNonDefault;
            VectorValue = vectorValue;
        }

        public override void RunAction(MaterialContext materialContext)
        {
            switch(PropertyType)
            {
                case SerializedMaterialPropertyType.Float:
                    if(!SkipIfNonDefault)
                    {
                        if(materialContext.TryGetFloat(TargetPropertyName, out float currentFloatValue))
                        {
                            if(Mathf.Approximately(currentFloatValue, default))
                                materialContext.Material.SetFloat(TargetPropertyName, FloatValue);
                        }
                        else
                        {
                            #if MOCHIE_DEV
                            Debug.LogWarning($"Failed to set <b>Float</b> property <b>{TargetPropertyName}</b> in action <b>{GetType().Name}</b>. Property not found");
                            #endif
                        }
                    }
                    else
                    {
                        materialContext.Material.SetFloat(TargetPropertyName, FloatValue);
                    }
                    break;
                case SerializedMaterialPropertyType.Int:
                    if(!SkipIfNonDefault) 
                    {
                        if(materialContext.TryGetInt(TargetPropertyName, out int currentIntValue))
                        {
                            if(currentIntValue == default)
                                materialContext.Material.SetInt(TargetPropertyName, IntValue);
                        }
                        else
                        {
                            #if MOCHIE_DEV
                            Debug.LogWarning($"Failed to set <b>Int</b> property <b>{TargetPropertyName}</b> in action <b>{GetType().Name}</b>. Property not found");
                            #endif
                        }
                    }
                    else
                    {
                        materialContext.Material.SetInt(TargetPropertyName, IntValue);
                    }
                    break;
                case SerializedMaterialPropertyType.Vector:
                    if(!SkipIfNonDefault)
                    {
                        if(materialContext.TryGetColorOrVector(TargetPropertyName, out Color currentVectorValue))
                        {
                            if(currentVectorValue == default)
                                materialContext.Material.SetColor(TargetPropertyName, VectorValue);
                        }
                        else
                        {
                            #if MOCHIE_DEV
                            Debug.LogWarning($"Failed to set <b>Color/Vector</b> property <b>{TargetPropertyName}</b> in action <b>{GetType().Name}</b>. Property not found");
                            #endif
                        }
                    }
                    else
                    {
                        materialContext.Material.SetColor(TargetPropertyName, VectorValue);
                    }
                    break;
                case SerializedMaterialPropertyType.Texture:
                    if(GuidValue.Empty())
                    {
                        #if MOCHIE_DEV
                        Debug.LogError($"Texture GUID is empty. Failed when running {GetType().Name} ({SourcePropertyName} -> {TargetPropertyName})");
                        #endif
                        break;
                    }

                    string assetPath = AssetDatabase.GUIDToAssetPath(GuidValue);
                    if(string.IsNullOrWhiteSpace(assetPath))
                    {
                        #if MOCHIE_DEV
                        Debug.LogWarning($"Couldn't get asset path of GUID {GuidValue}. Asset might not exist in your project. Failed when running {GetType().Name} ({SourcePropertyName} -> {TargetPropertyName})");
                        #endif
                        break;
                    }

                    Texture newTexture = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                    if(newTexture == null)
                    {
                        #if MOCHIE_DEV
                        Debug.LogWarning($"Couldn't load texture with guid {GuidValue}. It's probably not a texture. Failed when running {GetType().Name} ({SourcePropertyName} -> {TargetPropertyName})");
                        #endif
                        break;
                    }

                    if(!SkipIfNonDefault)
                    {
                        if(materialContext.TryGetTexture(TargetPropertyName, out TextureContainer currentTextureContainerValue))
                        {
                            if(currentTextureContainerValue.texture == null)
                                materialContext.Material.SetTexture(TargetPropertyName, newTexture);
                        }
                        else
                        {
                            #if MOCHIE_DEV
                            Debug.LogWarning($"Failed to set <b>Texture</b> property <b>{TargetPropertyName}</b> in action <b>{GetType().Name}</b>. Property not found");
                            #endif
                        }
                    }
                    else
                    {
                        materialContext.Material.SetTexture(TargetPropertyName, newTexture);
                    }
                    break;
                default:
                    #if MOCHIE_DEV
                    throw new ArgumentException($"Unsupported property type {PropertyType}. Failed when running {GetType().Name} ({TargetPropertyName})");
                    #endif
                    break;
            }
        }
    }
}