// Some actions need leeway time, e.g. coyote time, jump buffers, etc. 
public class Leeway
{
  private float amount;
  private float counter = 0f;

  public Leeway(float amount)
  {
    this.amount = amount;
  }

  public void Reset()
  {
    this.counter = amount;
  }

  public void Tick(float amount)
  {
    this.counter -= amount;
  }

  public bool Valid()
  {
    return this.counter > 0;
  }

  public void Invalidate()
  {
    this.counter = 0;
  }
}