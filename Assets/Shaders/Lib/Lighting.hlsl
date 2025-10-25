#pragma once

bool LayerOverLay(int surfaceLayerMask, int lightLayerMask)
{
    return (surfaceLayerMask == lightLayerMask);
}


half3 GetPhongDiffuse(Surface surface, Light light)
{
    float3 normal   = surface.normal;
    float3 lightDir = light.lightDirection;

    float3 diffuse  = saturate(dot(normal, lightDir));

    return diffuse;
}


float3 GetPhongSpecular(Surface surface, Light light)
{
    float3 normal       = surface.normal;
    float3 viewDir      = surface.viewDir;
    float3 lightDir     = light.lightDirection;
    float3 reflectDir   = reflect(-lightDir, normal);
    float shininess     = surface.shininess;
    float3 specular     = pow(saturate(dot(viewDir, reflectDir)), shininess) * surface.specColor;
    
    return specular;
}

float3 GetBlinnPhongSpecular(Surface surface, Light light)
{
    float3 viewDir      = surface.viewDir;
    float3 lightDir     = light.lightDirection;
    float3 normal       = surface.normal;
    float3 h            = normalize(viewDir + lightDir);
    float shininess     = surface.shininess;
    
    float3 specular     = pow(saturate(dot(h, normal)), shininess) * surface.specColor;

    return specular;
}

half3 GetDirectionalLightsColor(Surface surface, ShadowStrengthCascadeData shadowStrengthData)
{
    half3 diffuse       = 0;
    half3 specular      = 0;

    half3 lightColor    = 0;
    
    int directonalCount = GetDirectionalLightCount();
    for(int i = 0; i < directonalCount; ++i)
    {
        Light light = GetDirectionalLight(i);

        #if defined(ARP_PBR_ON)
            BRDF brdf   = GetBRDF(surface, light);
            diffuse     = brdf.Diffuse;
            specular    = brdf.Specular;
        #else
            diffuse = GetPhongDiffuse(surface, light);
            #if defined(ARP_BlinnPhong_ON)
                specular = GetBlinnPhongSpecular(surface, light);
            #else
                specular = GetPhongSpecular(surface, light);
            #endif
        #endif

        half shadowAtten        = GetDirectionalShadowAtten(i, surface,shadowStrengthData);
        half3 lightIntensity    = light.lightColor * shadowAtten;
        half3 finalCol          = (diffuse * surface.baseColor + specular) * lightIntensity;
        lightColor              += finalCol;
    }

    int additionalLightCount = GetAdditionalLightCount();

    for (int j = 0; j < additionalLightCount; ++j)
    {
        Light light                     = GetAdditionalLight(j, surface);
        diffuse                         = GetPhongDiffuse(surface, light);
        const half3 lightIntensity      = light.lightColor;
        const half lightAtten           = light.attenuation;
        const half3 finalCol            = diffuse * surface.baseColor * lightIntensity * lightAtten;
        lightColor                      += finalCol;
    }
    
    
    return lightColor;
}


half3 GetIncomingLightsColors(Surface surface, ShadowStrengthCascadeData shadowStrengthData)
{
    half3 res                       = 0;
    half3 directionallightColor     = GetDirectionalLightsColor(surface,shadowStrengthData);
    res += directionallightColor;
    return res;
}



