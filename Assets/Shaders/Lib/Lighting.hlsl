#pragma once

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


half3 GetIncomingLightsColors(Surface surface)
{
    int directonalCount = GetDirectionalCount();
    half3 res           = 0;
    half3 diffuse       = 0;
    half3 specular      = 0;
    
    for (int n = 0; n < directonalCount; ++n)
    {
        Light light = GetDirectionalLight(n);
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
        half3 lightColor = (diffuse + specular) * light.lightColor * surface.baseColor;
        res += lightColor;
    }
    return res;
}



