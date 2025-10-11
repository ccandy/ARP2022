#pragma once

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"
#include "Lib/ARPShadowDatas.hlsl"


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

float GetDistanceFadeStrength(float depth, float oneovershadowDistance, float oneoverfade)
{
    float temp = 1 - depth  * oneovershadowDistance;
    return saturate(temp * oneoverfade);
}

float GetDistace(float3 pa, float3 pb)
{
    return dot(pa - pb,pa - pb);
}


float GetFadeShadowStrength(Surface surface)
{
    half oneOverShadowDistance          = _ShadowDistanceData.x;
    half oneOverShadowDistanceFade      = _ShadowDistanceData.y;
    half depth                          = surface.depth;
    
    return saturate((1.0 - depth * oneOverShadowDistance) * oneOverShadowDistanceFade);
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

    const int cascadeIndex              = GetCascadeIndex(surface.worldPos);
    
    DirectionalShadowData dirShadowData = GetDirectionalShadowData(lightindex, cascadeIndex);
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
    
    float shadowStrengthFade            = GetFadeShadowStrength(surface);
    shadowStrength                      *= shadowStrengthFade;

    if (cascadeindex == _CascadeCount - 1)
    {
        float shadowDistanceSqr = _ShadowDistanceData;
    }
    
    
    return lerp(1 , shadowAtten, shadowStrength);
    
}