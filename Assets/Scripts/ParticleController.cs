using UnityEngine;
using System.Collections.Generic;

public class ParticleController : StateObserver
{
  [SerializeField] private ParticleSystem gooExplosionParticles;
  [SerializeField] private ParticleSystem hoverParticles;
  [SerializeField] private int minExplosionEmitAmount = 5;
  [SerializeField] private int maxExplosionEmitAmount = 10;

  public override void OnLanded()
  {
    EmitParticles(minExplosionEmitAmount, maxExplosionEmitAmount);
  }

  public override void OnNotHovering()
  {
    hoverParticles.Stop();
  }

  public void OnHoverImportantFrame()
  {
    hoverParticles.Play();
  }

  // TODO: make this better!
  public void OnBashImportantFrame()
  {
    // Don't emit particles if our bash collided with the ground layer at all.
    // This helps prevent weird cases where we emit particles out of bounds by accident.
    List<Collider2D> bashColliders = PlayerAttackController.GetBashColliders();
    foreach (Collider2D collider in bashColliders)
    {
      if (collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
      {
        return;
      }
    }

    Vector3 originalPosition = gooExplosionParticles.transform.position;

    float yOffset = 8f / 16f;
    float xOffset = 8f / 16f;
    if (!PlayerController.FacingRight)
    {
      xOffset *= -1;
    }

    gooExplosionParticles.transform.position += new Vector3(xOffset, yOffset, 0);
    EmitParticles(minExplosionEmitAmount, maxExplosionEmitAmount);
    gooExplosionParticles.transform.position = originalPosition;
  }

  private void EmitParticles(int min, int max)
  {
    int amount = Random.Range(min, max + 1);
    gooExplosionParticles.Emit(amount);
  }
}