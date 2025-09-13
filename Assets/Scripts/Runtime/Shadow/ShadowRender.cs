using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class ShadowRender 
{
    private int ShadowToWorldCascadeMatID                   = Shader.PropertyToID("_ShadowToWorldCascadeMat");
    private int DirectionalShadowDatasID                    = Shader.PropertyToID("_DirectionalShadowDatas");
    private int CullSphereDatasID                           = Shader.PropertyToID("_CullSphereDatas");
    private int CascadeShadowMapID                          = Shader.PropertyToID("_CascadeShadowMap");
    private int CascadeCountID                              = Shader.PropertyToID("_CascadeCount");    
    
    private const string bufferName                         = "ShadowBuffer";
    
    private CommandBuffer ShadowBuffer;
    
    public ShadowGlobalData GlobalShadowData                    = new ShadowGlobalData();
    private DirectionalShadowData[] _directionalShadowDatas     = new DirectionalShadowData[ShadowConstants.MAX_DIRECTIONS_SHADOW_LIGHTS];
    private Vector4[] cullingSpheres                            = new Vector4[ShadowConstants.MAX_CASACDE_COUNT];
    
    private int dirShadowCount                                  = 0;
    /*private int cascadeSplit                                    = 0;
    private int cascadeTileSize                                 = 0;*/

    private CascadeData cascadeData;
    
    public ShadowRender()
    {
        if (ShadowBuffer == null)
        {
            ShadowBuffer = new CommandBuffer()
            {
                name = "ShadowBuffer"
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
            ShadowStrength  = visibleLight.light.shadowStrength,
            ShadowBias      = visibleLight.light.shadowBias,
            ShadowNearPlane = visibleLight.light.shadowNearPlane,
        };
    }

    public void ConfigShadowLightData(ref CullingResults cullingResults)//, ref ShadowGlobalData shadowGlobalData)
    {
        dirShadowCount = 0;
        NativeArray<VisibleLight> visibleLights  = cullingResults.visibleLights;
        int visibleLightCount = visibleLights.Length;
        
        for (int i = 0; i < visibleLightCount; ++i)
        {
            VisibleLight visibleLight = visibleLights[i];

            if (visibleLight.lightType == LightType.Directional)
            {
                SetupShadowData(ref visibleLight, i);
                dirShadowCount++;
            }
        }
        //cascadeSplit = ShadowUtil.GetSplit(dirShadowCount);
    }

    public void ConfigShadowDirectionalLightData(ref VisibleLight visibleLight, int index)
    {
        SetupShadowData(ref visibleLight, index);
        dirShadowCount++;
    }

    public void UpdateShadowCascadeData(ref ShadowGlobalData shadowGlobalData)
    {

        int cascadeCount    = (int) shadowGlobalData.CascadeCount;
        int shadowmapSize   = (int) shadowGlobalData.ShadowMapSize;
        int tileCount       = cascadeCount * dirShadowCount;
        
        cascadeData                     = new CascadeData();
        int split                       = ShadowUtil.GetSplit(dirShadowCount * cascadeCount);
        cascadeData.CascadeSplit        = split;
        cascadeData.CascadeTileSize     = shadowmapSize / split;
    }
    
    public void Render(ref ScriptableRenderContext context, ref CullingResults cullingResults, ref ShadowGlobalData shadowGlobalData)
    {
        NativeArray<VisibleLight> visibleLights     = cullingResults.visibleLights;
        int shadowmapSize                           = (int) shadowGlobalData.ShadowMapSize;
        int tileSize                                = cascadeData.CascadeTileSize;

        GetShadowMap(ref context, CascadeShadowMapID, shadowmapSize, GlobalShadowData.ShadowMapDepth);
        RenderUtil.SetupRenderTarget(ref context, CascadeShadowMapID, ShadowBuffer);
        
        for (int i = 0; i < visibleLights.Length; ++i)
        {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)
            {
                RenderShadowCascade(ref context, ref cullingResults, ref shadowGlobalData, i,tileSize);
            }
        }
    }
    
    
    private void RenderShadowCascade(ref ScriptableRenderContext context,ref CullingResults cullingResults, 
        ref ShadowGlobalData shadowGlobalData, int index, int tileSize)
    {
        ref DirectionalShadowData _directionalShadowData = ref _directionalShadowDatas[index];
        if (_directionalShadowData == null)
        {
            Debug.LogErrorFormat("DirectionalShadowData at {0} is null", index);
            return;
        }
        
        int cascadeCount = (int)shadowGlobalData.CascadeCount;
        
        Matrix4x4 viewMatrix        = Matrix4x4.identity;
        Matrix4x4 projectionMatrix  = Matrix4x4.identity;
        Vector3 cascadeRatio        = shadowGlobalData.CascadeRaito;
        float nearPlane             = _directionalShadowData.ShadowNearPlane;
        float shadowBias            = _directionalShadowData.ShadowBias;
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
                Vector4 cullingSphere   = splitData.cullingSphere;
                cullingSphere.w         *= cullingSphere.w;
                cullingSpheres[n]       = cullingSphere;
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
            _directionalShadowData.ShadowMatrix[n]              = worldToViewMatrix;
            _directionalShadowData.TileIndex                    = tileIndex;
        }
    }
    
    public void SendToGPU(ScriptableRenderContext context, ref ShadowGlobalData shadowGlobalData)
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
            dsd.y                           = data.ShadowBias;
            dsd.z                           = data.ShadowNearPlane;
            dsd.w                           = data.TileIndex;
            dirShadowData[i]                = dsd;
        }
        
        ShadowBuffer.SetGlobalVectorArray(DirectionalShadowDatasID, dirShadowData);
        ShadowBuffer.SetGlobalMatrixArray(ShadowToWorldCascadeMatID, worldToShadowMat);
        ShadowBuffer.SetGlobalVectorArray(CullSphereDatasID, cullingSpheres);
        ShadowBuffer.SetGlobalInt(CascadeCountID, cascadeCount);
        
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
        RenderUtil.ReleaseRenderTexture(ref context, ShadowBuffer, CascadeShadowMapID);
        dirShadowCount = 0;
    }
    
    
    
    
    
    
    
    
    
}
