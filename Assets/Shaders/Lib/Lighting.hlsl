#pragma once


half3 GetPhongDiffuse(Surface surface, Light light)
{
    float3 normal = surface.normal;
    float3 lightDir = light.lightDirection;

    float3 diffuse = saturate(dot(normal, lightDir));

    return diffuse;
}


float3 GetPhongSpecular(Surface surface, Light light)
{
    float3 normal = surface.normal;
    float3 viewDir = surface.viewDir;
    float3 lightDir = light.lightDirection;
    float3 reflectDir = reflect(-lightDir, normal);
    float shinness = surface.shininess;
    float3 specular = pow(saturate(dot(viewDir, reflectDir)), shinness) * surface.specColor;
    
    return specular;
}

half3 GetIncomingLightsColors(Surface surface)
{
    int directonalCount = GetDirectionalCount();
    half3 res = 0;
    for (int n = 0; n < directonalCount; ++n)
    {
        Light light = GetDirectionalLight(n);
        half3 diffuse = GetPhongDiffuse(surface, light);
        half3 specular = GetPhongSpecular(surface, light);

        half3 lightColor = (diffuse + specular) * light.lightColor * surface.baseColor;
        
        res += lightColor;
    }
    return res;
}



