using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public enum LetterEffect
{
  None,
  Shake,
  Wave,
  FadeOut,
}

public class Letter
{
  private char character;
  private GameObject prefab;
  private LetterEffect effect;
  private const float padding = 1f / 16f;

  public Letter(char _character, LetterEffect _effect = LetterEffect.None)
  {
    character = _character;
    effect = _effect;
    prefab = DialogueManager.CharPrefabLookup[character];
  }

  public float Width
  {
    get
    {
      return prefab.GetComponent<SpriteRenderer>().bounds.size.x - padding;
    }
  }

  public void Instantiate(Vector2 pos, DialogueBox parent)
  {
    GameObject letterObj = GameObject.Instantiate(prefab, pos, Quaternion.identity);
    letterObj.GetComponent<LetterEffects>().AddEffect(effect);
    letterObj.transform.SetParent(parent.transform);
  }
}

public class Text
{
  private string raw;

  // A map containing string index to possible letter effect
  public Dictionary<int, LetterEffect> EffectMap = new Dictionary<int, LetterEffect>();
  public string Parsed;

  public Text(string _raw)
  {
    raw = _raw;
    ParseRaw(raw);
  }

  private List<string> ParseLetterEffect(string strSource, LetterEffect effect)
  {
    List<string> parsedOut = new List<string>();
    string currStr = strSource;
    string startSearchStr, endSearchStr;

    switch (effect)
    {
      case LetterEffect.Shake:
        startSearchStr = "<shake>";
        endSearchStr = "</shake>";
        break;
      case LetterEffect.Wave:
        startSearchStr = "<wave>";
        endSearchStr = "</wave>";
        break;
      case LetterEffect.None:
      case LetterEffect.FadeOut:
      default:
        return parsedOut;
    }

    while (currStr.Contains(startSearchStr) && currStr.Contains(endSearchStr))
    {
      int start, end;
      start = currStr.IndexOf(startSearchStr, 0) + startSearchStr.Length;
      end = currStr.IndexOf(endSearchStr, start);
      string between = strSource.Substring(start, end - start);
      currStr = currStr.Substring(0, currStr.IndexOf(startSearchStr, 0)) + between + currStr.Substring(end + endSearchStr.Length);
      parsedOut.Add(between);
    }

    Parsed = currStr;
    return parsedOut;
  }

  private void ParseRaw(string raw)
  {
    Parsed = raw;
    var shakePhrases = ParseLetterEffect(Parsed, LetterEffect.Shake);
    var wavePhrases = ParseLetterEffect(Parsed, LetterEffect.Wave);

    foreach (var phrase in shakePhrases)
    {
      int startIdx = Parsed.IndexOf(phrase, 0);
      for (int i = startIdx; i < startIdx + phrase.Length; i++)
      {
        EffectMap.Add(i, LetterEffect.Shake);
      }
    }

    foreach (var phrase in wavePhrases)
    {
      int startIdx = Parsed.IndexOf(phrase, 0);
      for (int i = startIdx; i < startIdx + phrase.Length; i++)
      {
        EffectMap.Add(i, LetterEffect.Wave);
      }
    }
  }
}

public class DialogueBox : MonoBehaviour
{
  [SerializeField] private float spaceWidth = 1f;
  [SerializeField] private float lineHeight = 1f;
  [SerializeField] private float paddingTop = 0.5f;
  [SerializeField] private float paddingRight = 0.5f;
  [SerializeField] private float paddingLeft = 0.5f;
  [SerializeField] private float typeWritingInterval = 0.1f;
  [SerializeField] private float fullStopTypeWritingInterval = 1f;
  [SerializeField] private GameObject caret;
  private float originalTypeWritingInterval;
  private float originalFullStopTypeWritingInterval;
  private bool isTypeWriting = false;

  private Vector2 cursor;
  private SpriteRenderer spriteRenderer;

  private float minX
  {
    get
    {
      return spriteRenderer.bounds.min.x + paddingLeft;
    }
  }

  private float maxX
  {
    get
    {
      return spriteRenderer.bounds.max.x - paddingRight;
    }
  }

  private float maxY
  {
    get
    {
      return spriteRenderer.bounds.max.y - paddingTop;
    }
  }

  private void Awake()
  {
    spriteRenderer = GetComponent<SpriteRenderer>();
    originalTypeWritingInterval = typeWritingInterval;
    originalFullStopTypeWritingInterval = fullStopTypeWritingInterval;
    ResetCursor();
    DisableCaret();
  }

  public void EnableCaret()
  {
    caret.GetComponent<SpriteRenderer>().enabled = true;
  }

  public void DisableCaret()
  {
    caret.GetComponent<SpriteRenderer>().enabled = false;
  }

  public void Clear()
  {
    ResetCursor();
    DisableCaret();
    typeWritingInterval = originalTypeWritingInterval;
    fullStopTypeWritingInterval = originalFullStopTypeWritingInterval;

    foreach (Transform child in transform)
    {
      // Don't ever destroy the caret when clearing.
      if (child.CompareTag("DialogueCaret"))
      {
        continue;
      }

      Destroy(child.gameObject);
    }
  }

  private void ResetCursor()
  {
    cursor = new Vector2(minX, maxY);
  }

  public bool IsTypeWriting()
  {
    return isTypeWriting;
  }

  public void FinishTypeWriting()
  {
    typeWritingInterval = 0f;
    fullStopTypeWritingInterval = 0f;
  }

  public void StartTypeWriting(string content)
  {
    StartCoroutine(TypeWritingRoutine(content));
  }

  private IEnumerator TypeWritingRoutine(string content)
  {
    isTypeWriting = true;
    Text text = new Text(content);

    for (int i = 0; i < text.Parsed.Length; i++)
    {
      char character = text.Parsed[i];

      // equivalent to " "
      if (character == (char)32)
      {
        MoveCursorBy(new Vector2(spaceWidth, 0));

        if (text.Parsed[i - 1] == ".".ToCharArray()[0])
        {
          // Instead of a single long WaitForSeconds, use a bunch of short ones
          // so the routine can be responsively interrupted by player input
          int totalLoops = Mathf.FloorToInt(fullStopTypeWritingInterval / typeWritingInterval);
          for (int idx = 0; idx < totalLoops; idx++)
          {
            yield return new WaitForSeconds(typeWritingInterval);
          }
        }
        else
        {
          yield return new WaitForSeconds(typeWritingInterval);
        }

        continue;
      }

      // Look forward word to see if the word fits
      if (!FitsOnLine(text.Parsed, i))
      {
        MoveCursorBy(new Vector2(minX - cursor.x, -lineHeight));
      }

      Letter letter;
      if (text.EffectMap.ContainsKey(i))
      {
        letter = new Letter(character, text.EffectMap[i]);
      }
      else
      {
        letter = new Letter(character);
      }

      WriteAtCursor(letter);
      MoveCursorBy(new Vector2(letter.Width, 0));

      yield return new WaitForSeconds(typeWritingInterval);
    }

    isTypeWriting = false;
    EnableCaret();
  }

  private bool FitsOnLine(string text, int index)
  {
    string restOfText = text.Substring(index);
    string firstWord = restOfText.Split(" ")[0];

    float width = 0;
    foreach (char c in firstWord)
    {
      Letter letter = new Letter(c);
      width += letter.Width;
    }

    return cursor.x + width < maxX;
  }

  private void MoveCursorBy(Vector2 displacement)
  {
    cursor += displacement;
  }

  private void WriteAtCursor(Letter letter)
  {
    letter.Instantiate(cursor, this);
  }
}
