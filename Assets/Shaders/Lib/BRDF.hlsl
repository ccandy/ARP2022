#pragma once

struct BRDF
{
    float HdotV;
    float NdotL;
    float NdotV;

    float3 F0;
    float3 F;
    float3 Diffuse;
    float3 Specular;
};

float3 FresnelSchlick(float hdotv, float3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - hdotv, 5.0);
}

float GGX_D(float roughness, float NdotH)
{
    float alpha     = roughness * roughness;
    float alpha2    = alpha * alpha;
    
    float NdotH2    = NdotH * NdotH;

    float denom     = (NdotH2 * (alpha2 - 1.0) + 1.0);
    denom           = PI * denom * denom;
    
    return alpha2 / max(denom, 1e-5);
}

float GGX_Schlick(float roughness, float NdotV)
{
    float r     = roughness + 1.0;
    float k     = (r * r) / 8.0;

    return NdotV / (NdotV * (1.0 - k) + k + 1e-5);
}

float GeometrySmithGGX(float NdotV, float NdotL, float roughness)
{
    
    float ggx1  = GGX_Schlick(roughness, NdotV);
    float ggx2  = GGX_Schlick(roughness, NdotL);

    return ggx1 * ggx2;
}

float3 CookTorranceSpec(float3 F, float NdotV, float NdotL, float NdotH, float roughness)
{
    float D         = GGX_D(roughness, NdotH);
    float G         = GeometrySmithGGX(NdotV, NdotL, roughness);
    float denom     = max(4.0 * NdotV * NdotL, 1e-5);
    float3 spec     = (D * F * G) / denom;

    return spec;
}

float3 DiffuseLambert(float3 albedo, float3 F0, float metallic, float NdotL)
{
    float3 kD = (1.0 - metallic) * (1.0 - F0);
    
    return (kD * albedo / PI) * NdotL;
}

BRDF GetBRDF(Surface surface, Light light)
{
    BRDF brdf;

    half3 v             = normalize(surface.viewDir);
    half3 l             = normalize(light.lightDirection);
    half3 h             = normalize(v + l);
    half3 n             = normalize(surface.normal);
    half3 baseColor     = surface.baseColor;
    half metallic       = surface.metallic;
    half roughness      = surface.roughness * surface.roughness;
    
    half hdotv = saturate(dot(h, v));
    half ndotl = saturate(dot(n, l));
    half ndotv = saturate(dot(n, v));
    half ndotH = saturate(dot(n, h));

    brdf.HdotV = hdotv;
    brdf.NdotL = ndotl;
    brdf.NdotV = ndotv;

    float3 F0       = lerp(float3(0.04, 0.04, 0.04), baseColor, metallic);
    brdf.F0         = F0;
    half3 F         = FresnelSchlick(hdotv, F0);
    brdf.F          = F;
    brdf.Diffuse    = DiffuseLambert(baseColor, F0, metallic, ndotl);
    brdf.Specular   = CookTorranceSpec(F,ndotv,ndotl, ndotH, roughness);
    
    return brdf;
}





