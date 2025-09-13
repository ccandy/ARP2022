#pragma once

float4 _Color;

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

float4 _MainTex_ST;
float _CutOff;

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

VertexOutput ShadowCasterVertex( VertexInput input )
{
    VertexOutput output;

    float3 worldPos         = TransformObjectToWorld(input.PositionOS.xyz);
    output.PositionCS       = TransformWorldToHClip(worldPos);
    output.uv               = TRANSFORM_TEX(input.uv,_MainTex);
    return output;
}


float4 ShadowCasterFrag( VertexOutput input ) :SV_TARGET
{
    float a = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a * _Color.a;
    clip(a - _CutOff);             
    return 0;                  
}
