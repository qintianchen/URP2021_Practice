using UnityEngine;

[CreateAssetMenu(fileName = "AtmosphereRenderSettings", menuName = "ScriptableObjects/AtmosphereRenderSettings", order = 1)]
public class AtmosphereRenderSettings : ScriptableObject
{
    public bool             updateTransmittanceLutThisFrame;
    public AtmosphereParams atmosphereParams;
    
    [System.Serializable]
    public struct AtmosphereParams
    {
        public float   planetRadius;
        public float   atmosphereHeight;
        public Vector3 rayleighScattering_h0;
        public float   rayleighHeight;
        public Vector3 mieScattering_h0;
        public float   mieHeight;
        public float   mieAnisotropy;
        public Vector3 mieAbsorption;
        public Vector3 ozoneAbsorption;
        public float   ozoneCenter;
        public float   ozoneWidth;
    }
}
