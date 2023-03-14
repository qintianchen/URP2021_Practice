using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AtmosphereScatteringRendererFeature : ScriptableRendererFeature
{
    private BeforeRenderSkyboxPass beforeRenderSkyboxPass;
    private AerialPerspectivePass  aerialPerspectivePass;

    public override void Create()
    {
        beforeRenderSkyboxPass = new BeforeRenderSkyboxPass();
        beforeRenderSkyboxPass.renderPassEvent = RenderPassEvent.BeforeRendering;
        
        aerialPerspectivePass = new AerialPerspectivePass();
        aerialPerspectivePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(beforeRenderSkyboxPass);
        renderer.EnqueuePass(aerialPerspectivePass);
    }
    
    private class BeforeRenderSkyboxPass: ScriptableRenderPass
    {
        private Shader transmittanceLutShader;
        private Shader skyViewLutShader;
        private Shader aerialPerspectiveLutShader;
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            
        }
    }

    private class AerialPerspectivePass : ScriptableRenderPass
    {
        private Shader aerialPerspectiveShader;
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            
        }
    }
}
