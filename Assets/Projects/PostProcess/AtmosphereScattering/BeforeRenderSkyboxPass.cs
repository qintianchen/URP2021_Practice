using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BeforeRenderSkyboxPass : ScriptableRenderPass
{
    private static string k_RenderTag = "BeforeRenderSkybox";

    private static int _FOVId                    = Shader.PropertyToID("_FOV");
    private static int _AspectId                 = Shader.PropertyToID("_Aspect");
    private static int _NearPlaneVectorsId       = Shader.PropertyToID("_NearPlaneVectors");
    private static int _TransmittanceLutTempRTId = Shader.PropertyToID("_TransmittanceLutTempRT");
    private static int _TransmittanceLutId       = Shader.PropertyToID("_TransmittanceLut");

    private AtmosphereRenderSettings atmosphereRenderSettings;
    private ComputeShader            shaderForTransmittanceLut;
    private RenderTexture            transmittanceLut;

    public void Setup(AtmosphereRenderSettings atmosphereRenderSettings, ComputeShader shaderForTransmittanceLut, RenderTexture transmittanceLut)
    {
        this.atmosphereRenderSettings = atmosphereRenderSettings;
        this.shaderForTransmittanceLut = shaderForTransmittanceLut;
        this.transmittanceLut = transmittanceLut;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (!TryInitResources()) return;

        var cmd = CommandBufferPool.Get(k_RenderTag);

        if (atmosphereRenderSettings != null && atmosphereRenderSettings.updateTransmittanceLutThisFrame)
        {
            UpdateTransmittanceLut(context, cmd, ref renderingData);
        }

        BeforeRenderSkyboxInNative(context, ref renderingData, cmd);

        CommandBufferPool.Release(cmd);
    }

    private void UpdateTransmittanceLut(ScriptableRenderContext context, CommandBuffer cmd, ref RenderingData renderingData)
    {
        var kernelId = shaderForTransmittanceLut.FindKernel("CSMain");
        var buffer   = new ComputeBuffer(1, Marshal.SizeOf<AtmosphereRenderSettings.AtmosphereParams>());

        var atmosphereParams = atmosphereRenderSettings.atmosphereParams;
        buffer.SetData(new[] { atmosphereParams });
        shaderForTransmittanceLut.SetBuffer(kernelId, "_AtmosphereParamses", buffer);
        var rtWidth  = transmittanceLut.width;
        var rtHeight = transmittanceLut.height;
        cmd.GetTemporaryRT(_TransmittanceLutTempRTId, rtWidth, rtHeight, 0, filter: default, format: default, readWrite: default, antiAliasing: 1, enableRandomWrite: true);
        cmd.SetComputeTextureParam(shaderForTransmittanceLut, kernelId, "Result", _TransmittanceLutTempRTId);
        cmd.DispatchCompute(shaderForTransmittanceLut, kernelId, Mathf.CeilToInt(rtWidth / 8f), Mathf.CeilToInt(rtHeight / 8f), 1);

        context.ExecuteCommandBuffer(cmd);
        context.Submit();
        cmd.Clear();

        cmd.Blit(_TransmittanceLutTempRTId, transmittanceLut);
        cmd.ReleaseTemporaryRT(_TransmittanceLutTempRTId);
        context.ExecuteCommandBuffer(cmd);
        context.Submit();
        cmd.Clear();
        
        cmd.SetRenderTarget(renderingData.cameraData.renderer.cameraColorTarget);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    /// <summary>
    /// 当前版本的 URP 仍然是在 Native 层处理 Skybox 的渲染（ScriptableRenderContext.DrawSkybox()）。
    /// 所以这里的 Pass 只是给 Skybox 材质初始化好相关的数据，不做实际的 Graphics Pass 
    /// </summary>
    private void BeforeRenderSkyboxInNative(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
    {
        var cameraData = renderingData.cameraData;
        var camera     = cameraData.camera;
        var near       = camera.nearClipPlane;
        var aspect     = camera.aspect;
        var fov        = camera.fieldOfView * Mathf.Deg2Rad;

        var height = 2 * near * Mathf.Tan(fov / 2);
        var width  = height * aspect;

        var toForward = camera.transform.forward * near;
        var toUp      = camera.transform.up * height / 2;
        var toRight   = camera.transform.right * width / 2;

        var toUpLeft    = toForward + toUp - toRight;
        var toUpRight   = toForward + toUp + toRight;
        var toDownLeft  = toForward - toUp - toRight;
        var toDownRight = toForward - toUp + toRight;

        var nearPlaneVectors = new Matrix4x4();
        nearPlaneVectors.SetRow(0, toUpLeft);
        nearPlaneVectors.SetRow(1, toUpRight);
        nearPlaneVectors.SetRow(2, toDownLeft);
        nearPlaneVectors.SetRow(3, toDownRight);

        cmd.SetGlobalTexture(_TransmittanceLutId, transmittanceLut);
        cmd.SetGlobalMatrix(_NearPlaneVectorsId, nearPlaneVectors);
        cmd.SetGlobalFloat(_FOVId, fov);
        cmd.SetGlobalFloat(_AspectId, aspect);

        context.ExecuteCommandBuffer(cmd);
        context.Submit();
        cmd.Clear();
    }

    private bool TryInitResources()
    {
        return true;
    }
}