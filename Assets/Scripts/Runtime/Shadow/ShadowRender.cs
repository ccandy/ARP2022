using ARP.Constant;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using ARP.Util;

namespace ARP.Render
{
    public class ShadowRender 
    {
        private const string bufferName                         = "ShadowBuffer";
        
        private CommandBuffer ShadowBuffer;
        
        public ShadowGlobalData GlobalShadowData                    = new ShadowGlobalData();
        private DirectionalShadowData[] _directionalShadowDatas     = new DirectionalShadowData[ShadowConstants.MAX_DIRECTIONS_SHADOW_LIGHTS];
        private Vector4[] cullingSpheres                            = new Vector4[ShadowConstants.MAX_CASACDE_COUNT];
        private CullSphereData[] _cullSphereDatas                   = new CullSphereData[ShadowConstants.MAX_CASACDE_COUNT];    
        private Vector4[] cullSpheresData                           = new Vector4[ShadowConstants.MAX_CASACDE_COUNT];
        
        public int dirShadowCount                                   = 0;

        private CascadeData cascadeData;

        private DirectionalShadowRender _directionalShadowRender;
        
        
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
        }
        
        private void SetupShadowData(ref VisibleLight visibleLight, int index)
        {
            _directionalShadowRender.SetupShadowData(ref visibleLight, index);
        }
        
        public void ConfigShadowDirectionalLightData(ref VisibleLight visibleLight, int index)
        {
            SetupShadowData(ref visibleLight, index);
            dirShadowCount++;
        }

        public void UpdateShadowData(ref ShadowGlobalData shadowGlobalData)
        {
            _directionalShadowRender.UpdateShadowCascadeData(ref shadowGlobalData);
        }
        

        public void UpdateAdditionalShadowData(int additionalLightCount)
        {
            
        }
        
        
        public void Render(ref ScriptableRenderContext context, ref CullingResults cullingResults, ref ShadowGlobalData shadowGlobalData)
        {
            _directionalShadowRender.Render(ref context, ref cullingResults, ref shadowGlobalData);
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
            float shadowDistance        = shadowGlobalData.ShadowDistance;
            float shadowDistaceFade     = shadowGlobalData.ShadowDistanceFade;
            
            Vector4 shadowDistanceData  = new Vector4();
            shadowDistanceData.x        = 1 / shadowDistance;
            shadowDistanceData.y        = 1 / shadowDistaceFade;
            shadowDistanceData.z        = shadowDistance * shadowDistance;
            ShadowBuffer.SetGlobalVector(ShadowConstants.ShadowDistanceDataID, shadowDistanceData);

            if (cascadeData != null)
            {
                Vector4 cascadeDatas        = new Vector4();
                cascadeDatas.x              = cascadeData.CascadeFadeScale;
                cascadeDatas.y              = cascadeData.CascadeFadeRadius;
                ShadowBuffer.SetGlobalVector(ShadowConstants.CascadeDataID, cascadeDatas);
            }
            
            context.ExecuteCommandBuffer(ShadowBuffer);
            ShadowBuffer.Clear();
        }
        

        private void GetShadowMap(ref ScriptableRenderContext context, int shadowmapID, int shadowmapSize, int shadowmapDepth)
        {
            if (ShadowBuffer == null)
            {
                Debug.LogError("Shadow buffer not initialized, cannot create shadowmap");
                return;
            }
            RenderUtil.GetRenderTexture(ref context, shadowmapID, shadowmapSize, shadowmapSize, shadowmapDepth, 
                ShadowBuffer, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
            
        }

        public void CleanUP(ref ScriptableRenderContext context)
        {
            RenderUtil.ReleaseRenderTexture(ref context, ShadowBuffer, ShadowConstants.CascadeShadowMapID);
            _directionalShadowRender.CleanUp();
        }
    }
}
    
