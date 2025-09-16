using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using ARP.Constant;

namespace ARP.Render
{
    public class LightRender
    {
        private ShadowRender _shadowRender                      = new ShadowRender();
        
        private DirectionalLightRender _directionalLightRender  = new DirectionalLightRender();
        private AdditionalLightRender _additionalLightRender    = new AdditionalLightRender();
        
        private CommandBuffer cmd;
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
            additionalLightCount                    = 0;
            
            _shadowRender.dirShadowCount            = 0;
            
            _directionalLightRender.Init();
            
            NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
            for (int i = 0; i < visibleLights.Length; ++i)
            {
                VisibleLight visibleLight = visibleLights[i];
                if (visibleLight.lightType == LightType.Directional)
                {
                    _directionalLightRender.ConfigurelLightData(visibleLight);
                    if (visibleLight.light.shadows != LightShadows.None)
                    {
                        _shadowRender.ConfigShadowDirectionalLightData(ref visibleLight, i);
                    }
                }
                else
                {
                   _additionalLightRender.ConfigurelLightData(visibleLight);
                }
            }
            _shadowRender.UpdateShadowCascadeData(ref shadowGlobalData);
            
        }
        
        private void CleanUp(ref ScriptableRenderContext context)
        {
            _directionalLightRender.CleanUp();
            _additionalLightRender.CleanUp();
            _shadowRender.CleanUP(ref context);
        }
        
        private void SendToGPU(ref ScriptableRenderContext context, CommandBuffer cmd)
        {
            if (cmd == null || context == null)
            {
                return;
            }

            _directionalLightRender.SendToGPU(ref context, cmd);
            _additionalLightRender.SendToGPU(ref context, cmd);
        }
    }
}
    
