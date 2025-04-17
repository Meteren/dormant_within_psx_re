using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FogVolumeProfile : VolumeComponent, IPostProcessComponent
{

    public ClampedFloatParameter fogStart = new ClampedFloatParameter(10f, 0f, 100f);
    public ClampedFloatParameter fogEnd = new ClampedFloatParameter(30f, 0f, 100f);
    public ClampedFloatParameter fogDensity = new ClampedFloatParameter(1f, 0f, 100f);
    public ClampedFloatParameter fogDistance = new ClampedFloatParameter(10f, 0f, 100f);
    public ColorParameter fogColor = new ColorParameter(new Color(0.1f,0.1f,0.1f,0.1f));
    public FloatParameter method = new FloatParameter(0,false);
    public BoolParameter isActive = new BoolParameter(true);
    public bool IsActive() => isActive.value;
    public bool IsTileCompatible() => false;
 
}
