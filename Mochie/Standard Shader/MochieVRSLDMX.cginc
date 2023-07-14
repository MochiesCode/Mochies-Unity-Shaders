#define IF(a, b, c) lerp(b, c, step((fixed) (a), 0));
UNITY_INSTANCING_BUFFER_START(Props)
    UNITY_DEFINE_INSTANCED_PROP(uint, _DMXChannel)
    UNITY_DEFINE_INSTANCED_PROP(uint, _NineUniverseMode)
    UNITY_DEFINE_INSTANCED_PROP(uint, _EnableDMX)
    UNITY_DEFINE_INSTANCED_PROP(uint, _PanInvert)
    UNITY_DEFINE_INSTANCED_PROP(uint, _TiltInvert)
    UNITY_DEFINE_INSTANCED_PROP(float4, _Emission)
    UNITY_DEFINE_INSTANCED_PROP(float4, _EmissionDMX)
    UNITY_DEFINE_INSTANCED_PROP(float, _GlobalIntensity)
    UNITY_DEFINE_INSTANCED_PROP(float, _FinalIntensity)
    UNITY_DEFINE_INSTANCED_PROP(uint, _EnableStrobe)
    UNITY_DEFINE_INSTANCED_PROP(uint, _FixtureRotationX)
    UNITY_DEFINE_INSTANCED_PROP(uint, _FixtureBaseRotationY)
UNITY_INSTANCING_BUFFER_END(Props)

#ifdef _VRSL_LEGACY_TEXTURES
    Texture2D _OSCGridRenderTexture, _OSCGridRenderTextureRAW, _OSCGridStrobeTimer, _OSCGridSpinTimer;
    uniform float4 _OSCGridRenderTextureRAW_TexelSize, _OSCGridSpinTimer_TexelSize, _OSCGridRenderTexture_TexelSize;
    SamplerState VRSL_PointClampSampler;
#else
    Texture2D _Udon_DMXGridRenderTexture;
    uniform float4 _Udon_DMXGridRenderTexture_TexelSize;
    Texture2D _Udon_DMXGridStrobeTimer, _Udon_DMXGridSpinTimer, _Udon_DMXGridRenderTextureMovement;
    uniform float4 _Udon_DMXGridStrobeTimer_TexelSize, _Udon_DMXGridSpinTimer_TexelSize, _Udon_DMXGridRenderTextureMovement_TexelSize;
    SamplerState VRSL_PointClampSampler;
#endif


uint _EnableCompatibilityMode, _EnableVerticalMode;
half _MaxMinTiltAngle, _MaxMinPanAngle;

float VRSL_invLerp(float from, float to, float value)
{
  return (value - from) / (to - from);
}

float VRSL_remap(float origFrom, float origTo, float targetFrom, float targetTo, float value)
{
  float rel = VRSL_invLerp(origFrom, origTo, value);
  return lerp(targetFrom, targetTo, rel);
}

float4 getBaseEmission()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props, _Emission);
}
float4 getAltBaseEmission()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props, _EmissionDMX);
}

float getGlobalIntensity()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props, _GlobalIntensity);
}

float getFinalIntensity()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props, _FinalIntensity);
}

uint isStrobe()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props,_EnableStrobe);
}

uint isDMX()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props,_EnableDMX);
}

uint GetDMXChannel()
{
    return (uint) round(UNITY_ACCESS_INSTANCED_PROP(Props, _DMXChannel));  
}

int ConvertToRawDMXChannel(int chan, int universe)
{
    return abs(chan + ((universe-1) * 512) + ((universe-1) * 8));
}

uint getNineUniverseMode()
{
    return (uint) UNITY_ACCESS_INSTANCED_PROP(Props, _NineUniverseMode);
}
uint GetPanInvert()
{
    return (uint) UNITY_ACCESS_INSTANCED_PROP(Props, _PanInvert);
}
uint GetTiltInvert()
{
    return (uint) UNITY_ACCESS_INSTANCED_PROP(Props, _TiltInvert);
}
float GetOffsetX()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props,_FixtureRotationX);
}

float GetOffsetY()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props,_FixtureBaseRotationY);
}


float2 LegacyRead(int channel, int sector)
{
    // say we were on sector 6
    // we need to move over 2 sectors
    // and we need to move up 3 sectors

    //1 sector is every 13 channels
        float x = 0.02000;
        float y = 0.02000;
        //TRAVERSING THE Y AXIS OF THE OSC GRID
        float ymod = floor(sector / 2.0);       

        //TRAVERSING THE X AXIS OF THE OSC GRID
        float xmod = sector % 2.0;

        x+= (xmod * 0.50);
        y+= (ymod * 0.04);
        y-= sector >= 23 ? 0.025 : 0.0;
        x+= (channel * 0.04);
        x-= sector >= 40 ? 0.01 : 0.0;
        //we are now on the correct
        return float2(x,y);

}

