#pragma once

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"

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

half GetDirectionalShadowAtten(int lightindex, Surface surface, ShadowStrengthCascadeData shadowStrengthCascadeData)
{
    if (lightindex >= MAX_DIRECTIONS_SHADOW_LIGHTS)
    {
        return 0;
    }

    const int cascadeIndex              = surface.cascadeIndex;
    
    DirectionalShadowData dirShadowData = GetDirectionalShadowData(lightindex);
    const int cascadeindex              = shadowStrengthCascadeData.cascadeIndex;
    
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
    shadowStrength                      *= shadowStrengthCascadeData.shadowStrengthFade;
    
    return lerp(1 , shadowAtten, shadowStrength);
    
}