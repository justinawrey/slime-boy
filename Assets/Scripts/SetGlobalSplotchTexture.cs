using UnityEngine;

public class SetGlobalSplotchTexture : MonoBehaviour
{
  [SerializeField] private RenderTexture splotchTexture;

  private void Update()
  {
    Shader.SetGlobalTexture("_SplotchTexture", splotchTexture);
  }
}
