using ARP.Constant;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using ARP.Util;

namespace ARP.Render
{
    public class ShadowRender 
    {
        private const string            bufferName                         = "ShadowBuffer";
        
        private CommandBuffer           ShadowBuffer;
        private CascadeData             cascadeData;

        private DirectionalShadowRender _directionalShadowRender;
        private AdditionalShadowRender _additionalShadowRender;
        
        public ShadowRender()
        {
            if (ShadowBuffer == null)
            {
                ShadowBuffer = new CommandBuffer()
                {
                    name = bufferName
                };
            }

            _directionalShadowRender = new DirectionalShadowRender(ShadowBuffer);
            _additionalShadowRender = new AdditionalShadowRender(ShadowBuffer);
        }
        
        public void ConfigShadowDirectionalLightData(ref VisibleLight visibleLight, int index)
        {
            _directionalShadowRender.SetupShadowData(ref visibleLight, index);
        }

        public void ConfigShadowAdditionalLightData(ref VisibleLight visibleLight, int index)
        {
            _additionalShadowRender.SetupShadowData(ref visibleLight, index);
        }
        
        public void UpdateShadowData(ref ShadowGlobalData shadowGlobalData)
        {
            _directionalShadowRender.UpdateShadowCascadeData(ref shadowGlobalData);
        }
        
        public void Render(ref ScriptableRenderContext context, ref CullingResults cullingResults, ref ShadowGlobalData shadowGlobalData)
        {
            _directionalShadowRender.Render(ref context, ref cullingResults, ref shadowGlobalData);
            _additionalShadowRender.Render(ref context, ref cullingResults, ref shadowGlobalData);
        }
        
        private void SendShadowTexelDataToGPU(ref ScriptableRenderContext context, ref ShadowGlobalData shadowGlobalData)
        {
            Vector4 shadowmapTexel  = new Vector4();
        
            int shadowmapSize       = (int) shadowGlobalData.ShadowMapSize;
            shadowmapTexel.x        = shadowmapSize;
            shadowmapTexel.y        = 1f / shadowmapSize;
            
            ShadowBuffer.SetGlobalVector(ShadowConstants.ShadowMapTexelSizeID, shadowmapTexel);
            context.ExecuteCommandBuffer(ShadowBuffer);
            ShadowBuffer.Clear();
            
        }

        private void SendAdditionalShadowDataToGPU(ref ScriptableRenderContext context,
            ref ShadowGlobalData shadowGlobalData)
        {
            
        }
        
        
        public void SendToGPU(ref ScriptableRenderContext context, ref ShadowGlobalData shadowGlobalData)
        {
            _directionalShadowRender.SendToGPU(ref context, ref shadowGlobalData);
            SendAdditionalShadowDataToGPU(ref context, ref shadowGlobalData);
            
            SendShadowTexelDataToGPU(ref context, ref shadowGlobalData);
            SendGlobalShadowDataToGPU(ref context, ref shadowGlobalData);

        }

        private void SendGlobalShadowDataToGPU(ref ScriptableRenderContext context, ref ShadowGlobalData shadowGlobalData)
        {
            SendShadowDistanceDataToGPU(ref shadowGlobalData);
            SendCascadeDataToGPU();
            
            context.ExecuteCommandBuffer(ShadowBuffer);
            ShadowBuffer.Clear();
        }

        private void SendShadowDistanceDataToGPU(
            ref ShadowGlobalData shadowGlobalData)
        {
            float shadowDistance        = shadowGlobalData.ShadowDistance;
            float shadowDistaceFade     = shadowGlobalData.ShadowDistanceFade;
            
            Vector4 shadowDistanceData  = new Vector4();
            shadowDistanceData.x        = 1 / shadowDistance;
            shadowDistanceData.y        = 1 / shadowDistaceFade;
            shadowDistanceData.z        = shadowDistance * shadowDistance;
            ShadowBuffer.SetGlobalVector(ShadowConstants.ShadowDistanceDataID, shadowDistanceData);
        }

        private void SendCascadeDataToGPU()
        {
            if (cascadeData != null)
            {
                Vector4 cascadeDatas        = new Vector4();
                cascadeDatas.x              = cascadeData.CascadeFadeScale;
                cascadeDatas.y              = cascadeData.CascadeFadeRadius;
                ShadowBuffer.SetGlobalVector(ShadowConstants.CascadeDataID, cascadeDatas);
            }
        }
        
        public void CleanUP(ref ScriptableRenderContext context)
        {
            RenderUtil.ReleaseRenderTexture(ref context, ShadowBuffer, ShadowConstants.CascadeShadowMapID);
            _directionalShadowRender.CleanUp();
            _additionalShadowRender.CleanUp();
        }
    }
}
    
