using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RenderUtil
{
    public static void SetupRenderTarget(ref ScriptableRenderContext context, int renderTargetId, CommandBuffer cmd)
    {
        cmd.SetRenderTarget(renderTargetId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        cmd.ClearRenderTarget(true,false, Color.clear);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    public static void GetRenderTexture(ref ScriptableRenderContext context, int renderTextureID, int width, int height, 
        int depth, CommandBuffer cmd, 
        FilterMode filterMode = FilterMode.Bilinear, RenderTextureFormat format = RenderTextureFormat.ARGB32,
        bool clearColor = false, bool clearDepth = true)
    {
        if (cmd == null)
        {
            Debug.LogWarning("GetRenderTexture: cmd is null");
            return;
        }
        
        cmd.GetTemporaryRT(renderTextureID, width, height, depth, filterMode, format);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    public static void ReleaseRenderTexture(ref ScriptableRenderContext context, CommandBuffer cmd, int renderTextureID)
    {
        cmd.ReleaseTemporaryRT(renderTextureID);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }
}
