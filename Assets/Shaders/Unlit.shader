Shader "ARP/Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFrag
            #include "Lib/Common.hlsl"
            #include "Lib/Unlit.hlsl"

            
            ENDHLSL
        }
    }
}
