using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LightRender
{
    private const string bufferName = "LightBuffer";

    private CommandBuffer cmd;


    public LightRender()
    {
        cmd = new CommandBuffer()
        {
            name = "LightRender"
        };
    }

    public void Render(ScriptableRenderContext context, ref CullingResults cullingResults)
    {
        
    }
    
}
