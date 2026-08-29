using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class InfoText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float _displayDuration = 2f;
    [SerializeField] private float _fadeSpeed = 2f;
    private Coroutine coroutine;

    private void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        SetAlpha(0f);
    }
    private void OnEnable()
    {
        PlayerStats.OnShowInfo += ShowMessage;
        EnemySpawner.OnWaveCompletedText += ShowMessage;
    }
    private void OnDisable()
    {
        PlayerStats.OnShowInfo -= ShowMessage;
        EnemySpawner.OnWaveCompletedText -= ShowMessage;

    }
    private void ShowMessage(string message)
    {
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        coroutine = StartCoroutine(ShowMessageRoutine(message));
    }
    
    private IEnumerator ShowMessageRoutine(string message)
    {
        _text.text = message;
        float currentAlpha = _text.color.a;
        while(currentAlpha < 1f)
        {
            currentAlpha += Time.deltaTime * _fadeSpeed;
            SetAlpha(currentAlpha);
            yield return null;
        }
        yield return new WaitForSeconds(_displayDuration);

        while (currentAlpha > 0f)
        {
            currentAlpha -= Time.deltaTime * _fadeSpeed;
            SetAlpha(currentAlpha);
            yield return null;
        }
        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        Color color = _text.color;
        color.a = Mathf.Clamp01(alpha);
        _text.color = color;
    }
}
