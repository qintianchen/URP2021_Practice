using UnityEngine;

[CreateAssetMenu(fileName = "AtmosphereRenderSettings", menuName = "ScriptableObjects/AtmosphereRenderSettings", order = 1)]
public class AtmosphereRenderSettings : ScriptableObject
{
    public bool             updateTransmittanceLutThisFrame;
    public AtmosphereParams atmosphereParams;

    public float rayleighScatteringScale;
    public float mieScatteringScale;
    public float mieAbsorptionScale;
    public float ozoneAbsorptionScale;

    [System.Serializable]
    public struct AtmosphereParams
    {
        public                   float   planetRadius;
        public                   float   atmosphereHeight;
        [HideInInspector] public Vector3 rayleighScattering_h0; // => new Vector3(5.802f, 13.558f, 33.1f) * 1E-6f;
        public                   float   rayleighHeight;
        [HideInInspector] public Vector3 mieScattering_h0; // => Vector3.one * 3.996f * 1E-6f;
        public                   float   mieHeight;
        public                   float   mieAnisotropy;
        [HideInInspector] public Vector3 mieAbsorption;   // => Vector3.one * 4.4f * 1E-6f;
        [HideInInspector] public Vector3 ozoneAbsorption; // => new Vector3(0.650f, 1.881f, 0.085f) * 1E-6f;
        public                   float   ozoneCenter;
        public                   float   ozoneWidth;
    }
}