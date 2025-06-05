using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ShadowRender 
{
    private int ShadowToWorldCascadeMatID                   = Shader.PropertyToID("_ShadowToWorldCascadeMat");
    private const string bufferName                         = "ShadowBuffer";
    private const int MAX_DIRECTIONS_SHADOW_LIGHTS          = 4;

    public ShadowGlobalData GlobalShadowData                = new ShadowGlobalData();
    private DirectionalShadowData[] _directionalShadowData  = new DirectionalShadowData[MAX_DIRECTIONS_SHADOW_LIGHTS];
    
    private Matrix4x4[] ShadowToWorldCascadeMats            = new Matrix4x4[MAX_DIRECTIONS_SHADOW_LIGHTS];
    


    public void SetupShadowData(float shadowDistance, ref VisibleLight visibleLight, int index)
    {
        GlobalShadowData.ShadowDistance = shadowDistance;
        if (index >= MAX_DIRECTIONS_SHADOW_LIGHTS)
        {
            return;
        }

        _directionalShadowData[index] = new DirectionalShadowData()
        {
            ShadowStrength  = visibleLight.light.shadowStrength,
            ShadowBias      = visibleLight.light.shadowBias,
            ShadowNearPlane = visibleLight.light.shadowNearPlane
        };
    }
    
    
    
    
    
}