float2 IndustryRead(int x, int y)
{
    #ifdef _VRSL_LEGACY_TEXTURES
        float resMultiplierX = (_OSCGridRenderTextureRAW_TexelSize.z / 13);
        float2 xyUV = float2(0.0,0.0);
        
        xyUV.x = ((x * resMultiplierX) * _OSCGridRenderTextureRAW_TexelSize.x);
        xyUV.y = (y * resMultiplierX) * _OSCGridRenderTextureRAW_TexelSize.y;
    #else
        float resMultiplierX = (_Udon_DMXGridRenderTexture_TexelSize.z / 13);
        float2 xyUV = float2(0.0,0.0);
        
        xyUV.x = ((x * resMultiplierX) * _Udon_DMXGridRenderTexture_TexelSize.x);
        xyUV.y = (y * resMultiplierX) * _Udon_DMXGridRenderTexture_TexelSize.y;
    #endif
    xyUV.y -= 0.001915;
    xyUV.x -= 0.015;
   // xyUV.x = DMXChannel == 15 ? xyUV.x + 0.0769 : xyUV.x;
    return xyUV;
}
int getTargetRGBValue(uint universe)
{
    universe -=1;
    return floor((int)(universe / 3));
    //returns 0 for red, 1 for green, 2, for blue
}

//function for getting the value on the DMX Grid
//Returns a value from 0 to 1
float ReadDMX(uint DMXChannel, Texture2D _Tex)
{
    uint universe = ceil(((int) DMXChannel)/512.0);
    int targetColor = getTargetRGBValue(universe);
    
    //DMXChannel = DMXChannel == 15.0 ? DMXChannel + 1 : DMXChannel;
    universe-=1;
    DMXChannel = targetColor > 0 ? DMXChannel - (((universe - (universe % 3)) * 512)) - (targetColor * 24) : DMXChannel;

    uint x = DMXChannel % 13; // starts at 1 ends at 13
    x = x == 0.0 ? 13.0 : x;
    float y = DMXChannel / 13.0; // starts at 1 // doubles as sector
    y = frac(y)== 0.00000 ? y - 1 : y;
    if(x == 13.0) //for the 13th channel of each sector... Go down a sector for these DMX Channel Ranges...
    {
    
        //I don't know why, but we need this for some reason otherwise the 13th channel gets shifted around improperly.
        //I"m not sure how to express these exception ranges mathematically. Doing so would be much more cleaner though.
        y = DMXChannel >= 90 && DMXChannel <= 101 ? y - 1 : y;
        y = DMXChannel >= 160 && DMXChannel <= 205 ? y - 1 : y;
        y = DMXChannel >= 326 && DMXChannel <= 404 ? y - 1 : y;
        y = DMXChannel >= 676 && DMXChannel <= 819 ? y - 1 : y;
        y = DMXChannel >= 1339 ? y - 1 : y;
    }

    // y = (y > 6 && y < 31) && x == 13.0 ? y - 1 : y;
    
    float2 xyUV = _EnableCompatibilityMode == 1 ? LegacyRead(x-1.0,y) : IndustryRead(x,(y + 1.0));
        
    float4 uvcoords = float4(xyUV.x, xyUV.y, 0,0);
    //float4 c = tex2Dlod(_Tex, uvcoords);
    float4 c = _Tex.SampleLevel(VRSL_PointClampSampler, xyUV, 0);
    float value = 0.0;
    
   if(getNineUniverseMode() && _EnableCompatibilityMode != 1)
   {
    value = c.r;
    value = IF(targetColor > 0, c.g, value);
    value = IF(targetColor > 1, c.b, value);
   }
   else
   {
        float3 cRGB = float3(c.r, c.g, c.b);
        value = LinearRgbToLuminance(cRGB);
    }
    value = LinearToGammaSpaceExact(value);
    return value;
}


