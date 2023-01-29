using UnityEngine.Rendering.Universal;

public class GlobalFogRenderFeature : ScriptableRendererFeature
{
    GlobalFogPass m_GlobalFogPass;

    public override void Create()
    {
        m_GlobalFogPass = new GlobalFogPass();
        m_GlobalFogPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_GlobalFogPass);
    }
}


