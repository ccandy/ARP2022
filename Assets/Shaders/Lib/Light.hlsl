#pragma once

#define MAX_DIRECTIONS_LIGHTS 4

CBUFFER_START(LightBuffer)
    float4 _DirectionaLightsDir[MAX_DIRECTIONS_LIGHTS];
    float4 _DirectionalLightsColor[MAX_DIRECTIONS_LIGHTS];
    float4 _DirectionalLightsData[MAX_DIRECTIONS_LIGHTS];
    int _DirectionalLightCount;
CBUFFER_END


struct Light
{
    half4 lightColor;
    float3 lightDirection;
    float3 lightPosition;
    float attenuation;
    float renderLayerMask;
    
};

int GetDirectionalCount()
{
    return _DirectionalLightCount;
}

Light GetDirectionalLight(int index)
{
    Light light = (Light)0;

    light.attenuation       = 1.0f;
    light.lightColor        = _DirectionalLightsColor[index];
    light.lightDirection    = normalize(_DirectionaLightsDir[index]);
    light.renderLayerMask    = _DirectionalLightsData[index].y;
    
    return light;
}


