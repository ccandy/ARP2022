using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ARenderPipeline : RenderPipeline
{
    private CameraRender _cameraRender = new CameraRender();
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        for (int n = 0; n < cameras.Length; n++)
        {
            _cameraRender.Render(context, cameras[n]);
        }
    }
}
