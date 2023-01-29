using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalFogPass : ScriptableRenderPass
{
    private static string k_RenderTag = "GlobalFogPass";

    private static readonly int _GlobalFogTempRTId = Shader.PropertyToID("_GlobalFogTempRT");
    private static readonly int _FogHeightMinId       = Shader.PropertyToID("_FogHeightMin");
    private static readonly int _FogHeightMaxId       = Shader.PropertyToID("_FogHeightMax");
    private static readonly int _FogDepthId        = Shader.PropertyToID("_FogDepth");
    private static readonly int _FogColorId        = Shader.PropertyToID("_FogColor");
    private static readonly int _Inverse_VPId        = Shader.PropertyToID("_Inverse_VP");

    private Material  material;
    private GlobalFog globalFogComponent;

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ConfigureInput(ScriptableRenderPassInput.Depth);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (!TryInitResources(ref renderingData))
        {
            return;
        }

        var cmd = CommandBufferPool.Get(k_RenderTag);

        var cameraData = renderingData.cameraData;
        var width      = cameraData.camera.scaledPixelWidth;
        var height     = cameraData.camera.scaledPixelHeight;
        cmd.GetTemporaryRT(_GlobalFogTempRTId, width, height, 0, FilterMode.Point);

        cmd.Blit(cameraData.renderer.cameraColorTarget, _GlobalFogTempRTId);

        cmd.SetGlobalFloat(_FogDepthId, globalFogComponent.fogDepth.value);
        cmd.SetGlobalFloat(_FogHeightMinId, globalFogComponent.fogHeightMin.value);
        cmd.SetGlobalFloat(_FogHeightMaxId, globalFogComponent.fogHeightMax.value);
        cmd.SetGlobalColor(_FogColorId, globalFogComponent.fogColor.value);

        var v         = cameraData.GetViewMatrix();
        var p         = cameraData.GetGPUProjectionMatrix();
        var vpInverse = (p * v).inverse; 
        cmd.SetGlobalMatrix(_Inverse_VPId, vpInverse);
        cmd.Blit(_GlobalFogTempRTId, cameraData.renderer.cameraColorTarget, material);

        context.ExecuteCommandBuffer(cmd);
        context.Submit();

        cmd.ReleaseTemporaryRT(_GlobalFogTempRTId);
        CommandBufferPool.Release(cmd);
    }

    private bool TryInitResources(ref RenderingData renderingData)
    {
        // Check if post process enabled
        if (!renderingData.cameraData.postProcessEnabled || renderingData.cameraData.isSceneViewCamera)
        {
            return false;
        }

        var stack = VolumeManager.instance.stack;
        globalFogComponent = stack.GetComponent<GlobalFog>();
        if (globalFogComponent == null || !globalFogComponent.IsActive())
        {
            return false;
        }

        // Try Create Material
        var shader = Shader.Find("Custom/PostProcess/GlobalFog");
        material = CoreUtils.CreateEngineMaterial(shader);
        if (material == null)
        {
            Debug.LogError($"Fail to create material from shader: Custom/PostProcess/GlobalFog");
            return false;
        }

        return true;
    }
}