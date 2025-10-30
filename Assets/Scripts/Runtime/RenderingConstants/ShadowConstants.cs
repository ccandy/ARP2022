using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARP.Constant
{
    public static class ShadowConstants 
    {
        public readonly static int MAX_DIRECTIONS_SHADOW_LIGHTS             = 4;
        public readonly static int MAX_CASACDE_COUNT                        = 4;
        public readonly static int MAX_CASCADE_SHDAOW_DATA_COUNT            = MAX_DIRECTIONS_SHADOW_LIGHTS * MAX_CASACDE_COUNT;
        
        public readonly static int MAX_SPOT_SHADOW_LIGHTS                   = 12;
        
        public readonly static int ShadowToWorldCascadeMatID                = Shader.PropertyToID("_ShadowToWorldCascadeMat");
        public readonly static int DirectionalShadowDatasID                 = Shader.PropertyToID("_DirectionalShadowDatas");
        public readonly static int CullSpherePosID                          = Shader.PropertyToID("_CullSpherePos");
        public readonly static int CullSphereDataID                         = Shader.PropertyToID("_CullSphereData");
        public readonly static int CascadeShadowMapID                       = Shader.PropertyToID("_CascadeShadowMap");
        public readonly static int CascadeCountID                           = Shader.PropertyToID("_CascadeCount");
        public readonly static int CascadeDataID                            = Shader.PropertyToID("_CascadeData");
        
        public readonly static int CascadeShadowMapTexelSizeID              = Shader.PropertyToID("_CascadeShadowMapTexelSize");
        public readonly static int ShadowDistanceDataID                     = Shader.PropertyToID("_ShadowDistanceData");
        
        
        public readonly static int ShadowMapID                              = Shader.PropertyToID("_ShadowMap");
        public readonly static int ShadowToWorldMatID                       = Shader.PropertyToID("_ShadowToWorldMat");
        public readonly static int AdditiaonlShadowDatasID                  = Shader.PropertyToID("_AdditiaonlShadowDatas");
        public readonly static int ShadowMapTexelSizeID                     = Shader.PropertyToID("_ShadowMapTexelSize");

    }
}

