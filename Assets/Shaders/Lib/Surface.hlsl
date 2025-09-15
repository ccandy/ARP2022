#pragma once

struct Surface
{
    float3 baseColor;
    float3 normal;
    float3 viewDir;
    float3 specColor;
    float3 worldPos;
    float alpha;
    float shininess;
    float roughness;
    float metallic;
    float renderLayerMask;
}; 

Surface GetSurface(float4 baseColor, float3 normal, float3 worldPos, float3 specColor,
    float shininess, float roughness, float metallic, int renderLayerMask)
{
    Surface s;

    s.baseColor         = baseColor;
    s.normal            = normal;
    s.alpha             = baseColor.a;
    s.viewDir           = normalize(_WorldSpaceCameraPos - worldPos);
    s.shininess         = shininess;
    s.specColor         = specColor;
    s.metallic          = metallic;
    s.roughness         = roughness;
    s.worldPos          = worldPos;
    s.renderLayerMask   = renderLayerMask;
    return s;
}


