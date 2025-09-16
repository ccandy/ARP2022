using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightData
{
    public Color    LightColor;
    public Vector4  LightAxis;
    public Vector4  LightPosition;
    public float    LightAtten;
    public float    RenderLayerMask;
}

public class AdditionalLightData : LightData
{
    public float        LightRange;
    public float        LightSpotAngle;
    public LightType    AdditionalLightType;
}
