using System.Collections;
using System.Collections.Generic;
using ARP.Constant;
using UnityEngine;
using UnityEngine.Rendering;
using ARP.Util;

namespace ARP.Render
{
    public class DirectionalShadowRender 
    {

        private DirectionalShadowData[] _directionalShadowDatas     = new DirectionalShadowData[ShadowConstants.MAX_DIRECTIONS_SHADOW_LIGHTS];
        private Vector4[] cullingSpheres                            = new Vector4[ShadowConstants.MAX_CASACDE_COUNT];
        private CullSphereData[] _cullSphereDatas                   = new CullSphereData[ShadowConstants.MAX_CASACDE_COUNT];    
        private Vector4[] cullSpheresData                           = new Vector4[ShadowConstants.MAX_CASACDE_COUNT];
        
        public int dirShadowCount                                  = 0;
        public CommandBuffer shadowBuffer;

        private CascadeData cascadeData;

        public DirectionalShadowRender(CommandBuffer cmd)
        {
            shadowBuffer = cmd;
        }
        
        public void SetupShadowData(ref VisibleLight visibleLight, int index)
        {
            int maxDirectionalShadowCount = ShadowConstants.MAX_DIRECTIONS_SHADOW_LIGHTS;
            
            if (index >= maxDirectionalShadowCount)
            {
                return;
            }

            _directionalShadowDatas[index] = new DirectionalShadowData()
            {
                ShadowStrength          = visibleLight.light.shadowStrength,
                NormalShadowBias        = visibleLight.light.shadowNormalBias,
                ShadowNearPlane         = visibleLight.light.shadowNearPlane,
                ShadowBias              = visibleLight.light.shadowBias,
                EnableSoftShadow        = (visibleLight.light.shadows == LightShadows.Soft),
                ShadowLightType         = visibleLight.lightType
            };
            dirShadowCount++;
        }

        public void UpdateShadowCascadeData(ref ShadowGlobalData shadowGlobalData)
        {
            if (dirShadowCount == 0)
            {
                return;
            }
            
            int cascadeCount    = (int) shadowGlobalData.CascadeCount;
            int shadowmapSize   = (int) shadowGlobalData.ShadowMapSize;
            
            cascadeData                     = new CascadeData();
            int split                       = ShadowUtil.GetSplit(dirShadowCount * cascadeCount);
            cascadeData.CascadeSplit        = split;
            cascadeData.CascadeTileSize     = shadowmapSize / split;
            
            float f                         = 1f - shadowGlobalData.CascadeFade;
            cascadeData.CascadeFadeScale    = 1f / (1f - f * f);
        }

        public void Render(ref ScriptableRenderContext context, ref CullingResults cullingResults,
            ref ShadowGlobalData shadowGlobalData)
        {
            if (dirShadowCount == 0)
            {
                return;
            }
            
            int shadowmapSize                           = (int) shadowGlobalData.ShadowMapSize;
            int tileSize                                = cascadeData.CascadeTileSize;

            RenderUtil.GetShadowMap(shadowBuffer, ref context, ShadowConstants.CascadeShadowMapID, shadowmapSize, shadowGlobalData.ShadowMapDepth);
            RenderUtil.SetupRenderTarget(ref context, ShadowConstants.CascadeShadowMapID, shadowBuffer);
            
            for (int i = 0; i < dirShadowCount; ++i)
            {
                DirectionalShadowData data = _directionalShadowDatas[i];
                if (data.ShadowLightType == LightType.Directional)
                {
                    RenderShadowCascade(ref context, ref cullingResults, ref shadowGlobalData,ref data, i,tileSize);
                }
            }
        }
        private void RenderShadowCascade(ref ScriptableRenderContext context,ref CullingResults cullingResults, 
            ref ShadowGlobalData shadowGlobalData, ref DirectionalShadowData data, int index, int tileSize)
        {
            if (data == null)
            {
                Debug.LogErrorFormat("DirectionalShadowData at {0} is null", index);
                return;
            }
            
            int cascadeCount = (int)shadowGlobalData.CascadeCount;
            
            Matrix4x4 viewMatrix        = Matrix4x4.identity;
            Matrix4x4 projectionMatrix  = Matrix4x4.identity;
            Vector3 cascadeRatio        = shadowGlobalData.CascadeRaito;
            float nearPlane             = data.ShadowNearPlane;
            float shadowBias            = data.ShadowBias;
            var shadowSettings =
                new ShadowDrawingSettings(cullingResults, index,BatchCullingProjectionType.Orthographic);
            
            for (int n = 0; n < cascadeCount; n++)
            {
                if (!cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives
                    (
                        index,
                        n,
                        cascadeCount,
                        cascadeRatio,
                        tileSize,
                        nearPlane,
                        out viewMatrix,
                        out projectionMatrix,
                        out ShadowSplitData splitData
                    ))
                {
                    continue;
                }
                shadowSettings.splitData = splitData;
               
                if (index == 0)
                {
                    Vector4 cullingSphere               = splitData.cullingSphere;
                    float radius                        = cullingSphere.w;
                    float texelSize                     = 2 * radius / tileSize;
                    cullingSphere.w                     *= cullingSphere.w;
                    CullSphereData cullingSphereData    = new CullSphereData();
                    cullingSphereData.Center            = cullingSphere;
                    cullingSphereData.TexelSize         = texelSize;
                    _cullSphereDatas[n]                 = cullingSphereData;
                    if (index == cascadeCount - 1)
                    {
                        cascadeData.CascadeFadeRadius = 1 / cullingSphere.w;
                    }
                }
                
                int tileIndex               = index * cascadeCount + n;
                int cascadeSplit            = cascadeData.CascadeSplit;
                Vector2 offset              = ShadowUtil.GetViewOffset(tileIndex, cascadeSplit);
                ShadowUtil.SetViewPort(ref context, shadowBuffer, offset, tileSize);
                ShadowUtil.SetViewProjectMatrix(ref context, shadowBuffer, viewMatrix, projectionMatrix);
                ShadowUtil.SetShadowBias(ref context, shadowBuffer, shadowBias);
                context.DrawShadows(ref shadowSettings);
                ShadowUtil.SetShadowBias(ref context, shadowBuffer, 0);
                
                Matrix4x4 worldToViewMatrix                         = ShadowUtil.GetWorldToShadowMatrix(viewMatrix, projectionMatrix,cascadeSplit, offset);
                data.ShadowMatrix[n]                                = worldToViewMatrix;
                data.TileIndex                                      = tileIndex;
            }
        }

