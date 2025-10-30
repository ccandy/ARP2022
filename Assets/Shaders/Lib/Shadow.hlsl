#pragma once

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"
#include "Lib/ARPShadowDatas.hlsl"

half SampleShadowmap(
    TEXTURE2D_SHADOW_PARAM(shadowTex, shadowSampler),float3 shadowpos,float4 shadowMapTexelSize,      
    int enableSoftShadow)
{
    if (enableSoftShadow == 0) 
    {
       return SAMPLE_TEXTURE2D_SHADOW(shadowTex, shadowSampler, shadowpos);
    }
    else
    {
        float fetchesWeights[FLITER_SIZE];
        float2 fetchesUV[FLITER_SIZE];
        
        SOFTSHDADOW_COMPUTESAMPLES_TENT(shadowMapTexelSize.yyxx,shadowpos.xy, fetchesWeights,fetchesUV);
        half shadow = 0;
        for (int n = 0; n < FLITER_SIZE; n++)
        {
            float2 pos = fetchesUV[n];
            shadow += fetchesWeights[n] * SAMPLE_TEXTURE2D_SHADOW(shadowTex,
                shadowSampler, float3(pos.x, pos.y, shadowpos.z));
        }

        return shadow;
    }
}


half GetDirectionalShadowAtten(int lightindex, Surface surface, ShadowStrengthCascadeData shadowStrengthCascadeData)
{
    if (lightindex >= MAX_DIRECTIONS_SHADOW_LIGHTS)
    {
        return 0;
    }

    const int cascadeIndex              = shadowStrengthCascadeData.cascadeIndex;
    
    ShadowData dirShadowData            = GetDirectionalShadowData(lightindex);
    int tileindex                       = lightindex * _CascadeCount + cascadeIndex;
    float4x4 shadowToWorldCascadeMat    = _ShadowToWorldCascadeMat[tileindex];

    const float3 worldpos               = surface.worldPos;
    const float3 worldnormal            = surface.normal;

    const float texelSize               = _CullSphereData[cascadeIndex];
    const float normalBias              = dirShadowData.normalbias * texelSize;
    
    const float3 bias                   = normalBias * worldnormal;
    const int enableSoftShadow          = dirShadowData.enableSoftShadow;
    
    float4 shadowPos                    = mul(shadowToWorldCascadeMat,float4(worldpos + bias,1));
    shadowPos.xyz                       /= shadowPos.w;
    half shadowAtten                    = SampleShadowmap(TEXTURE2D_ARGS(_CascadeShadowMap, sampler_CascadeShadowMap), shadowPos.xyz, _CascadeShadowMapTexelSize, enableSoftShadow);
    half shadowStrength                 = lerp(0, dirShadowData.strength,(cascadeIndex < MAX_DIRECTIONS_CASCADES));
    shadowStrength                      *= shadowStrengthCascadeData.shadowStrengthFade;
    
    return lerp(1 , shadowAtten, shadowStrength);
    
}

half GetAdditionalShadowAtten(int lightindex, Surface surface)
{
    if (lightindex >= MAX_DIRECTIONS_SHADOW_LIGHTS)
    {
        return 0;
    }

    const float3 worldpos               = surface.worldPos;
    const float3 worldnormal            = surface.normal;
    
    ShadowData shadowData               = GetAdditionalShadowData(lightindex);
    float4x4 shadowToWorldCascadeMat    = _ShadowToWorldMat[lightindex];
    
    const float normalBias              = shadowData.normalbias;
    const float3 bias                   = normalBias * worldnormal;
    const int enableSoftShadow          = shadowData.enableSoftShadow;

    float4 shadowPos                    = mul(shadowToWorldCascadeMat,float4(worldpos + bias,1));
    shadowPos.xyz                       /= shadowPos.w;

    const half shadowAtten              = SampleShadowmap(TEXTURE2D_ARGS(_ShadowMap, sampler_ShadowMap), shadowPos.xyz, _ShadowMapTexelSize, 1);
    const half shadowStrength           = shadowData.strength;
    
    return lerp(1, shadowAtten,shadowStrength);
    
}