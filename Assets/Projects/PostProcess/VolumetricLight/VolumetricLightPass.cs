using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumetricLightPass : ScriptableRenderPass
{
    private static string   k_RenderTag = "VolumetricLight";
    private        Material material;

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (!TryInitResources(ref renderingData)) return;
    }

    private bool TryInitResources(ref RenderingData renderingData)
    {
        if (!renderingData.cameraData.postProcessEnabled) return false;

        var shader = Shader.Find("Custom/PostProcess/VolumetricLight");
        material = CoreUtils.CreateEngineMaterial(shader);
        if (material == null)
        {
            return false;
        }

        return false;
    }
}