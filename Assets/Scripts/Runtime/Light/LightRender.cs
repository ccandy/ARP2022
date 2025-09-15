using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using ARP.Constant;

namespace ARP.Render
{
    public class LightRender
    {
        private LightData[] _lightDatas = new LightData[LightConstants.MAX_DIRECTIONAL_LIGHTS];
        
        private ShadowRender _shadowRender      = new ShadowRender();
        
        private CommandBuffer cmd;
        private int directionalLightCount;

        private int maxDirectionalLightCount = LightConstants.MAX_DIRECTIONAL_LIGHTS;
        
        public LightRender()
        {
            cmd = new CommandBuffer()
            {
                name = LightConstants.bufferName
            };
        }

        public void Render(ScriptableRenderContext context, ref CullingResults cullingResults, ref ShadowGlobalData shadowGlobalData)
        {
            _shadowRender.Render(ref context, ref cullingResults, ref shadowGlobalData);
            _shadowRender.SendToGPU(context, ref shadowGlobalData);
            SendToGPU(context, cmd);
            CleanUp(ref context);
        }


        public void SetupLightData(ScriptableRenderContext context, ref CullingResults cullingResults, ref ShadowGlobalData shadowGlobalData)
        {
            directionalLightCount                   = 0;
            int maxDirectionalLightCount            = LightConstants.MAX_DIRECTIONAL_LIGHTS;
            NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
            for (int i = 0; i < visibleLights.Length; ++i)
            {
                VisibleLight visibleLight = visibleLights[i];
                if (visibleLight.lightType == LightType.Directional)
                {
                    ConfigDirectionalLightData(visibleLight, directionalLightCount); 
                    _shadowRender.ConfigShadowDirectionalLightData(ref visibleLight, i);
                    directionalLightCount++;
                }
            }
            _shadowRender.UpdateShadowCascadeData(ref shadowGlobalData);
            
        }
        
        private void ConfigDirectionalLightData(VisibleLight visibleLight, int count)
        {
            if (visibleLight == null || count < 0)
            {
                return;
            }
            
            
            LightData directionalLightData = new LightData();
            
            directionalLightData.LightDirection     = -visibleLight.localToWorldMatrix.GetColumn(2);
            directionalLightData.LightColor         = visibleLight.finalColor;
            directionalLightData.LightAtten         = 1;
            directionalLightData.RenderLayerMask    = visibleLight.light.renderingLayerMask;

            _lightDatas[directionalLightCount]      = directionalLightData;
            
        }

        private void CleanUp(ref ScriptableRenderContext context)
        {
            System.Array.Clear(_lightDatas, 0, _lightDatas.Length);
            _shadowRender.CleanUP(ref context);
        }
        
        private void SendToGPU(ScriptableRenderContext context, CommandBuffer cmd)
        {
            if (cmd == null)
            {
                return;
            }
            
            Vector4[] dirLightColor = new Vector4[maxDirectionalLightCount];
            Vector4[] dirLightDir   = new Vector4[maxDirectionalLightCount];
            Vector4[] dirLightData  = new Vector4[maxDirectionalLightCount];

            for (int i = 0; i < LightConstants.MAX_DIRECTIONAL_LIGHTS; ++i)
            {
                dirLightColor[i] = _lightDatas[i].LightColor;
                dirLightDir[i]      = _lightDatas[i].LightDirection;

                Vector4 lightData   = new Vector4();
                lightData.x         = _lightDatas[i].LightAtten;
                lightData.y         = _lightDatas[i].RenderLayerMask;
                
                dirLightData[i]     = lightData;
            }
            
            cmd.SetGlobalVectorArray(LightConstants.DirectionalLightsColorId,dirLightColor);
            cmd.SetGlobalVectorArray(LightConstants.DirectionalLightsDirId, dirLightDir);
            cmd.SetGlobalInt(LightConstants.DirectonalLightAccountId, directionalLightCount);
            cmd.SetGlobalVectorArray(LightConstants.DirectionalLightsDirId, dirLightData);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }
    }
}
    
