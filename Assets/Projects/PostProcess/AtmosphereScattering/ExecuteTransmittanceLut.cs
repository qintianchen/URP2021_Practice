using System.Runtime.InteropServices;
using UnityEngine;

public class ExecuteTransmittanceLut : MonoBehaviour
{
    public ComputeShader shader;

    private void OnEnable()
    {
        if (shader == null)
        {
            return;
        }

        var kernelId = shader.FindKernel("CSMain");
        var buffer   = new ComputeBuffer(1, Marshal.SizeOf<AtmosphereParams>());

        var atmosphereParams = new AtmosphereParams();
        atmosphereParams.planetRadius = 0;
        atmosphereParams.atmosphereHeight = 0;
        atmosphereParams.rayleighScattering_h0 = Vector3.zero;
        atmosphereParams.rayleighHeight = 0;
        atmosphereParams.mieScattering_h0 = Vector3.zero;
        atmosphereParams.mieHeight = 0;
        atmosphereParams.mieAnisotropy = 0;
        atmosphereParams.mieAbsorption = Vector3.zero;
        atmosphereParams.ozoneAbsorption = Vector3.zero;
        atmosphereParams.ozoneCenter = 0;
        atmosphereParams.ozoneWidth = 0;

        buffer.SetData(new[] { atmosphereParams });

        shader.SetBuffer(kernelId, "_AtmosphereParamses", buffer);
    }

    struct AtmosphereParams
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