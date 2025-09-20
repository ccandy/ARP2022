#pragma once

#define MAX_DIRECTIONS_SHADOW_LIGHTS 4
#define MAX_DIRECTIONS_CASCADES 4

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"

CBUFFER_START(ShadowBuffer)
    float4      _DirectionalShadowDatas[MAX_DIRECTIONS_SHADOW_LIGHTS];
    float4x4    _ShadowToWorldCascadeMat[MAX_DIRECTIONS_SHADOW_LIGHTS * MAX_DIRECTIONS_CASCADES];
    float4      _CullSpherePos[MAX_DIRECTIONS_CASCADES];
    float4      _CullSphereData[MAX_DIRECTIONS_CASCADES];
    int         _CascadeCount;
    float4      _ShadowMapTexelSize;
    float4      _ShadowDistanceFade;
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

TEXTURE2D_SHADOW(_CascadeShadowMap);
SAMPLER_CMP(sampler_CascadeShadowMap);

float GetDistace(float3 pa, float3 pb)
{
    return dot(pa - pb,pa - pb);
}

float GetFadeShadowStrength(float distance, float scale, float fade)
{
    return saturate((1.0 - distance * scale) * fade);
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

struct DirectionalShadowData
{
    float strength;
    float normalbias;
    int enableSoftShadow;

    int CascadeIndex;
};

DirectionalShadowData GetDirectionalShadowData(int index,Surface surface)
{
    float4 shadowdata           = _DirectionalShadowDatas[index];
    
    DirectionalShadowData data  = (DirectionalShadowData) 0;
    data.strength               = shadowdata.x;
    data.normalbias             = shadowdata.y;
    data.enableSoftShadow       = asint(shadowdata.z);

    const float3 worldpos       = surface.worldPos;
    data.CascadeIndex           = GetCascadeIndex(worldpos);
    
    return data;
}

half SampleCascadeShadowmap(float3 shadowpos, int enableSoftShadow)
{
    if (enableSoftShadow == 0)
    {
        return SAMPLE_TEXTURE2D_SHADOW(_CascadeShadowMap, sampler_CascadeShadowMap, shadowpos);
    }else
    {
        float fetchesWeights[FLITER_SIZE];
        float2 fetchesUV[FLITER_SIZE];
        
        SOFTSHDADOW_COMPUTESAMPLES_TENT(_ShadowMapTexelSize.yyxx,shadowpos.xy, fetchesWeights,fetchesUV);
        half shadow = 0;
        for (int n = 0; n < FLITER_SIZE; n++)
        {
            float2 pos = fetchesUV[n];
            shadow += fetchesWeights[n] * SAMPLE_TEXTURE2D_SHADOW(_CascadeShadowMap,
                sampler_CascadeShadowMap, float3(pos.x, pos.y, shadowpos.z));
        }

        return shadow;
    }
}

half GetDirectionalShadowAtten(int lightindex, Surface surface)
{
    if (lightindex >= MAX_DIRECTIONS_SHADOW_LIGHTS)
    {
        return 0;
    }
    
    DirectionalShadowData dirShadowData = GetDirectionalShadowData(lightindex, surface);
    const int cascadeindex              = dirShadowData.CascadeIndex;
    
    int tileindex                       = lightindex * _CascadeCount + cascadeindex;
    float4x4 shadowToWorldCascadeMat    = _ShadowToWorldCascadeMat[tileindex];

    const float3 worldpos               = surface.worldPos;
    const float3 worldnormal            = surface.normal;

    const float texelSize               = _CullSphereData[cascadeindex];
    const float normalBias              = dirShadowData.normalbias * texelSize;
    
    const float3 bias                   = normalBias * worldnormal;
    const int enableSoftShadow          = dirShadowData.enableSoftShadow;
    
    float4 shadowPos                    = mul(shadowToWorldCascadeMat,float4(worldpos + bias,1));
    shadowPos.xyz                       /= shadowPos.w;
    half shadowAtten                    = SampleCascadeShadowmap(shadowPos.xyz, enableSoftShadow);
    half shadowStrength                 = lerp(0, dirShadowData.strength,(cascadeindex < MAX_DIRECTIONS_CASCADES));
    
    return lerp(1 , shadowAtten, shadowStrength);
    
}