        public void SendToGPU(ref ScriptableRenderContext context, ref ShadowGlobalData shadowGlobalData)
        {
            int maxCascadeShadowDataCount       = ShadowConstants.MAX_CASCADE_SHDAOW_DATA_COUNT;
            int maxDirShadow                    = ShadowConstants.MAX_DIRECTIONS_SHADOW_LIGHTS;
            int cascadeCount                    = (int)shadowGlobalData.CascadeCount;
            
            Matrix4x4[] worldToShadowMat        = new Matrix4x4[maxCascadeShadowDataCount];
            Vector4[] dirShadowData             = new Vector4[maxDirShadow];
            
            for (int i = 0; i < dirShadowCount; i++)
            { 
                DirectionalShadowData data  = _directionalShadowDatas[i];
                Matrix4x4[] matrices        = data.ShadowMatrix;
                
                for (int j = 0; j < cascadeCount; j++)
                {
                    int matIndex                        = i * cascadeCount+ j;
                    worldToShadowMat[matIndex]          = matrices[j];
                }
                
                Vector4 dsd                     = new Vector4();
                dsd.x                           = data.ShadowStrength;
                dsd.y                           = data.NormalShadowBias;
                dsd.z                           = data.EnableSoftShadow ? 1 : 0;
                dsd.w                           = data.TileIndex;
                dirShadowData[i]                = dsd;
            }

            for (int n = 0; n < cascadeCount; n++)
            {
                CullSphereData data     = _cullSphereDatas[n];
                cullingSpheres[n]       = data.Center;

                Vector4 cullingSphereData   = new Vector4();
                cullingSphereData.x         = data.TexelSize;
                
                cullSpheresData[n]          = cullingSphereData;
            }

            Vector4 cascadeDataVector   = new Vector4();
            cascadeDataVector.x         = cascadeData.CascadeFadeRadius;
            cascadeDataVector.y         = cascadeData.CascadeFadeScale;
            
            shadowBuffer.SetGlobalVectorArray(ShadowConstants.DirectionalShadowDatasID, dirShadowData);
            shadowBuffer.SetGlobalMatrixArray(ShadowConstants.ShadowToWorldCascadeMatID, worldToShadowMat);
            shadowBuffer.SetGlobalVectorArray(ShadowConstants.CullSpherePosID, cullingSpheres);
            shadowBuffer.SetGlobalVectorArray(ShadowConstants.CullSphereDataID, cullSpheresData);
            shadowBuffer.SetGlobalInt(ShadowConstants.CascadeCountID, cascadeCount);
            shadowBuffer.SetGlobalVector(ShadowConstants.CascadeDataID, cascadeDataVector);
             
            context.ExecuteCommandBuffer(shadowBuffer);
            shadowBuffer.Clear();
        }

        public void CleanUp()
        {
            dirShadowCount = 0;
        }
        
    }
}

