#pragma once

float4 _Color;

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

float4 MainTex_ST;

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

VertexOutput UnlitPassVertex( VertexInput input )
{
    VertexOutput output;

    float3 worldPos = TransformObjectToWorld(input.PositionOS.xyz);
    output.PositionCS = TransformWorldToHClip(worldPos);
    output.uv = TRANSFORM_TEX(input.uv,MainTex);
    

    return output;
}


float4 UnlitPassFrag( VertexOutput input ) :SV_TARGET
{
    float4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    float4 color = _Color;
    float4 baseColor = baseMap * color;
    
    return baseColor;
}


