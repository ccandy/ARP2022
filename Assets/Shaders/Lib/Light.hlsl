#pragma once

#define MAX_DIRECTIONS_LIGHTS 4

CBUFFER_START(LightBuffer)
    float4 _DirectionaLightsDir[MAX_DIRECTIONS_LIGHTS];
    float4 _DirectionalLightsColor[MAX_DIRECTIONS_LIGHTS];
    float _directionalLightCount;
CBUFFER_END


struct Light
{
    half4 lightColor;
    float3 lightDirection;
    float3 lightPosition;
    float attenuation;
};

int GetDirectionalCount()
{
    return _directionalLightCount;
}

Light GetDirectionalLight(int index)
{
    Light light = (Light)0;

    light.attenuation = 1.0f;
    light.lightColor = _DirectionalLightsColor[index];
    light.lightDirection = _DirectionaLightsDir[index];

    return light;
}


