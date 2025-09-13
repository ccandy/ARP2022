using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
            
public class LightRender
{
    
    private int DirectionalLightsDirId      = Shader.PropertyToID("_DirectionaLightsDir");
    private int DirectionalLightsColorId    = Shader.PropertyToID("_DirectionalLightsColor");
    private int DirectonalLightAccountId    = Shader.PropertyToID("_DirectionalLightCount");
    
    private const string bufferName         = "LightBuffer";
    
    private Vector4[] DirectionaLightsDir   = new Vector4[LightConstants.MAX_DIRECTIONAL_LIGHTS];
    private Vector4[] DirectionaLightsColor = new Vector4[LightConstants.MAX_DIRECTIONAL_LIGHTS];
    private ShadowRender _shadowRender      = new ShadowRender();
    
    private CommandBuffer cmd;
    private int directionalLightCount;

   
    
    public LightRender()
    {
        cmd = new CommandBuffer()
        {
            name = bufferName
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
        DirectionaLightsColor[directionalLightCount] = visibleLight.finalColor;
        DirectionaLightsDir[directionalLightCount]      = -visibleLight.localToWorldMatrix.GetColumn(2);
    }

    private void CleanUp(ref ScriptableRenderContext context)
    {
        System.Array.Clear(DirectionaLightsDir, 0, DirectionaLightsDir.Length);
        System.Array.Clear(DirectionaLightsColor, 0, DirectionaLightsColor.Length);
        _shadowRender.CleanUP(ref context);
    }
    
    private void SendToGPU(ScriptableRenderContext context, CommandBuffer cmd)
    {
        if (cmd == null)
        {
            return;
        }
        
        cmd.SetGlobalVectorArray(DirectionalLightsColorId,DirectionaLightsColor);
        cmd.SetGlobalVectorArray(DirectionalLightsDirId, DirectionaLightsDir);
        cmd.SetGlobalInt(DirectonalLightAccountId, directionalLightCount);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }
    
    
    
}
