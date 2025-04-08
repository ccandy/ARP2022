#pragma once



#define MAX_DIRECTIONS_LIGHTS 4

CBUFFER_START(UnityPerMaterial)
    float4 _DirectionaLightsDir[MAX_DIRECTIONS_LIGHTS];
    float4 _DirectionalLightsColor[MAX_DIRECTIONS_LIGHTS];
    float4 _Color;
    float4 _MainTex_ST;
    float _CutOff;
CBUFFER_END

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);



struct VertexInput
{
    float4 PositionOS : POSITION;
    float2 uv : TEXCOORD0;
};

struct VertexOutput
{
    float4 PositionCS : SV_POSITION;
    float2 uv : VAR_BASE_UV;
};

VertexOutput LitPassVertex( VertexInput input )
{
    VertexOutput output;

    float3 worldPos = TransformObjectToWorld(input.PositionOS.xyz);
    output.PositionCS = TransformWorldToHClip(worldPos);
    output.uv = TRANSFORM_TEX(input.uv,_MainTex);
    return output;
}


float4 LitPassFrag( VertexOutput input ) :SV_TARGET
{
    float4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    float4 color = _Color;
    float4 baseColor = baseMap * color;
    return baseColor;
}
