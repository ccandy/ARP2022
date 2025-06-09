using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShadowGlobalData
{
    public float ShadowDistance = 20f;
    public int ShadowMapDepth = 32;
    
    public enum SMapSize
    {
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096
    }
    public SMapSize ShadowMapSize = SMapSize._1024;
    
    public enum CascadeAmount
    {
        _1 = 1,
        _2 = 2,
        _4 = 4
    }
    public CascadeAmount CascadeCount = CascadeAmount._1;
}


[System.Serializable]
public class DirectionalShadowData
{
    public float ShadowStrength;
    public float ShadowNearPlane;
    public float ShadowBias;
}
