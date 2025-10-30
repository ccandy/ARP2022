using System.Collections;
using System.Collections.Generic;
using ARP.Constant;
using ARP.Interface;
using ARP.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace ARP.Render
{
    public class AdditionalShadowRender:ShadowRenderInterface
    {
        private AdditionalShadowData[] _additionalLightData     = new AdditionalShadowData[ShadowConstants.MAX_SPOT_SHADOW_LIGHTS];
        public int additionalShadowCount                        = 0;
        
        public CommandBuffer shadowBuffer;

        public AdditionalShadowRender(CommandBuffer cmd)
        {
            shadowBuffer = cmd;
        }

        public void SetupShadowData(ref VisibleLight visibleLight, int index)
        {
            int maxAdditionalShadowCount = ShadowConstants.MAX_SPOT_SHADOW_LIGHTS;

            if (index >= maxAdditionalShadowCount || additionalShadowCount >= maxAdditionalShadowCount)
            {
                return;
            }

            _additionalLightData[index] = new AdditionalShadowData()
            {
                ShadowStrength      = visibleLight.light.shadowStrength,
                NormalShadowBias    = visibleLight.light.shadowNormalBias,
                ShadowNearPlane     = visibleLight.light.shadowNearPlane,
                ShadowBias          = visibleLight.light.shadowBias,
                EnableSoftShadow    = (visibleLight.light.shadows == LightShadows.Soft),
                ShadowLightType     = visibleLight.lightType
            };
            
            additionalShadowCount++;
        }

        
        

        public void Render(ref ScriptableRenderContext context, ref CullingResults cullingResults,
            ref ShadowGlobalData shadowGlobalData)
        {
            if (additionalShadowCount == 0)
            {
                return;
            }
            int shadowmapSize                           = (int) shadowGlobalData.ShadowMapSize;
            RenderUtil.GetShadowMap(shadowBuffer, ref context, ShadowConstants.ShadowMapID, shadowmapSize, shadowGlobalData.ShadowMapDepth);
            RenderUtil.SetupRenderTarget(ref context, ShadowConstants.ShadowMapID, shadowBuffer);

            int split       = ShadowUtil.GetSplit(additionalShadowCount);
            int tileSize    = (int)shadowGlobalData.ShadowMapSize / split;
            for (int i = 0; i < additionalShadowCount; ++i)
            {
                AdditionalShadowData shadowData = _additionalLightData[i];
                RenderAdditionalShadow(ref context, ref cullingResults,ref shadowGlobalData, ref shadowData, i, tileSize,split);
            }
        }

        private void RenderAdditionalShadow(ref ScriptableRenderContext context, ref CullingResults cullingResults,
            ref ShadowGlobalData shadowGlobalData, ref AdditionalShadowData data, int index, int tileSize, int split)
        {
            if (data == null)
            {
                Debug.LogErrorFormat("ShadowData at {0} is null", index);
                return;
            }
            
            Matrix4x4 viewMatrix        = Matrix4x4.identity;
            Matrix4x4 projectionMatrix  = Matrix4x4.identity;
            
            float shadowBias            = data.ShadowBias;
            
            var shadowSettings =
                new ShadowDrawingSettings(cullingResults, index,BatchCullingProjectionType.Perspective);

            if (!cullingResults.ComputeSpotShadowMatricesAndCullingPrimitives(index, out viewMatrix, out projectionMatrix,out ShadowSplitData splitData
                ))
            {
                return;
            }
            
            shadowSettings.splitData =   splitData;

            int tileIndex               = index;
            Vector2 offset              = ShadowUtil.GetViewOffset(tileIndex, split);
            ShadowUtil.SetViewPort(ref context, shadowBuffer, offset, tileSize);
            ShadowUtil.SetViewProjectMatrix(ref context, shadowBuffer, viewMatrix, projectionMatrix);
            ShadowUtil.SetShadowBias(ref context, shadowBuffer, shadowBias);
            context.DrawShadows(ref shadowSettings);
            ShadowUtil.SetShadowBias(ref context, shadowBuffer, 0);
            Matrix4x4 worldToViewMatrix                         = ShadowUtil.GetWorldToShadowMatrix(viewMatrix, projectionMatrix,split, offset);
            
            data.ShadowMatrix                                   = worldToViewMatrix;
            data.TileIndex                                      = tileIndex;
        }
        
        public void SendToGPU(ref ScriptableRenderContext context, ref ShadowGlobalData shadowGlobalData)
        {
            if (additionalShadowCount == 0)
            {
                return;
            }
            
            int maxAdditionalShadowCount    = ShadowConstants.MAX_SPOT_SHADOW_LIGHTS;
            Matrix4x4[] worldToShadowMat    = new Matrix4x4[maxAdditionalShadowCount];
            Vector4[] additionalShadowData  = new Vector4[maxAdditionalShadowCount];

            for (int i = 0; i < additionalShadowCount; ++i)
            {
                AdditionalShadowData data   = _additionalLightData[i];
                worldToShadowMat[i]         = data.ShadowMatrix;
                
                Vector4 asd                 = new Vector4();
                asd.x                       = data.ShadowStrength;
                asd.y                       = data.NormalShadowBias;
                asd.z                       = data.EnableSoftShadow ? 1 : 0;
                asd.w                       = data.TileIndex;
                additionalShadowData[i]     = asd;
            }
            
            shadowBuffer.SetGlobalMatrixArray(ShadowConstants.ShadowToWorldMatID, worldToShadowMat);
            shadowBuffer.SetGlobalVectorArray(ShadowConstants.AdditiaonlShadowDatasID, additionalShadowData);
            
            context.ExecuteCommandBuffer(shadowBuffer);
            shadowBuffer.Clear();
        }
        
        public void CleanUp()
        {
            additionalShadowCount = 0;
        }
    }
}
    