//function for getting the value on the DMX Grid
//Returns a value from 0 to 1
float ReadDMXRaw(uint DMXChannel, Texture2D _Tex)
{
   // DMXChannel = DMXChannel == 15.0 ? DMXChannel + 1 : DMXChannel;
    uint universe = ceil(((int) DMXChannel)/512.0);
    int targetColor = getTargetRGBValue(universe);

    universe-=1;
    DMXChannel = targetColor > 0 ? DMXChannel - (((universe - (universe % 3)) * 512)) - (targetColor * 24) : DMXChannel;


    uint x = DMXChannel % 13; // starts at 1 ends at 13
    x = x == 0.0 ? 13.0 : x;
    float y = DMXChannel / 13.0; // starts at 1 // doubles as sector
    y = frac(y)== 0.00000 ? y - 1 : y;
    
    float2 xyUV = _EnableCompatibilityMode == 1 ? LegacyRead(x-1.0,y) : IndustryRead(x,(y + 1.0));

    float4 uvcoords = float4(xyUV.x, xyUV.y, 0,0);
    //float4 c = tex2Dlod(_Tex, uvcoords);
    float4 c = _Tex.SampleLevel(VRSL_PointClampSampler, xyUV, 0);
    float value = c.r;
    value = IF(targetColor > 0, c.g, value);
    value = IF(targetColor > 1, c.b, value);
	return value;
}

float GetStrobeOutput(uint DMXChannel)
{
    #ifdef _VRSL_LEGACY_TEXTURES
        float phase = ReadDMXRaw(DMXChannel, _OSCGridStrobeTimer);
        float status = ReadDMX(DMXChannel, _OSCGridRenderTextureRAW);
    #else
        float phase = ReadDMXRaw(DMXChannel, _Udon_DMXGridStrobeTimer);
        float status = ReadDMX(DMXChannel, _Udon_DMXGridRenderTexture);
    #endif

    half strobe = (sin(phase));//Get sin wave
    strobe = IF(strobe > 0.0, 1.0, 0.0);//turn to square wave
    //strobe = saturate(strobe);

    strobe = IF(status > 0.2, strobe, 1); //minimum channel threshold set
    
    //check if we should even be strobing at all.
    strobe = IF(isDMX() == 1, strobe, 1);
    strobe = IF(isStrobe() == 1, strobe, 1);
    
    return strobe;

}

float GetImmediateStrobeOutput(uint DMXChannel)
{
    
    #ifdef _VRSL_LEGACY_TEXTURES
        float phase = ReadDMXRaw(DMXChannel, _OSCGridStrobeTimer);
        float status = ReadDMX(DMXChannel, _OSCGridRenderTextureRAW);
    #else
        float phase = ReadDMXRaw(DMXChannel, _Udon_DMXGridStrobeTimer);
        float status = ReadDMX(DMXChannel, _Udon_DMXGridRenderTexture);
    #endif

    half strobe = (sin(phase));//Get sin wave
    strobe = IF(strobe > 0.0, 1.0, 0.0);//turn to square wave
    //strobe = saturate(strobe);

    strobe = IF(status > 0.2, strobe, 1); //minimum channel threshold set
    return strobe;

}

//Function for getting the RGB Color Value (Channels 4, 5, and 6)
float4 GetDMXColor(uint DMXChannel)
{
    #ifdef _VRSL_LEGACY_TEXTURES
        float redchannel = ReadDMX(DMXChannel, _OSCGridRenderTextureRAW);
        float greenchannel = ReadDMX(DMXChannel + 1, _OSCGridRenderTextureRAW);
        float bluechannel = ReadDMX(DMXChannel + 2, _OSCGridRenderTextureRAW);
    #else
        float redchannel = ReadDMX(DMXChannel, _Udon_DMXGridRenderTexture);
        float greenchannel = ReadDMX(DMXChannel + 1, _Udon_DMXGridRenderTexture);
        float bluechannel = ReadDMX(DMXChannel + 2, _Udon_DMXGridRenderTexture);
    #endif


    #if defined(PROJECTION_YES)
        redchannel = redchannel * _RedMultiplier;
        bluechannel = bluechannel * _BlueMultiplier;
        greenchannel = greenchannel * _GreenMultiplier;
    #endif


    //return IF(isOSC() == 1,lerp(fixed4(0,0,0,1), float4(redchannel,greenchannel,bluechannel,1), GetOSCIntensity(DMXChannel, _FixtureMaxIntensity)), float4(redchannel,greenchannel,bluechannel,1) * GetOSCIntensity(DMXChannel, _FixtureMaxIntensity));
    return float4(redchannel,greenchannel,bluechannel,1);
}

