#pragma once

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"
#include "Lib/ARPShadowDatas.hlsl"

float GetDistanceFadeStrength(float depth, float oneovershadowDistance, float oneoverfade)
{
    float temp = 1 - depth  * oneovershadowDistance;
    return saturate(temp * oneoverfade);
}

float GetDistace(float3 pa, float3 pb)
{
    return dot(pa - pb,pa - pb);
}

float GetShadowFadeStrength(float distance, float scale, float fade)
{
    return saturate((1.0 - distance * scale) * fade);
}


float GetFadeShadowStrength(Surface surface, ShadowDistaceData shadowDistaceData)
{
    half oneOverShadowDistance          = shadowDistaceData.OneOverShadowDistance;
    half oneOverShadowDistanceFade      = shadowDistaceData.OneOverShadowDistanceFade;
    half depth                          = surface.depth;

    return GetShadowFadeStrength(depth, oneOverShadowDistance,oneOverShadowDistanceFade);
}

float GetCascadeFadeStrength(ShadowCascadeData cascadedata, ShadowDistaceData distacedata)
{
    const float shadowDistanceSqr       = distacedata.ShadowDistanceSqr;
    const float CascadeFadeRadius       = cascadedata.CascadeFadeRadius;
    const float CascadeFadeScale        = cascadedata.CascadeFadeScale;

    return GetShadowFadeStrength(shadowDistanceSqr, CascadeFadeRadius,CascadeFadeScale);
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

    ShadowDistaceData shadowDistanceData    =  GetShadowDistaceData();
    const float shadowStrengthFade          = GetFadeShadowStrength(surface, shadowDistanceData);
    shadowStrength                          *= shadowStrengthFade;
    
    if (cascadeindex == _CascadeCount - 1)
    {
        ShadowCascadeData shadowCascadeData = GetShadowCascadeData();
        const float cascadeFade             = GetCascadeFadeStrength(shadowCascadeData, shadowDistanceData);
        shadowStrength                      *= cascadeFade;
    }
    
    return lerp(1 , shadowAtten, shadowStrength);
    
}