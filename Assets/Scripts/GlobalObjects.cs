using UnityEngine;

public class GlobalObjects : MonoBehaviour
{
  private static GlobalObjects _instance;

  public static GlobalObjects Instance { get { return _instance; } }

  private void Awake()
  {
    if (_instance != null && _instance != this)
    {
      Destroy(gameObject);
    }
    else
    {
      _instance = this;
      DontDestroyOnLoad(gameObject);
    }
  }
}
