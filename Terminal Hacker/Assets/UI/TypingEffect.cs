using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TypingEffect : MonoBehaviour
{
    private TMP_Text text;
    [SerializeField] private float waitForSeconds = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMP_Text>();
        text.maxVisibleCharacters = 0;
        StartCoroutine(Type());
    }
    IEnumerator Type()
    {
        text.maxVisibleCharacters += 1;
        yield return new WaitForSeconds(waitForSeconds);
        if (text.maxVisibleCharacters < text.textInfo.characterCount)
        {
            StartCoroutine(Type());
        }
    }
}
