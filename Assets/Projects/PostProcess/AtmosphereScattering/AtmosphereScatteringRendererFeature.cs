using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AtmosphereScatteringRendererFeature : ScriptableRendererFeature
{
    public AtmosphereRenderSettings atmosphereRenderSettings;
    public ComputeShader            shaderForTransmittanceLut;
    public RenderTexture            transmittanceLut;

    private BeforeRenderSkyboxPass m_BeforeRenderSkyboxPass;

    public override void Create()
    {
        m_BeforeRenderSkyboxPass = new();
        m_BeforeRenderSkyboxPass.renderPassEvent = RenderPassEvent.BeforeRenderingSkybox;
        
        m_BeforeRenderSkyboxPass.Setup(atmosphereRenderSettings, shaderForTransmittanceLut, transmittanceLut);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_BeforeRenderSkyboxPass);
    }
}
