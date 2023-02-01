using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AtmosphereScatteringPass : ScriptableRenderPass
{
    private static string   k_RenderTag = "AtmosphereScattering";
    private        Material material;

    private static int _NearPlaneVectorsId = Shader.PropertyToID("_NearPlaneVectors");
    
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (!TryInitResources()) return;
        
        var cmd = CommandBufferPool.Get();

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
        
        cmd.SetGlobalMatrix(_NearPlaneVectorsId, nearPlaneVectors);
        
        CommandBufferPool.Release(cmd);
    }

    private bool TryInitResources()
    {
        var shader = Shader.Find("Custom/PostProcess/AtmosphereScattering");
        material = CoreUtils.CreateEngineMaterial(shader);
        if (material == null)
        {
            return false;
        }

        return true;
    }
}