using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AtmosphereScatteringRendererFeature : ScriptableRendererFeature
{
    public Material skyViewLutMat; 
    
    private SkyViewLutPass skyViewLutPass;

    public override void Create()
    {
        skyViewLutPass = new SkyViewLutPass(skyViewLutMat);
        skyViewLutPass.renderPassEvent = RenderPassEvent.BeforeRendering;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(skyViewLutPass);
    }

    private class SkyViewLutPass : ScriptableRenderPass
    {
        private static string k_RenderTag = "SkyViewLutPass";
        
        private Material skyViewLutMat;

        private static int _SkyViewLutId = Shader.PropertyToID("_SkyViewLut");

        public SkyViewLutPass(Material skyViewLutMat)
        {
            this.skyViewLutMat = skyViewLutMat;
            
            if (skyViewLutMat == null)
            {
                Debug.LogError($"Material SkyViewLutMat is null.");
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (skyViewLutMat == null)
            {
                return;
            }

            var cmd = CommandBufferPool.Get(k_RenderTag);
            cmd.Blit(null, _SkyViewLutId, skyViewLutMat);
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_SkyViewLutId);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            cmd.GetTemporaryRT(_SkyViewLutId, 256, 128, 0, FilterMode.Bilinear);
        }
    }
}