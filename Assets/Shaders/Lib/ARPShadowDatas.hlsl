#pragma once

#define MAX_DIRECTIONS_SHADOW_LIGHTS 4
#define MAX_DIRECTIONS_CASCADES 4

CBUFFER_START(ShadowBuffer)
    float4      _DirectionalShadowDatas[MAX_DIRECTIONS_SHADOW_LIGHTS];
float4x4    _ShadowToWorldCascadeMat[MAX_DIRECTIONS_SHADOW_LIGHTS * MAX_DIRECTIONS_CASCADES];
float4      _CullSpherePos[MAX_DIRECTIONS_CASCADES];
float4      _CullSphereData[MAX_DIRECTIONS_CASCADES];
float4      _ShadowMapTexelSize;
float4      _ShadowDistanceData;
float4      _CascadeData;
int         _CascadeCount;
CBUFFER_END


#if defined(ENABLE_DIRECTIONAL_SOFTSHADOW_PCF3X3)
    #define SOFTSHDADOW_COMPUTESAMPLES_TENT SampleShadow_ComputeSamples_Tent_3x3
    #define FLITER_SIZE 4
#elif defined(ENABLE_DIRECTIONAL_SOFTSHADOW_PCF5X5)
    #define SOFTSHDADOW_COMPUTESAMPLES_TENT SampleShadow_ComputeSamples_Tent_5x5
    #define FLITER_SIZE 9
#elif defined(ENABLE_DIRECTIONAL_SOFTSHADOW_PCF9X9)
    #define SOFTSHDADOW_COMPUTESAMPLES_TENT SampleShadow_ComputeSamples_Tent_5x5
    #define FLITER_SIZE 16
#endif


struct DirectionalShadowData
{
    float strength;
    float normalbias;
    int enableSoftShadow;

    int CascadeIndex;
};


DirectionalShadowData GetDirectionalShadowData(int index,int cascadeIndex)
{
    float4 shadowdata           = _DirectionalShadowDatas[index];
    
    DirectionalShadowData data  = (DirectionalShadowData) 0;
    data.strength               = shadowdata.x;
    data.normalbias             = shadowdata.y;
    data.enableSoftShadow       = asint(shadowdata.z);
    
    data.CascadeIndex           = cascadeIndex;
    
    return data;
}

struct ShadowDistaceData
{
    float OneOverShadowDistance;
    float OneOverShadowDistanceFade;
    float ShadowDistanceSqr;
};

ShadowDistaceData GetShadowDistaceData()
{
    ShadowDistaceData data;

    data.OneOverShadowDistance      = _ShadowDistanceData.x;
    data.OneOverShadowDistanceFade  = _ShadowDistanceData.y;
    data.ShadowDistanceSqr          = _ShadowDistanceData.z;

    return data;
}