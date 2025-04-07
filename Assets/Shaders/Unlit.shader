Shader "ARP/Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", color) = (1,1,1,1)
        _CutOff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [Toggle(_CLIPPING)]  _Clipping("Alpha Clipping", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM

            #pragma shader_feature _CLIPPING
            
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFrag
            #include "Lib/Common.hlsl"
            #include "Lib/Unlit.hlsl"

            
            ENDHLSL
        }
    }
}
