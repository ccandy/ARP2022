#pragma once

struct Surface
{
    float3 baseColor;
    float3 normal;
    float3 viewDir;
    float3 specColor;
    float alpha;
    float shininess;
    float roughness;
    float metallic;
};


Surface GetSurface(float4 baseColor, float3 normal, float3 worldPos, float3 specColor, float shininess, float roughness, float metallic)
{
    Surface s;

    s.baseColor     = baseColor;
    s.normal        = normal;
    s.alpha         = baseColor.a;
    s.viewDir       = normalize(_WorldSpaceCameraPos - worldPos);
    s.shininess     = shininess;
    s.specColor     = specColor;
    s.metallic      = metallic;
    s.roughness     = roughness;
    
    return s;
}


