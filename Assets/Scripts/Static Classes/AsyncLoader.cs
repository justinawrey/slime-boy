using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AsyncLoader
{
  public static IEnumerator LoadSceneRoutine(string name)
  {
    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name);
    while (!asyncLoad.isDone)
    {
      yield return null;
    }
  }
}