using UnityEngine.Rendering.Universal;

public class AccessShadowMapRendererFeature : ScriptableRendererFeature
{
    private AccessShadowMapPass m_AccessShadowMapPass;
    
    public override void Create()
    {
        m_AccessShadowMapPass = new AccessShadowMapPass();
        m_AccessShadowMapPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_AccessShadowMapPass);
    }
}
