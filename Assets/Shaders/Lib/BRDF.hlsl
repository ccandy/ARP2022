#pragma once

struct BRDF
{
    float HdotV;
    float NdotL;
    float NdotV;

    float3 F0;
};


BRDF GetBRDF(Surface surface, Light light)
{
    BRDF brdf;

    half3 v = surface.viewDir;
    half3 l = light.lightDirection;
    half3 h = normalize(v + l);
    half3 n = surface.normal;
    half3 baseColor = surface.baseColor;
    half metallic = surface.metallic;
    
    half hdotv = dot(h, v);
    half ndotl = dot(n, l);
    half ndotv = dot(n, v);

    brdf.HdotV = hdotv;
    brdf.NdotL = ndotl;
    brdf.NdotV = ndotv;

    float3 F0 = lerp(float3(0.04, 0.04, 0.04), baseColor, metallic);
    brdf.F0 = F0;
    
    return brdf;
}




