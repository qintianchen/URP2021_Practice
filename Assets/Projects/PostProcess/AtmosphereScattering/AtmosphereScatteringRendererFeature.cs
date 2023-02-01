using UnityEngine.Rendering.Universal;

public class AtmosphereScatteringRendererFeature : ScriptableRendererFeature
{
    private AtmosphereScatteringPass m_AtmosphereScatteringPass;
    
    public override void Create()
    {
        m_AtmosphereScatteringPass = new();
        m_AtmosphereScatteringPass.renderPassEvent = RenderPassEvent.BeforeRenderingSkybox;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_AtmosphereScatteringPass);
    }
}
