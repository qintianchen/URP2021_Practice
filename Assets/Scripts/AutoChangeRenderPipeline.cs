using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class AutoChangeRenderPipeline : MonoBehaviour
{
    public UniversalRenderPipelineAsset renderPipelineAsset;
    
    private void OnValidate()
    {
        if (renderPipelineAsset != null)
        {
            GraphicsSettings.renderPipelineAsset = renderPipelineAsset;
            QualitySettings.renderPipeline = renderPipelineAsset;
        }
        else
        {
            Debug.LogWarning($"当前场景没有设置相应的 URP Asset");
        }
    }
}
