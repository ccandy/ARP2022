#pragma once

struct Surface
{
    float4 baseColor;
    float3 normal;
};


Surface GetSurface(float baseColor, float3 normal)
{
    Surface s;

    s.baseColor = baseColor;
    s.normal = normal;

    return s;
}
