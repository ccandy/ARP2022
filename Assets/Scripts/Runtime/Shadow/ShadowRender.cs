using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ShadowRender 
{
    private int ShadowToWorldCascadeMatID                   = Shader.PropertyToID("_ShadowToWorldCascadeMat");
    private int DirectionalShadowDatasID                    = Shader.PropertyToID("_DirectionalShadowDatas");
    private int CascadeShadowDatasID                        = Shader.PropertyToID("_CascadeShadowMap");
    
    
    private const string bufferName                         = "ShadowBuffer";
    private const int MAX_DIRECTIONS_SHADOW_LIGHTS          = 4;
    
    private CommandBuffer ShadowBuffer;
    

    public ShadowGlobalData GlobalShadowData                = new ShadowGlobalData();
    private DirectionalShadowData[] _directionalShadowDatas  = new DirectionalShadowData[MAX_DIRECTIONS_SHADOW_LIGHTS];
    
    private Matrix4x4[] ShadowToWorldCascadeMats            = new Matrix4x4[MAX_DIRECTIONS_SHADOW_LIGHTS];

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
    
    
    public void SetupShadowData(float shadowDistance, ref VisibleLight visibleLight, int index)
    {
        GlobalShadowData.ShadowDistance = shadowDistance;
        if (index >= MAX_DIRECTIONS_SHADOW_LIGHTS)
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

    public void RenderShadowCascade(ref ScriptableRenderContext context, CommandBuffer commandBuffer, ref CullingResults cullingResults, int index)
    {
        DirectionalShadowData _directionalShadowData = _directionalShadowDatas[index];
        if (_directionalShadowData == null)
        {
            Debug.LogErrorFormat("DirectionalShadowData at {0} is null", index);
            return;
        }
        
        
        GetShadowMap(ref context, CascadeShadowDatasID, GlobalShadowData.ShadowMapSize, GlobalShadowData.ShadowMapDepth );    
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
