using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ShadowConstants 
{
    public readonly static int MAX_DIRECTIONS_SHADOW_LIGHTS          = 4;
    public readonly static int MAX_CASACDE_COUNT                     = 4;
    public readonly static int MAX_CASCADE_SHDAOW_DATA_COUNT         = MAX_DIRECTIONS_SHADOW_LIGHTS * MAX_CASACDE_COUNT;
}
