using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public class GenerateFontPrefabs : MonoBehaviour
{
  [ContextMenu("Generate font prefabs")]
  public void Generate()
  {
    var charPrefabLookupSO = Resources.Load<CharPrefabDictSO>("Scriptable Objects/CharPrefabLookupSO");
    UnityEngine.Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath("Assets/Sprites/PNG/Micro-Chat.png");
    charPrefabLookupSO.CharPrefabs.Clear();

    List<CharPrefabLookup> lookups = new List<CharPrefabLookup>();
    string characters = "BCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    for (int i = 0; i < characters.Length; i++)
    {
      char c = characters[i];
      CharPrefabLookup lookupEntry = new CharPrefabLookup();
      lookupEntry.Character = c;
      lookupEntry.Prefab = GeneratePrefabVariantFromChar(c);
      lookupEntry.Prefab.GetComponent<SpriteRenderer>().sprite = (Sprite)sprites[i + 1];
      lookups.Add(lookupEntry);
    }

    // Transfer them over to the SO
    foreach (var lookup in lookups)
    {
      charPrefabLookupSO.CharPrefabs.Add(lookup);
    }
  }

  private GameObject GeneratePrefabVariantFromChar(char c)
  {
    GameObject prefabRef = (GameObject)AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Font/A-upper.prefab");
    GameObject instanceRoot = (GameObject)PrefabUtility.InstantiatePrefab(prefabRef);
    // Because prefabs are case insensitive...
    string name = Char.IsUpper(c) ? $"{Char.ToUpper(c)}-upper" : $"{Char.ToUpper(c)}-lower";
    GameObject pVariant = PrefabUtility.SaveAsPrefabAsset(instanceRoot, $"Assets/Prefabs/Font/{name}.prefab");
    GameObject.DestroyImmediate(instanceRoot);
    return pVariant;
  }
}