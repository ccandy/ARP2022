using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class ShadowRender 
{
    private int ShadowToWorldCascadeMatID                   = Shader.PropertyToID("_ShadowToWorldCascadeMat");
    private int DirectionalShadowDatasID                    = Shader.PropertyToID("_DirectionalShadowDatas");
    private int CascadeShadowDatasID                        = Shader.PropertyToID("_CascadeShadowMap");
    
    
    private const string bufferName                         = "ShadowBuffer";
    
    
    
    private CommandBuffer ShadowBuffer;
    

    public ShadowGlobalData GlobalShadowData                    = new ShadowGlobalData();
    private DirectionalShadowData[] _directionalShadowDatas     = new DirectionalShadowData[ShadowConstants.MAX_DIRECTIONS_SHADOW_LIGHTS];
    private Vector4[] cullingSpheres                            = new Vector4[ShadowConstants.MAX_CASACDE_COUNT];
    
    private Matrix4x4[] ShadowToWorldCascadeMats                = new Matrix4x4[ShadowConstants.MAX_DIRECTIONS_SHADOW_LIGHTS];
    
    private int cascadeTileCount                                = 0;
    private int cascadeSplit                                    = 0;
    
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
            ShadowNearPlane = visibleLight.light.shadowNearPlane
        };
    }

    public void ConfigShadowLightData(ref CullingResults cullingResults, ref ShadowGlobalData shadowGlobalData)
    {
        NativeArray<VisibleLight> visibleLights  = cullingResults.visibleLights;
        int visibleLightCount = visibleLights.Length;
        
        for (int i = 0; i < visibleLightCount; ++i)
        {
            VisibleLight visibleLight = visibleLights[i];

            if (visibleLight.lightType == LightType.Directional)
            {
                SetupShadowData(ref visibleLight, i);
                cascadeTileCount++;
            }
        }
        cascadeSplit = ShadowUtil.GetSplit(cascadeTileCount);
    }
    
    private void RenderShadowCascade(ref ScriptableRenderContext context, CommandBuffer commandBuffer, ref CullingResults cullingResults, 
        ref ShadowGlobalData shadowGlobalData, int index)
    {
        DirectionalShadowData _directionalShadowData = _directionalShadowDatas[index];
        if (_directionalShadowData == null)
        {
            Debug.LogErrorFormat("DirectionalShadowData at {0} is null", index);
            return;
        }
        
        int shadowmapSize   = (int) shadowGlobalData.ShadowMapSize;
        int tileSize        = shadowmapSize / cascadeTileCount;
        
        GetShadowMap(ref context, CascadeShadowDatasID, shadowmapSize, GlobalShadowData.ShadowMapDepth);
        int cascadeCount = (int)shadowGlobalData.CascadeCount;
        
        Matrix4x4 viewMatrix        = Matrix4x4.identity;
        Matrix4x4 projectionMatrix  = Matrix4x4.identity;
        Vector3 cascadeRatio        = shadowGlobalData.CascadeRaito;
        float nearPlane             = _directionalShadowData.ShadowNearPlane;
        
        var shadowSettings =
            new ShadowDrawingSettings(cullingResults, index);
        
        
        for (int n = 0; n < cascadeCount; n++)
        {
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives
            (
                index,
                n,
                cascadeCount,
                cascadeRatio,
                shadowmapSize,
                nearPlane,
                out viewMatrix,
                out projectionMatrix,
                out ShadowSplitData splitData
            );
            shadowSettings.splitData = splitData;
            
            
            
            
        }
        
        
    }
    

    private void GetShadowMap(ref ScriptableRenderContext context, int shadowmapID, int shadowmapSize, int shadowmapDepth)
    {
        if (ShadowBuffer == null)
        {
            Debug.LogError("Shadow buffer not initialized, cannot create shadowmap");
            return;
        }
        ShadowBuffer.GetTemporaryRT(shadowmapID, shadowmapSize, shadowmapSize, shadowmapDepth, FilterMode.Bilinear);
        
    }
    
    
    
    
    
    
    
    
    
}
