using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TestCommandBufferRendererFeature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        private ComputeBuffer buffer;
        private ComputeShader computeShader;

        private int resultID = Shader.PropertyToID("Result");

        public void SetUp(ComputeShader computeShader)
        {
            this.computeShader = computeShader;
        }
        
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            buffer = new ComputeBuffer(3, sizeof(float));
            buffer.SetData(new[]{0.5f, 0.4f, 0.6f});
            cmd.GetTemporaryRT(resultID, 256, 256, 0);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (computeShader == null)
            {
                Debug.LogError("ComputeShader is null");
                return;
            }
            
            var cmd = CommandBufferPool.Get("CustomRenderPass");
            var kernelIndex = computeShader.FindKernel("CSMain");

            using (new ProfilingScope(cmd, new ProfilingSampler("DispatchCompute")))
            {
                // cmd.SetBufferData(buffer, new[]{0.3f, 0.4f, 0.6f});
                cmd.SetComputeBufferParam(computeShader, kernelIndex, Shader.PropertyToID("bbs"), buffer);
                
                cmd.DispatchCompute(computeShader, kernelIndex, Mathf.CeilToInt(256 / 8f), Mathf.CeilToInt(256 / 8f), 1);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            buffer.Dispose();
            cmd.ReleaseTemporaryRT(resultID);
        }
    }

    public ComputeShader computeShader;
    CustomRenderPass     m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();
        m_ScriptablePass.SetUp(computeShader);
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


