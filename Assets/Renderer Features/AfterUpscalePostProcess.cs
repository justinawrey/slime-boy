using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AfterUpscaleFullScreenPassFeature : ScriptableRendererFeature
{
  class AfterUpscaleFullScreenPass : ScriptableRenderPass
  {
    private RTHandle rtTemp;
    private RTHandle rtUpscaled;
    private Material passMaterial;
    private static readonly int manualScaleID = Shader.PropertyToID("_ManualScale");

    public void Setup(Material material)
    {
      base.profilingSampler = new ProfilingSampler("After Upscale Full Screen Pass");
      renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing + 1;
      passMaterial = material;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
      rtUpscaled = renderingData.cameraData.renderer.GetUpscaledTextureHandle();
      RenderingUtils.ReAllocateIfNeeded(ref rtTemp, rtUpscaled.rt.descriptor, name: "_TemporaryColorTexture");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
      CommandBuffer cmd = CommandBufferPool.Get();

      using (new ProfilingScope(cmd, profilingSampler))
      {
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        Vector2 upscaledSize = rtUpscaled.referenceSize;
        Vector2 cameraRtSize = renderingData.cameraData.renderer.cameraColorTargetHandle.referenceSize;
        passMaterial.SetVector(manualScaleID, upscaledSize / cameraRtSize);

        Blitter.BlitCameraTexture(cmd, rtUpscaled, rtTemp);
        Blitter.BlitCameraTexture(cmd, rtTemp, rtUpscaled, passMaterial, 0);
      }

      context.ExecuteCommandBuffer(cmd);
      cmd.Clear();
      CommandBufferPool.Release(cmd);
    }

    public void Dispose()
    {
      rtTemp?.Release();
    }
  }

  AfterUpscaleFullScreenPass pass;
  public Material material;

  public override void Create()
  {
    pass = new AfterUpscaleFullScreenPass();
    pass.Setup(material);
  }

  public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
  {
    if (renderingData.cameraData.cameraType == CameraType.Game) {
      renderer.EnqueuePass(pass);
    }
  }

  protected override void Dispose(bool disposing)
  {
    pass.Dispose();
  }
}


