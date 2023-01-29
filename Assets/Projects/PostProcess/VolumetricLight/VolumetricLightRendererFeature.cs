using UnityEngine.Rendering.Universal;

public class VolumetricLightRendererFeature : ScriptableRendererFeature
{
    private VolumetricLightPass m_VolumetricLightPass;
    
    public override void Create()
    {
        m_VolumetricLightPass = new VolumetricLightPass();
        m_VolumetricLightPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_VolumetricLightPass);
    }
}
