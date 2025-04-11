using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class LightRender
{
    
    private int DirectionalLightsDirId = Shader.PropertyToID("_DirectionaLightsDir");
    private int DirectionalLightsColorId = Shader.PropertyToID("_DirectionalLightsColor");
    private int DirectonalLightAccountId = Shader.PropertyToID("_DirectionalLightCount");
    
    private const string bufferName = "LightBuffer";
    private const int MAX_DIRECTIONS_LIGHTS = 4;
    private Vector4[] DirectionaLightsDir = new Vector4[MAX_DIRECTIONS_LIGHTS];
    private Vector4[] DirectionaLightsColor = new Vector4[MAX_DIRECTIONS_LIGHTS];
    
    private CommandBuffer cmd;
    private int directionalLightCount;
    

    public LightRender()
    {
        cmd = new CommandBuffer()
        {
            name = bufferName
        };
    }

    public void Render(ScriptableRenderContext context, ref CullingResults cullingResults)
    {
        directionalLightCount = 0;
        
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        for (int i = 0; i < visibleLights.Length; ++i)
        {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)
            {
                if (directionalLightCount < MAX_DIRECTIONS_LIGHTS)
                {
                    ConfigDirectionalLightData(visibleLight, directionalLightCount);
                    directionalLightCount++;
                }
            }
        }

        SendToGPU(context, cmd);
        CleanUp();
    }

    private void ConfigDirectionalLightData(VisibleLight visibleLight, int count)
    {
        if (visibleLight == null || count < 0)
        {
            return;
        }
        DirectionaLightsColor[directionalLightCount] = visibleLight.finalColor;
        DirectionaLightsDir[directionalLightCount] = -visibleLight.localToWorldMatrix.GetColumn(2);
    }

    private void CleanUp()
    {
        System.Array.Clear(DirectionaLightsDir, 0, DirectionaLightsDir.Length);
        System.Array.Clear(DirectionaLightsColor, 0, DirectionaLightsColor.Length);
    }
    
    private void SendToGPU(ScriptableRenderContext context, CommandBuffer cmd)
    {
        if (cmd == null)
        {
            return;
        }
        
        cmd.SetGlobalVectorArray(DirectionalLightsColorId,DirectionaLightsColor);
        cmd.SetGlobalVectorArray(DirectionalLightsDirId, DirectionaLightsDir);
        cmd.SetGlobalInt(DirectionalLightsDirId, directionalLightCount);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }
    
    
    
}
