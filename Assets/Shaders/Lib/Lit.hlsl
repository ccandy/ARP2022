#pragma once

#define MAX_DIRECTIONS_LIGHTS 4

CBUFFER_START(UnityPerMaterial)
    float4 _Color;
   
    float4 _MainTex_ST;
    float _CutOff;
    float4 _SpecularColor;
    float _Roughness;
    float _Metallic;
    float _Shininess;
CBUFFER_END

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

struct VertexInput
{
    float4 PositionOS   : POSITION;
    float2 uv           : TEXCOORD0;
    float3 Normal       : NORMAL;
};

struct VertexOutput
{
    float4 PositionCS   : SV_POSITION;
    float2 uv           : VAR_BASE_UV;
    float3 NormalWS     : NORMAL;
    float3 worldPos     : TEXCOORD0;
};

VertexOutput LitPassVertex( VertexInput input )
{
    VertexOutput output;

    float3 worldPos     = TransformObjectToWorld(input.PositionOS.xyz);
    output.PositionCS   = TransformWorldToHClip(worldPos);
    output.uv           = TRANSFORM_TEX(input.uv,_MainTex);
    output.NormalWS     = TransformObjectToWorldNormal(input.Normal);
    output.worldPos     = worldPos;
    return output;
}


float4 LitPassFrag( VertexOutput input ) :SV_TARGET
{
    float4 baseMap      = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    float4 color        = _Color;
    float4 baseColor    = baseMap * color;
    float3 worldPos     = input.worldPos;
    float3 normalWS     = normalize(input.NormalWS);
    Surface surface     = GetSurface(baseColor,normalWS, worldPos, _SpecularColor, _Shininess, _Roughness, _Metallic);
    half3 result        = GetIncomingLightsColors(surface);
     
    return half4(result, surface.alpha);
}
