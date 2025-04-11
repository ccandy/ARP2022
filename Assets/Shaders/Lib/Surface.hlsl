#pragma once

struct Surface
{
    float3 baseColor;
    float3 normal;
    float alpha;
};


Surface GetSurface(float4 baseColor, float3 normal)
{
    Surface s;

    s.baseColor = baseColor;
    s.normal = normal;
    s.alpha = baseColor.a;

    return s;
}
