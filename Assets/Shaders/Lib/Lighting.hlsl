#pragma once


half3 GetPhongDiffuse(Surface surface, Light light)
{
    float3 normal = surface.normal;
    float3 lightDir = light.lightDirection;

    float3 diffuse = saturate(dot(normal, lightDir));

    return diffuse * surface.baseColor * light.lightColor;
}


half3 GetIncomingLightsColors(Surface surface)
{
    int directonalCount = GetDirectionalCount();
    half3 res = 0;
    for (int n = 0; n < directonalCount; ++n)
    {
        Light light = GetDirectionalLight(n);
        half3 lightColor = GetPhongDiffuse(surface, light);
        res += lightColor;
    }
    return res;
}

