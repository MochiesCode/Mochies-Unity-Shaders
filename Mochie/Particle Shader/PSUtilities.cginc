#ifndef P_UTILS_INCLUDED
#define P_UTILS_INCLUDED

float3 GetCameraPos(){
    float3 cameraPos = _WorldSpaceCameraPos;
    #if UNITY_SINGLE_PASS_STEREO
        cameraPos = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1]) * 0.5;
    #endif
    return cameraPos;
}

float Average(float3 rgb){
    return (rgb.r + rgb.g + rgb.b)/3.0;
}

#endif