using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AccessShadowMapPass : ScriptableRenderPass
{
    private static string k_RenderTag = "AccessShadowMap";

    private static readonly int _AccessShadowTempRTId = Shader.PropertyToID("_AccessShadowTempRT");
    
    private Material material;

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ConfigureInput(ScriptableRenderPassInput.Depth);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (!TryInitResources(ref renderingData)) return;
        
        var cmd = CommandBufferPool.Get(k_RenderTag);

        var width  = renderingData.cameraData.camera.scaledPixelWidth;
        var height = renderingData.cameraData.camera.scaledPixelHeight;
        cmd.GetTemporaryRT(_AccessShadowTempRTId, width, height, 0, FilterMode.Point);
        var source = renderingData.cameraData.renderer.cameraColorTarget;
        
        cmd.Blit(source, _AccessShadowTempRTId);
        
        cmd.Blit(_AccessShadowTempRTId, source, material);

        context.ExecuteCommandBuffer(cmd);
        context.Submit();
        
        cmd.ReleaseTemporaryRT(_AccessShadowTempRTId);
        CommandBufferPool.Release(cmd);
    }

    private bool TryInitResources(ref RenderingData renderingData)
    {
        if (renderingData.cameraData.postProcessEnabled)
        {
            return false;
        }
        
        var shader = Shader.Find("Custom/Testing/AccessShadowMap");
        material = CoreUtils.CreateEngineMaterial(shader);
        if (material == null)
        {
            Debug.LogError($"Fail to create material from shader: Custom/Testing/AccessShadowMap");
            return false;
        }

        return true;
    }
}
