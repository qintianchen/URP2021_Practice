using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalFog : VolumeComponent, IPostProcessComponent
{
    public                   BoolParameter  isOn         = new BoolParameter(false);
    [Range(0, 1000)]  public FloatParameter fogHeightMin = new FloatParameter(0);
    [Range(0, 1000)]  public FloatParameter fogHeightMax = new FloatParameter(0);
    [Range(10, 1000)] public FloatParameter fogDepth     = new(500);
    public                   ColorParameter fogColor     = new(Color.white);

    public bool IsActive()         => isOn.value;
    public bool IsTileCompatible() => false;
}