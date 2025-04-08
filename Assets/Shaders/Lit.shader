Shader "ARP/Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "LightMode" = "ARPLit"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFrag
            #include "Lib/Common.hlsl"
            #include "Lib/Lit.hlsl"

            
            ENDHLSL
        }
    }
}
