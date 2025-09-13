#pragma once

#define MAX_DIRECTIONS_SHADOW_LIGHTS 4
#define MAX_DIRECTIONS_CASCADES 4

CBUFFER_START(ShadowBuffer)
    float4      _DirectionalShadowDatas[MAX_DIRECTIONS_SHADOW_LIGHTS];
    float4x4    _ShadowToWorldCascadeMat[MAX_DIRECTIONS_SHADOW_LIGHTS * MAX_DIRECTIONS_CASCADES];
    float4      _CullSphereDatas[MAX_DIRECTIONS_CASCADES];
    int         _CascadeCount;
CBUFFER_END

TEXTURE2D_SHADOW(_CascadeShadowMap);
SAMPLER_CMP(sampler_CascadeShadowMap);

float GetDistace(float3 pa, float3 pb)
{
    return dot(pa - pb,pa - pb);
}


int GetCascadeIndex(float3 worldpos)
{
    int i = 0;
    for (; i < _CascadeCount; i++)
    {
        float4 cullsphere   = _CullSphereDatas[i];
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
    float ShadowNearPlane;

    int CascadeIndex;
};

DirectionalShadowData GetDirectionalShadowData(int index,Surface surface)
{

    float4 shadowdata           = _DirectionalShadowDatas[index];
    
    DirectionalShadowData data  = (DirectionalShadowData) 0;
    data.strength               = shadowdata.x;
    data.normalbias             = shadowdata.y;
    data.ShadowNearPlane        = shadowdata.z;

    const float3 worldpos       = surface.worldPos;
    data.CascadeIndex           = GetCascadeIndex(worldpos);
    
    return data;
}


half SampleCascadeShadowmap(float3 shadowpos)
{
    return SAMPLE_TEXTURE2D_SHADOW(_CascadeShadowMap, sampler_CascadeShadowMap, shadowpos);
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
    float4 shadowPos                    = mul(shadowToWorldCascadeMat,float4(worldpos,1));
    shadowPos.xyz                       /= shadowPos.w;
    half shadowAtten                    = SampleCascadeShadowmap(shadowPos.xyz);
    half shadowStrength                 = dirShadowData.strength;
    
    return lerp(1 , shadowAtten, shadowStrength);
    
}