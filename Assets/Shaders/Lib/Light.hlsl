#pragma once

#define MAX_DIRECTIONS_LIGHTS 4
#define MAX_SPOT_LIGHTS 8
#define MAX_ADDITION_LIGHTS 8

CBUFFER_START(LightBuffer)
    float4 _DirectionaLightsDir[MAX_DIRECTIONS_LIGHTS];
    float4 _DirectionalLightsColor[MAX_DIRECTIONS_LIGHTS];
    float4 _DirectionalLightsData[MAX_DIRECTIONS_LIGHTS];
    int _DirectionalLightCount;

    float4 _AdditionalLightsPos[MAX_ADDITION_LIGHTS];
    float4 _AdditionalLightsColor[MAX_ADDITION_LIGHTS];
    float4 _AdditionalLightsData[MAX_ADDITION_LIGHTS];
    float4 _AdditionalLightsAxis[MAX_ADDITION_LIGHTS];

    int _AdditionalLightCount;

CBUFFER_END


struct Light
{
    half4 lightColor;
    float3 lightDirection;
    float3 lightPosition;
    
    float attenuation;
    float range;
    
    int renderLayerMask;
    
};

int GetDirectionalLightCount()
{
    return _DirectionalLightCount;
}

int GetAdditionalLightCount()
{
    return _AdditionalLightCount;
}

float GetRangeAtten(float distanceSqr, float range)
{
   return POW2(saturate(1.0 - POW2(distanceSqr * range)));
}

float GetSpotAtten(float3 lightdir, float3 lightAxis)
{
    return saturate(dot(lightdir, lightAxis));
}


Light GetDirectionalLight(int index)
{
    Light light = (Light)0;

    light.attenuation       = 1.0f;
    light.lightColor        = _DirectionalLightsColor[index];
    light.lightDirection    = normalize(_DirectionaLightsDir[index]);
    light.renderLayerMask    = asint(_DirectionalLightsData[index].y);
    
    return light;
}

Light GetAdditionalLight(int index, Surface surface)
{
    Light light = (Light)0;

    light.lightColor            = _AdditionalLightsColor[index];
    light.lightPosition         = _AdditionalLightsPos[index].xyz;

    float3 worldPos             = surface.worldPos;
    float3 lightPos             = light.lightPosition;
    
    float3 lightVector          = worldPos - lightPos;

    const float3 lightDirection = normalize(lightVector);
    
    light.lightDirection        = lightDirection;

    float distanceSqr           = max(dot(lightVector,lightVector), 0.0001f);
    
    float4 lightData            = _AdditionalLightsData[index];
    float range                 = lightData.y;

    const float3 lightAxis      = _AdditionalLightsAxis[index];
    float rangeAtten            = GetRangeAtten(distanceSqr, range);
    float spotAtten             = GetSpotAtten(lightDirection, lightAxis);
    
    
    return light;
}


