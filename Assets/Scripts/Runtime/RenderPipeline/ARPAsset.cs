using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
[CreateAssetMenu(menuName = "Rendering/ARP Asset")]
public class ARPAsset:RenderPipelineAsset
{
    [SerializeField]
    public ShadowGlobalData shadowGlobalData = new ShadowGlobalData(); 
    
    protected override RenderPipeline CreatePipeline()
    {
        return new ARenderPipeline(shadowGlobalData);
    }
}
