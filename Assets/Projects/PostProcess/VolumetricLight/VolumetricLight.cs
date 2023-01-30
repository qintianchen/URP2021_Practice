using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumetricLight : VolumeComponent, IPostProcessComponent
{
    public FloatParameter intensity = new FloatParameter(2);
    public                  BoolParameter  enable    = new BoolParameter(false);

    public bool IsActive()         => enable.value;
    public bool IsTileCompatible() => false;
}