using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ARP.Interface
{
    public interface ShadowRenderInterface
    { 
        public void SetupShadowData(ref VisibleLight visibleLight, int index);
        public void CleanUp();
        public void SendToGPU(ref ScriptableRenderContext context, ref ShadowGlobalData shadowGlobalData);

        public void Render(ref ScriptableRenderContext context, ref CullingResults cullingResults,
            ref ShadowGlobalData shadowGlobalData);

    }
}
    
