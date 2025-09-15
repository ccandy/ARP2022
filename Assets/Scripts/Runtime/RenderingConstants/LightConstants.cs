using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARP.Constant
{
    public static class LightConstants
    {
        public static readonly int MAX_DIRECTIONAL_LIGHTS = 4;
        
        public static readonly int DirectionalLightsDirId      = Shader.PropertyToID("_DirectionaLightsDir");
        public static readonly int DirectionalLightsColorId    = Shader.PropertyToID("_DirectionalLightsColor");
        public static readonly int DirectonalLightAccountId    = Shader.PropertyToID("_DirectionalLightCount");
        
        public static readonly string bufferName                = "LightBuffer";
    }
}
    
