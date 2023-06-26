using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorBlitRendererFeature : ScriptableRendererFeature
{
  public class ColorBlitPass : ScriptableRenderPass
  {
    ProfilingSampler m_ProfilingSampler = new ProfilingSampler("ColorBlit");
    Material m_Material;
    RTHandle m_CameraColorTarget;
    float m_Curvature;

    public ColorBlitPass(Material material)
    {
      m_Material = material;
      renderPassEvent = RenderPassEvent.AfterRendering;
    }

    public void SetTarget(RTHandle colorHandle, float curvature)
    {
      m_CameraColorTarget = colorHandle;
      m_Curvature = curvature;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
      ConfigureTarget(m_CameraColorTarget);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
      var cameraData = renderingData.cameraData;
      if (cameraData.camera.cameraType != CameraType.Game)
        return;

      if (m_Material == null)
        return;

      CommandBuffer cmd = CommandBufferPool.Get();
      using (new ProfilingScope(cmd, m_ProfilingSampler))
      {
        m_Material.SetFloat("_Curvature", m_Curvature);
        Blitter.BlitCameraTexture(cmd, m_CameraColorTarget, m_CameraColorTarget, m_Material, 0);
      }
      context.ExecuteCommandBuffer(cmd);
      cmd.Clear();

      CommandBufferPool.Release(cmd);
    }
  }

  public Shader m_Shader;
  public float m_Curvature;

  Material m_Material;

  ColorBlitPass m_RenderPass = null;

  public override void AddRenderPasses(ScriptableRenderer renderer,
                                  ref RenderingData renderingData)
  {
    if (renderingData.cameraData.cameraType == CameraType.Game)
      renderer.EnqueuePass(m_RenderPass);
  }

  public override void SetupRenderPasses(ScriptableRenderer renderer,
                                      in RenderingData renderingData)
  {
    if (renderingData.cameraData.cameraType == CameraType.Game)
    {
      // Calling ConfigureInput with the ScriptableRenderPassInput.Color argument
      // ensures that the opaque texture is available to the Render Pass.
      m_RenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
      m_RenderPass.SetTarget(renderer.cameraColorTargetHandle, m_Curvature);
    }
  }

  public override void Create()
  {
    m_Material = CoreUtils.CreateEngineMaterial(m_Shader);
    m_RenderPass = new ColorBlitPass(m_Material);
  }

  protected override void Dispose(bool disposing)
  {
    CoreUtils.Destroy(m_Material);
  }
}