float4 calculateRotations(float4 vertexInput, float4 vertexColor, int normalsCheck, float pan, float tilt, float4 rotationOrigin)
{
    //	vertexInput = IF(worldspacecheck == 1, float4(UnityObjectToWorldNormal(v.normal).x * -1.0, UnityObjectToWorldNormal(v.normal).y * -1.0, UnityObjectToWorldNormal(v.normal).z * -1.0, 1), vertexInput)
    #if defined(_VRSLPAN_ON)
        //CALCULATE BASE ROTATION. MORE FUN MATH. THIS IS FOR PAN.
        float angleY = radians(GetOffsetY() + pan);
        float c, s;
        sincos(angleY, s, c);

        float3x3 rotateYMatrix = float3x3(c, -s, 0,
                                        s, c, 0,
                                        0, 0, 1);
        float3 BaseAndFixturePos = vertexInput.xyz;

        //INVERSION CHECK
        rotateYMatrix = GetPanInvert() == 1 ? transpose(rotateYMatrix) : rotateYMatrix;

        float3 localRotY = mul(rotateYMatrix, BaseAndFixturePos);
        //LOCALROTY IS NEW ROTATION



    #endif

    #if defined(_VRSLTILT_ON)
        //CALCULATE FIXTURE ROTATION. WOO FUN MATH. THIS IS FOR TILT.

        //set new origin to do transform
        float3 newOrigin = vertexInput.w * rotationOrigin.xyz;
        //if vertexInput.w is 1 (vertex), origin changes
        //if vertexInput.w is 0 (normal/tangent), origin doesn't change

        //subtract new origin from original origin for blue vertexes
        vertexInput.xyz = vertexColor.b == 1.0 ? vertexInput.xyz - newOrigin : vertexInput.xyz;
        //DO ROTATION

        //#if defined(PROJECTION_YES)
        //buffer[3] = GetTiltValue(sector);
        //#endif
        float angleX = radians(GetOffsetX() + tilt);
        float cT, sT;
        sincos(angleX, sT, cT);
        float3x3 rotateXMatrix = float3x3(1, 0, 0,
                                        0, cT, -sT,
                                        0, sT, cT);
            
        //float4 fixtureVertexPos = vertexInput;
            
        //INVERSION CHECK
        rotateXMatrix = GetTiltInvert() == 1 ? transpose(rotateXMatrix) : rotateXMatrix;

        //float4 localRotX = mul(rotateXMatrix, fixtureVertexPos);
        //LOCALROTX IS NEW ROTATION
        //COMBINED ROTATION FOR FIXTURE
        #if defined(_VRSLPAN_ON)
            float3x3 rotateXYMatrix = mul(rotateYMatrix, rotateXMatrix);
            float3 localRotXY = mul(rotateXYMatrix, vertexInput.xyz);
        #else
            float3 localRotX = mul(rotateXMatrix, vertexInput.xyz);
        #endif
    #endif
	//LOCALROTXY IS COMBINED ROTATION
	//Apply fixture rotation ONLY to those with blue vertex colors
    #if defined(_VRSLTILT_ON)
	//apply LocalRotXY rotation then add back old origin
        #if defined(_VRSLPAN_ON)
	        vertexInput.xyz = vertexColor.b == 1.0 ? localRotXY + newOrigin : vertexInput.xyz;
        #else
            vertexInput.xyz = vertexColor.b == 1.0 ? localRotX + newOrigin : vertexInput.xyz;
        #endif
    #endif
	//vertexInput.xyz = v.color.b == 1.0 ? vertexInput.xyz + newOrigin : vertexInput.xyz;
	
	//appy LocalRotY rotation to lightfixture base;
    #if defined(_VRSLPAN_ON)
	    vertexInput.xyz = vertexColor.g == 1.0 ? localRotY : vertexInput.xyz;
    #endif
	return vertexInput;
}

float GetPanValue(uint DMXChannel)
{
    #ifdef _VRSL_LEGACY_TEXTURES
        float inputValue = ReadDMX(DMXChannel, _OSCGridRenderTexture);
    #else
        float inputValue = ReadDMX(DMXChannel, _Udon_DMXGridRenderTextureMovement);
    #endif
    return ((_MaxMinPanAngle * 2) * (inputValue)) - _MaxMinPanAngle;
}

float GetTiltValue(uint DMXChannel)
{
    #ifdef _VRSL_LEGACY_TEXTURES
        float inputValue = ReadDMX(DMXChannel + 2, _OSCGridRenderTexture);
    #else
        float inputValue = ReadDMX(DMXChannel + 2, _Udon_DMXGridRenderTextureMovement);
    #endif
    return ((_MaxMinTiltAngle * 2) * (inputValue)) - _MaxMinTiltAngle; 
}