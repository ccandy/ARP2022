using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using ARP.Constant;

namespace ARP.Render
{
    public class LightRender
    {
        private LightData[] _directionalLightDatas              = new LightData[LightConstants.MAX_DIRECTIONAL_LIGHTS];
        private AdditionalLightData [] _additionalLightDatas    = new AdditionalLightData[LightConstants.MAX_DIRECTIONAL_LIGHTS];
        
        private ShadowRender _shadowRender      = new ShadowRender();
        
        private CommandBuffer cmd;
        private int directionalLightCount;
        private int additionalLightCount;
        
        private int maxDirectionalLightCount    = LightConstants.MAX_DIRECTIONAL_LIGHTS;
        private int maxAdditionalLightCount     = LightConstants.MAX_ADDITIONAL_LIGHTS;
        private int maxSpotLightCount           = LightConstants.MAX_SPOT_LIGHTS;
        
        public LightRender()
        {
            cmd = new CommandBuffer()
            {
                name = LightConstants.bufferName
            };
        }

        public void Render(ref ScriptableRenderContext context, ref CullingResults cullingResults, ref ShadowGlobalData shadowGlobalData)
        {
            _shadowRender.Render(ref context, ref cullingResults, ref shadowGlobalData);
            _shadowRender.SendToGPU(ref context, ref shadowGlobalData);
            SendToGPU(ref context, cmd);
            CleanUp(ref context);
        }


        public void SetupLightData(ScriptableRenderContext context, ref CullingResults cullingResults, ref ShadowGlobalData shadowGlobalData)
        {
            directionalLightCount                   = 0;
            additionalLightCount                    = 0;
            
            _shadowRender.dirShadowCount            = 0;
            NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
            for (int i = 0; i < visibleLights.Length; ++i)
            {
                VisibleLight visibleLight = visibleLights[i];
                if (visibleLight.lightType == LightType.Directional)
                {
                    ConfigDirectionalLightData(visibleLight);
                    if (visibleLight.light.shadows != LightShadows.None)
                    {
                        _shadowRender.ConfigShadowDirectionalLightData(ref visibleLight, i);
                    }
                }
                else
                {
                    ConfigAdditionalLightData(visibleLight);
                }
            }
            _shadowRender.UpdateShadowCascadeData(ref shadowGlobalData);
            
        }

        private void ConfigAdditionalLightData(VisibleLight visibleLight)
        {
            if (visibleLight == null)
            {
                return;
            }
            
            AdditionalLightData additionalData          = new AdditionalLightData();

            additionalData.LightPosition                = visibleLight.localToWorldMatrix.GetColumn(3);
            additionalData.LightColor                   = visibleLight.finalColor;
            additionalData.AdditionalLightType          = visibleLight.lightType; 
            
            float range                                 = visibleLight.range;
            additionalData.LightRange                   = 1 / Mathf.Max(range * range, 0.0001f);
            
            _additionalLightDatas[additionalLightCount] = additionalData;
            additionalLightCount++;
        }
        
        
        private void ConfigDirectionalLightData(VisibleLight visibleLight)
        {
            if (visibleLight == null)
            {
                return;
            }
            
            
            LightData directionalLightData = new LightData();
            
            directionalLightData.LightDirection     = -visibleLight.localToWorldMatrix.GetColumn(2);
            directionalLightData.LightColor         = visibleLight.finalColor;
            directionalLightData.LightAtten         = 1;
            directionalLightData.RenderLayerMask    = visibleLight.light.renderingLayerMask - 1;

            _directionalLightDatas[directionalLightCount]      = directionalLightData;
            directionalLightCount++;
        }

        private void CleanUp(ref ScriptableRenderContext context)
        {
            System.Array.Clear(_directionalLightDatas, 0, _directionalLightDatas.Length);
            _shadowRender.CleanUP(ref context);
        }
        
        private void SendToGPU(ref ScriptableRenderContext context, CommandBuffer cmd)
        {
            if (cmd == null || context == null)
            {
                return;
            }
            SendDirectonalLightsDataToGPU(ref context, cmd);
            SendAdditionalLightsDataToGPU(ref context, cmd);
        }

        private void SendAdditionalLightsDataToGPU(ref ScriptableRenderContext context, CommandBuffer cmd)
        {
            Vector4[] additionalLightColor       = new Vector4[additionalLightCount];
            Vector4[] additionalLightPosition    = new Vector4[additionalLightCount];
            Vector4[] additionalightData         = new Vector4[additionalLightCount];

            for (int i = 0; i < additionalLightCount; ++i)
            {
                
                AdditionalLightData additionalLightData     = _additionalLightDatas[i];
                
                additionalLightColor[i]                  = additionalLightData.LightColor;
                additionalLightPosition[i]                  = additionalLightData.LightPosition;

                Vector4 lightData                           = new Vector4();
                lightData.x                                 = additionalLightData.LightRange;
                lightData.y                                 = (int)additionalLightData.AdditionalLightType;
            }
            cmd.SetGlobalVectorArray(LightConstants.AdditionalLightsColorId, additionalLightColor);
            cmd.SetGlobalVectorArray(LightConstants.AdditionalLightsDataId, additionalightData);
            cmd.SetGlobalVectorArray(LightConstants.AdditionalLightsPosId, additionalLightPosition);
            cmd.SetGlobalInt(LightConstants.AdditionalLightAccountId, additionalLightCount);
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }
        
        private void SendDirectonalLightsDataToGPU(ref ScriptableRenderContext context, CommandBuffer cmd)
        {
            Vector4[] dirLightColor = new Vector4[directionalLightCount];
            Vector4[] dirLightDir   = new Vector4[directionalLightCount];
            Vector4[] dirLightData  = new Vector4[directionalLightCount];

            for (int i = 0; i < directionalLightCount; ++i)
            {
                dirLightColor[i] = _directionalLightDatas[i].LightColor;
                dirLightDir[i]      = _directionalLightDatas[i].LightDirection;

                Vector4 lightData   = new Vector4();
                lightData.x         = _directionalLightDatas[i].LightAtten;
                lightData.y         = _directionalLightDatas[i].RenderLayerMask;
                
                dirLightData[i]     = lightData;
            }
            
            cmd.SetGlobalVectorArray(LightConstants.DirectionalLightsColorId,dirLightColor);
            cmd.SetGlobalVectorArray(LightConstants.DirectionalLightsDirId, dirLightDir);
            cmd.SetGlobalInt(LightConstants.DirectonalLightAccountId, directionalLightCount);
            cmd.SetGlobalVectorArray(LightConstants.DirectionalLightsDataId, dirLightData);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }
        
        
    }
}
    
