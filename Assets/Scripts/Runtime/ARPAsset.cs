using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
[CreateAssetMenu(menuName = "Rendering/ARP Asset")]
public class ARPAsset:RenderPipelineAsset
{
    protected override RenderPipeline CreatePipeline()
    {
        return new ARenderPipeline();
    }
}
