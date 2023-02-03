// using System;
// using System.Runtime.InteropServices;
// using UnityEngine;
// using UnityEngine.Rendering.Universal;
//
// public class ExecuteTransmittanceLut : MonoBehaviour
// {
//     public ComputeShader shader;
//     public RenderTexture transmittanceLut;
//
//     private void OnGUI()
//     {
//         var ret = GUI.Button(new Rect(100, 100, 300, 100), "生成TransmittanceLut");
//         if (ret)
//         {
//             Run();
//         }
//     }
//
//     private void Update()
//     {
//         Run();
//     }
//
//     private void Run()
//     {
//         if (shader == null)
//         {
//             return;
//         }
//
//         var kernelId = shader.FindKernel("CSMain");
//         var buffer   = new ComputeBuffer(1, Marshal.SizeOf<AtmosphereScatteringRendererFeature.AtmosphereParams>());
//
//         var camera = Camera.main;
//         var cameraData = camera.GetComponent<UniversalAdditionalCameraData>();
//         
//         var atmosphereParams = new AtmosphereScatteringRendererFeature.AtmosphereParams();
//         atmosphereParams.planetRadius = 6400000;
//         atmosphereParams.atmosphereHeight = 60000;
//         atmosphereParams.rayleighScattering_h0 = new Vector3(5.802f, 13.558f, 33.1f) * 0.000001f;
//         atmosphereParams.rayleighHeight = 8000;
//         atmosphereParams.mieScattering_h0 = Vector3.one * 3.996f * 0.000001f;
//         atmosphereParams.mieHeight = 1200;
//         atmosphereParams.mieAnisotropy = 0.8f;
//         atmosphereParams.mieAbsorption = Vector3.one * 4.4f * 0.000001f;
//         atmosphereParams.ozoneAbsorption = new Vector3(0.65f, 1.881f, 0.085f) * 0.000001f;
//         atmosphereParams.ozoneCenter = 25000f;
//         atmosphereParams.ozoneWidth = 15000f;
//
//         buffer.SetData(new[] { atmosphereParams });
//
//         shader.SetBuffer(kernelId, "_AtmosphereParamses", buffer);
//
//         var rt = RenderTexture.GetTemporary(512, 512, 0);
//         rt.enableRandomWrite = true;
//         shader.SetTexture(kernelId, "Result", rt);
//         
//         var width  = transmittanceLut.width;
//         var height = transmittanceLut.height;
//         shader.Dispatch(kernelId, Mathf.CeilToInt(width / 8f), Mathf.CeilToInt(height / 8f), 1);
//         
//         Graphics.Blit(rt, transmittanceLut);
//         RenderTexture.ReleaseTemporary(rt);
//     }
//     
//     // struct AtmosphereParams
//     // {
//     //     public float   planetRadius;
//     //     public float   atmosphereHeight;
//     //     public Vector3 rayleighScattering_h0;
//     //     public float   rayleighHeight;
//     //     public Vector3 mieScattering_h0;
//     //     public float   mieHeight;
//     //     public float   mieAnisotropy;
//     //     public Vector3 mieAbsorption;
//     //     public Vector3 ozoneAbsorption;
//     //     public float   ozoneCenter;
//     //     public float   ozoneWidth;
//     // }
// }