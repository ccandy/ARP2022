using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARP.Constant
{
    public static class LightConstants
    {
        public static readonly int MAX_DIRECTIONAL_LIGHTS   = 2;
        public static readonly int MAX_SPOT_LIGHTS          = 8;

        public static readonly int MAX_ADDITIONAL_LIGHTS    = MAX_SPOT_LIGHTS;
        
        
        public static readonly int DirectionalLightsDirId       = Shader.PropertyToID("_DirectionaLightsDir");
        public static readonly int DirectionalLightsColorId     = Shader.PropertyToID("_DirectionalLightsColor");
        public static readonly int DirectonalLightAccountId     = Shader.PropertyToID("_DirectionalLightCount");
        public static readonly int DirectionalLightsDataId      = Shader.PropertyToID("_DirectionalLightsData");
        
        public static readonly int AdditionalLightsPosId        = Shader.PropertyToID("_AdditionalLightsPos");
        public static readonly int AdditionalLightsColorId      = Shader.PropertyToID("_AdditionalLightsColor");
        public static readonly int AdditionalLightAccountId     = Shader.PropertyToID("_AdditionalLightCount");
        public static readonly int AdditionalLightsDataId       = Shader.PropertyToID("_AdditionalLightsData");
        public static readonly int AdditionalLightsAxisId        = Shader.PropertyToID("_AdditionalLightsAxis");
        
        
        public static readonly string bufferName                = "LightBuffer";
    }
}
    
