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

TEXTURE2D_SHADOW(_CascadeShadowMap);
SAMPLER_CMP(sampler_CascadeShadowMap);

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
};


DirectionalShadowData GetDirectionalShadowData(int index)
{
    float4 shadowdata           = _DirectionalShadowDatas[index];
    
    DirectionalShadowData data  = (DirectionalShadowData) 0;
    data.strength               = shadowdata.x;
    data.normalbias             = shadowdata.y;
    data.enableSoftShadow       = asint(shadowdata.z);
    
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

struct ShadowCascadeData
{
    float CascadeFadeRadius;
    float CascadeFadeScale;
};

ShadowCascadeData GetShadowCascadeData()
{
    ShadowCascadeData data;

    data.CascadeFadeRadius  = _CascadeData.x;
    data.CascadeFadeScale   = _CascadeData.y;

    return data;
}

struct ShadowStrengthCascadeData
{
    float cascadeIndex;
    float shadowStrengthFade;
};

float GetShadowFadeStrength(float distance, float scale, float fade)
{
    return saturate((1.0 - distance * scale) * fade);
}

float GetDistace(float3 pa, float3 pb)
{
    return dot(pa - pb,pa - pb);
}

float GetFadeShadowStrength(Surface surface, ShadowDistaceData shadowDistaceData)
{
    half oneOverShadowDistance          = shadowDistaceData.OneOverShadowDistance;
    half oneOverShadowDistanceFade      = shadowDistaceData.OneOverShadowDistanceFade;
    half depth                          = surface.depth;

    return GetShadowFadeStrength(depth, oneOverShadowDistance,oneOverShadowDistanceFade);
}

float GetCascadeFadeStrength(ShadowCascadeData cascadedata,int cascadeIndex, float3 worldPos)
{
    const float CascadeFadeRadius       = cascadedata.CascadeFadeRadius;
    const float CascadeFadeScale        = cascadedata.CascadeFadeScale;

    const float3 center                 = _CullSpherePos[cascadeIndex].xyz;
    const float radius                  = _CullSpherePos[cascadeIndex].z;
    const float distance                = GetDistace(center,worldPos);

    if (distance <radius)
    {
        return 1;
    }else
    {
        return GetShadowFadeStrength(distance, CascadeFadeRadius,CascadeFadeScale);
    }
}

float GetDistanceFadeStrength(float depth, float oneovershadowDistance, float oneoverfade)
{
    float temp = 1 - depth  * oneovershadowDistance;
    return saturate(temp * oneoverfade);
}

int GetCascadeIndex(float3 worldpos)
{
    int i = 0;
    for (; i < _CascadeCount; i++)
    {
        float4 cullsphere   = _CullSpherePos[i];
        float3 center       = cullsphere.xyz;
        float distance      = GetDistace(center , worldpos);
        float radius        = cullsphere.w;
        if (distance < radius)
        {
            break;
        }
    }
    return i;
}


ShadowStrengthCascadeData GetShadowStrengthCascadeData(Surface surface)
{
    ShadowStrengthCascadeData shadowStrengthcascadeData;
    ShadowDistaceData shadowDistanceData    = GetShadowDistaceData();
    float shadowStrengthFade                = GetFadeShadowStrength(surface, shadowDistanceData);
    const float3 worldPos                   = surface.worldPos;
    const int cascadeindex                  = GetCascadeIndex(worldPos);
    
    if (cascadeindex == _CascadeCount - 1)
    {
        ShadowCascadeData shadowCascadeData = GetShadowCascadeData();
        const float cascadeFade             = GetCascadeFadeStrength(shadowCascadeData, cascadeindex, worldPos);
        shadowStrengthFade                   *= cascadeFade;
    }
    shadowStrengthcascadeData.cascadeIndex          = cascadeindex;
    shadowStrengthcascadeData.shadowStrengthFade    = shadowStrengthFade;
    
    return shadowStrengthcascadeData;
}
