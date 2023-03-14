using UnityEngine;

[CreateAssetMenu(fileName = "AtmosphereRenderSettings", menuName = "ScriptableObjects/AtmosphereRenderSettings", order = 1)]
public class AtmosphereRenderSettings : ScriptableObject
{
    public AtmosphereParams atmosphereParams;
    
    [System.Serializable]
    public struct AtmosphereParams
    {
        public float   planetRadius;
        public float   atmosphereHeight;
        public float   ozoneCenter;
        public float   ozoneHeight;
        public Vector3 rayleigh_scattering_h0;
        public Vector3 mie_scattering_h0;
        public Vector3 mie_absorption_h0;
        public Vector3 ozone_absorption_h0;
    }
}