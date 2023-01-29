using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumetricLight : VolumeComponent, IPostProcessComponent
{
    public bool IsActive()         => true;
    public bool IsTileCompatible() => false;
}