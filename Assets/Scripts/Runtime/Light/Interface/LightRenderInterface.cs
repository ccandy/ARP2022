using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


namespace ARP.Interface
{
    public interface LightRenderInterface
    {
        public void ConfigurelLightData(VisibleLight visibleLight);
        public void Init();
        public void CleanUp();
        public void SendToGPU(ref ScriptableRenderContext context, CommandBuffer cmd);
    }
}
    
