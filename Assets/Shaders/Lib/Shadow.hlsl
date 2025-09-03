#pragma once

#define MAX_DIRECTIONS_SHADOW_LIGHTS 4
#define MAX_DIRECTIONS_CASCADES 4

CBUFFER_START(ShadowBuffer)
    float4 _DirectionalShadowDatas[MAX_DIRECTIONS_SHADOW_LIGHTS];
    float4 _ShadowToWorldCascadeMat[MAX_DIRECTIONS_SHADOW_LIGHTS];
    float4 _CullSphereDatas[MAX_DIRECTIONS_CASCADES];
    int _CascadeCount;
CBUFFER_END


TEXTURE2D(_CascadeShadowMap);
SAMPLER(sampler_CascadeShadowMap);

half SampleCascadeShadowmap(float3 worldpos)
{
    return SAMPLE_TEXTURE2D_SHADOW(_CascadeShadowMap, sampler_CascadeShadowMap, worldpos);
}

half GetDirectionalShadowAtten(int lightindex)
{
    if (lightindex >= MAX_DIRECTIONS_SHADOW_LIGHTS)
    {
        return 0;
    }
    float4 dirShadowData = _DirectionalShadowDatas[lightindex];
    
}




