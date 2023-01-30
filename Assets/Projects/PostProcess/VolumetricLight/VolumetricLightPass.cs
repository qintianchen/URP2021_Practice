using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumetricLightPass : ScriptableRenderPass
{
    private static string k_RenderTag = "VolumetricLight";

    private readonly int _VolumetricLightTempRTId    = Shader.PropertyToID("_VolumetricLightTempRT");
    private readonly int _NearPlaneVectorsId         = Shader.PropertyToID("_NearPlaneVectors");
    private readonly int _VolumetricLightIntensityId = Shader.PropertyToID("_VolumetricLightIntensity");

    private Material        material;
    private VolumetricLight volumeComponent;

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ConfigureInput(ScriptableRenderPassInput.Depth);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (!TryInitResources(ref renderingData)) return;

        var w = renderingData.cameraData.camera.scaledPixelWidth;
        var h = renderingData.cameraData.camera.scaledPixelHeight;

        var cmd = CommandBufferPool.Get(k_RenderTag);
        cmd.GetTemporaryRT(_VolumetricLightTempRTId, w, h, 0, FilterMode.Point);

        var camera = renderingData.cameraData.camera;
        var near   = camera.nearClipPlane;
        var aspect = camera.aspect;
        var fov    = Mathf.Deg2Rad * camera.fieldOfView;

        var height = 2 * near * Mathf.Tan(fov / 2);
        var width  = aspect * height;

        var toCenter = camera.transform.forward * near;
        var toTop    = camera.transform.up * height / 2;
        var toRight  = camera.transform.right * width / 2;

        Vector4 topLeftVector  = toCenter + toTop - toRight;
        Vector4 topRightVector = toCenter + toTop + toRight;
        Vector4 botLeftVector  = toCenter - toTop - toRight;
        Vector4 botRightVector = toCenter - toTop + toRight;

        var nearPlaneVectors = new Matrix4x4();
        nearPlaneVectors.SetRow(0, topLeftVector);
        nearPlaneVectors.SetRow(1, topRightVector);
        nearPlaneVectors.SetRow(2, botLeftVector);
        nearPlaneVectors.SetRow(3, botRightVector);

        cmd.SetGlobalMatrix(_NearPlaneVectorsId, nearPlaneVectors);
        cmd.SetGlobalFloat(_VolumetricLightIntensityId, volumeComponent.intensity.value);

        var source = renderingData.cameraData.renderer.cameraColorTarget;
        cmd.Blit(source, _VolumetricLightTempRTId);

        cmd.Blit(_VolumetricLightTempRTId, source, material);

        context.ExecuteCommandBuffer(cmd);
        context.Submit();

        cmd.ReleaseTemporaryRT(_VolumetricLightTempRTId);
        CommandBufferPool.Release(cmd);
    }

    private bool TryInitResources(ref RenderingData renderingData)
    {
        if (!renderingData.cameraData.postProcessEnabled || renderingData.cameraData.isSceneViewCamera) return false;

        var stack           = VolumeManager.instance.stack;
        volumeComponent = stack.GetComponent<VolumetricLight>();
        if (volumeComponent == null || !volumeComponent.IsActive())
        {
            return false;
        }

        var shader = Shader.Find("Custom/PostProcess/VolumetricLight");
        material = CoreUtils.CreateEngineMaterial(shader);
        if (material == null)
        {
            return false;
        }

        return true;
    }
}