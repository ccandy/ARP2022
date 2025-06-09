Shader "ARP/Lit"
{
    Properties
    {
        _MainTex ("Texture", 2D)                = "white" {}
        _Color("Color", Color)                  = (1,1,1,1)
        _Shininess("Shininess", float)          = 10
        _SpecularColor("Specular Color", Color) = (1,1,1,1)
        _Metallic("Metallic", Range(0, 1))      = 0.5
        _Roughness("Roughness", Range(0, 1))    = 0.5
        
        [MaterialToggle(ARP_BlinnPhong_ON)] ARP_BlinnPhong("BlinnPhong ON", Int) = 0
        [MaterialToggle(ARP_PBR_ON)] ARP_PBR("PBR ON", Int) = 0
        
        
    }
    SubShader
    {
        

        Pass
        {
            Tags
            {
                "RenderType"="Opaque"
                "LightMode" = "ARPLit"
            }
            LOD 100
            
            HLSLPROGRAM
            
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFrag
            #pragma shader_feature ARP_BlinnPhong_OFF ARP_BlinnPhong_ON
            #pragma shader_feature ARP_PBR_OFF ARP_PBR_ON
            
            #include "Lib/Common.hlsl"
            #include "Lib/Surface.hlsl"
            #include "Lib/Light.hlsl"
            #include "Lib/BRDF.hlsl"
            #include "Lib/Lighting.hlsl"
            #include "Lib/Lit.hlsl"

            
            ENDHLSL
        }
        
        Pass
        {
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
            
            ColorMask 0
            HLSLPROGRAM
            #include "Lib/Common.hlsl"
            
            #pragma vertex ShadowCasterVertex
            #pragma fragment ShadowCasterFrag
            #include "Lib/ShadowCaster.hlsl"
            
            ENDHLSL
            
        }


    }
}
