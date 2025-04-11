Shader "ARP/Lit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _Shininess("Shininess", float) = 10
        _SpecularColor("Specular Color", Color) = (1,1,1,1)
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
            #include "Lib/Surface.hlsl"
            #include "Lib/Light.hlsl"
            #include "Lib/Lighting.hlsl"
            #include "Lib/Lit.hlsl"

            
            ENDHLSL
        }
    }
}
