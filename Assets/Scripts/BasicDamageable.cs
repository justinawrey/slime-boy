using UnityEngine;

public class BasicDamageable : MonoBehaviour, IDamageable
{
  [SerializeField] private float health = 1f;

  public void TakeHit(float amount)
  {
    health -= amount;

    if (health <= 0)
    {
      Die();
    }
  }

  public void Die()
  {
    print("Fuck, I'm ded.");
  }
}