using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public interface LightRenderInterface
{
   public void ConfigurelLightData(VisibleLight visibleLight);
   public void CleanUp();
   public void SendToGPU(ref ScriptableRenderContext context, CommandBuffer cmd);

   public void Render(ref ScriptableRenderContext context, ref CullingResults cullingResults,
      ref ShadowGlobalData shadowGlobalData);

}
