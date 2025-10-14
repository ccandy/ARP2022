using System.Collections;
using System.Collections.Generic;
using ARP.Constant;
using UnityEngine;
using UnityEngine.Rendering;

namespace ARP.Render
{
    public class AdditionalLightRender : LightRenderInterface
    {
        // Start is called before the first frame update
    
        private AdditionalLightData [] _additionalLightDatas    = new AdditionalLightData[LightConstants.MAX_DIRECTIONAL_LIGHTS];
        private int additionalLightCount;
        public void ConfigurelLightData(VisibleLight visibleLight)
        {
            if (visibleLight == null)
            {
                return;
            }
        
            AdditionalLightData additionalData          = new AdditionalLightData();
            additionalData.LightPosition                = visibleLight.localToWorldMatrix.GetColumn(3);
            additionalData.LightAxis                    = -visibleLight.localToWorldMatrix.GetColumn(2);
            additionalData.LightColor                   = visibleLight.finalColor;
            additionalData.AdditionalLightType          = visibleLight.lightType; 
            
            float range                                 = visibleLight.range;
            additionalData.LightRange                   = 1.0f / Mathf.Max(range * range, 0.0001f);
            additionalData.LightSpotAngle               = visibleLight.spotAngle;
            
            float innerCos                              = Mathf.Cos(Mathf.Deg2Rad * 0.5f * visibleLight.light.innerSpotAngle);
            float outerCos                              = Mathf.Cos(Mathf.Deg2Rad * 0.5f * visibleLight.spotAngle);
            float angleRangeInv                         = 1f / Mathf.Max(innerCos - outerCos, 0.001f);
            additionalData.AngleRangeInv                = angleRangeInv;
            additionalData.OutterCos                    = -outerCos * angleRangeInv;
            
            _additionalLightDatas[additionalLightCount] = additionalData;
            
            additionalLightCount++;
        }

        public void Init()
        {
            additionalLightCount = 0;
        }

        public void CleanUp()
        {
            System.Array.Clear(_additionalLightDatas, 0, _additionalLightDatas.Length);
            additionalLightCount = 0;
        }

        public void SendToGPU(ref ScriptableRenderContext context, CommandBuffer cmd)
        {
            if (additionalLightCount == 0)
            {
                cmd.SetGlobalInt(LightConstants.AdditionalLightAccountId, additionalLightCount);
            }
            else
            {
                Vector4[] additionalLightColor       = new Vector4[additionalLightCount];
                Vector4[] additionalLightPosition    = new Vector4[additionalLightCount];
                Vector4[] additionalLightAxis        = new Vector4[additionalLightCount];
                Vector4[] additionalightData         = new Vector4[additionalLightCount];
                Vector4[] SpotAnglesData             = new Vector4[additionalLightCount];
            
                for (int i = 0; i < additionalLightCount; ++i)
                {
                    AdditionalLightData additionalLightData     = _additionalLightDatas[i];
                
                    additionalLightColor[i]                  = additionalLightData.LightColor;
                    additionalLightPosition[i]                  = additionalLightData.LightPosition;
                    additionalLightAxis[i]                      = additionalLightData.LightAxis;

                    Vector4 lightData                           = new Vector4();
                    lightData.x                                 = additionalLightData.LightRange;
                    lightData.y                                 = (int)additionalLightData.AdditionalLightType;

                    additionalightData[i]                       = lightData;
                    
                    Vector4 SpotAngles                          = new Vector4();
                    SpotAngles.x                                = additionalLightData.AngleRangeInv;
                    float outerCos                              = additionalLightData.OutterCos;
                    SpotAngles.y                                = -outerCos * additionalLightData.AngleRangeInv;
                    SpotAngles.z                                = additionalLightData.LightSpotAngle;

                    SpotAnglesData[i]                           = SpotAngles;
                    
                }
                cmd.SetGlobalVectorArray(LightConstants.AdditionalLightsColorId, additionalLightColor);
                cmd.SetGlobalVectorArray(LightConstants.AdditionalLightsDataId, additionalightData);
                cmd.SetGlobalVectorArray(LightConstants.AdditionalLightsPosId, additionalLightPosition);
                cmd.SetGlobalVectorArray(LightConstants.AdditionalLightsAxisId, additionalLightAxis);
                cmd.SetGlobalVectorArray(LightConstants.SpotAnglesId, SpotAnglesData);
                cmd.SetGlobalInt(LightConstants.AdditionalLightAccountId, additionalLightCount);
               
            }
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
        }
    }
}

