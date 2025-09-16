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
        
        public int dirShadowCount                                  = 0;

        private CascadeData cascadeData;
        
        public ShadowRender()
        {
            if (ShadowBuffer == null)
            {
                ShadowBuffer = new CommandBuffer()
                {
                    name = bufferName
                };
            }
        }
        
        private void SetupShadowData(ref VisibleLight visibleLight, int index)
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
        }
        
        public void ConfigShadowDirectionalLightData(ref VisibleLight visibleLight, int index)
        {
            SetupShadowData(ref visibleLight, index);
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
        }

        public void UpdateAdditionalShadowData(int additionalLightCount)
        {
            
        }
        
        
        public void Render(ref ScriptableRenderContext context, ref CullingResults cullingResults, ref ShadowGlobalData shadowGlobalData)
        {
            if (dirShadowCount == 0)
            {
                return;
            }
            
            int shadowmapSize                           = (int) shadowGlobalData.ShadowMapSize;
            int tileSize                                = cascadeData.CascadeTileSize;

            GetShadowMap(ref context, ShadowConstants.CascadeShadowMapID, shadowmapSize, GlobalShadowData.ShadowMapDepth);
            RenderUtil.SetupRenderTarget(ref context, ShadowConstants.CascadeShadowMapID, ShadowBuffer);
            
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
                cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives
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
                );
                
                shadowSettings.splitData = splitData;
               
                if (index == 0)
                {
                    Vector4 cullingSphere               = splitData.cullingSphere;
                    cullingSphere.w                     *= cullingSphere.w;
                    cullingSpheres[n]                   = cullingSphere;

                    float texelSize                     = 2 * cullingSphere.w / tileSize;
                    texelSize                           *= MathConstant.SQRT2;

                    data.TexelSize                      = texelSize;
                }
                
                int tileIndex               = index * cascadeCount + n;
                int cascadeSplit            = cascadeData.CascadeSplit;
                Vector2 offset              = ShadowUtil.GetViewOffset(tileIndex, cascadeSplit);
                ShadowUtil.SetViewPort(ref context, ShadowBuffer, offset, tileSize);
                ShadowUtil.SetViewProjectMatrix(ref context, ShadowBuffer, viewMatrix, projectionMatrix);
                ShadowUtil.SetShadowBias(ref context, ShadowBuffer, shadowBias);
                context.DrawShadows(ref shadowSettings);
                ShadowUtil.SetShadowBias(ref context, ShadowBuffer, 0);
                
                Matrix4x4 worldToViewMatrix                         = ShadowUtil.GetWorldToShadowMatrix(viewMatrix, projectionMatrix,cascadeSplit, offset);
                data.ShadowMatrix[n]                                = worldToViewMatrix;
                data.TileIndex                                      = tileIndex;
            }
        }

        private void SendDirectionalLightDataToGPU(ref ScriptableRenderContext context, ref ShadowGlobalData shadowGlobalData)
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
                dsd.y                           = data.NormalShadowBias * data.TexelSize;
                dsd.z                           = data.EnableSoftShadow ? 1 : 0;
                dsd.w                           = data.TileIndex;
                dirShadowData[i]                = dsd;
            }
            
            ShadowBuffer.SetGlobalVectorArray(ShadowConstants.DirectionalShadowDatasID, dirShadowData);
            ShadowBuffer.SetGlobalMatrixArray(ShadowConstants.ShadowToWorldCascadeMatID, worldToShadowMat);
            ShadowBuffer.SetGlobalVectorArray(ShadowConstants.CullSphereDatasID, cullingSpheres);
            ShadowBuffer.SetGlobalInt(ShadowConstants.CascadeCountID, cascadeCount);
            
            context.ExecuteCommandBuffer(ShadowBuffer);
            ShadowBuffer.Clear();
        }

        private void SendShadowTexelDataToGPU(ref ShadowGlobalData shadowGlobalData)
        {
            Vector4 shadowmapTexel  = new Vector4();
        
            int shadowmapSize       = (int) shadowGlobalData.ShadowMapSize;
            shadowmapTexel.x        = shadowmapSize;
            shadowmapTexel.y        = 1f / shadowmapSize;
            ShadowBuffer.SetGlobalVector(ShadowConstants.ShadowMapTexelSizeID, shadowmapTexel);
        }

        private void SendAdditionalShadowDataToGPU(ref ScriptableRenderContext context,
            ref ShadowGlobalData shadowGlobalData)
        {
            
        }
        
        
        public void SendToGPU(ref ScriptableRenderContext context, ref ShadowGlobalData shadowGlobalData)
        {
            SendDirectionalLightDataToGPU(ref context, ref shadowGlobalData);
            SendAdditionalShadowDataToGPU(ref context, ref shadowGlobalData);
            
            SendShadowTexelDataToGPU(ref shadowGlobalData);
            
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
            dirShadowCount = 0;
        }
    }
}
    
