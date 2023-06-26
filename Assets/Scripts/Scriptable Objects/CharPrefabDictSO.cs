using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CharPrefabLookup
{
  public char Character;
  public GameObject Prefab;
}

[CreateAssetMenu(menuName = "ScriptableObjects/CharPrefabDictSO")]
public class CharPrefabDictSO : ScriptableObject
{
  public List<CharPrefabLookup> CharPrefabs;

  public Dictionary<char, GameObject> CharPrefabDict
  {
    get
    {
      Dictionary<char, GameObject> dict = new Dictionary<char, GameObject>();
      foreach (CharPrefabLookup item in CharPrefabs)
      {
        dict[item.Character] = item.Prefab;
      }

      return dict;
    }
  }
}
