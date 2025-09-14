using System.Collections;
using System.Collections.Generic;
using ARP.Render;
using UnityEngine;
using UnityEngine.Rendering;

public class ARenderPipeline : RenderPipeline
{
    
    private ShadowGlobalData _shadowGlobalData;
    
    private CameraRender _cameraRender = new CameraRender();

    public ARenderPipeline(ShadowGlobalData shadowGlobalData)
    {
        if (shadowGlobalData != null)
        {
            _shadowGlobalData = shadowGlobalData;
        }
    }
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        for (int n = 0; n < cameras.Length; n++)
        {
            _cameraRender.Render(context, cameras[n], _shadowGlobalData);
        }
    }
}
