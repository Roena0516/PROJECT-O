using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    public Image fadeImage;

    public void FadeIn(float duration = 0.5f, Action onComplete = null)
    {
        // 페이드 효과 기능 제거: 즉시 콜백 실행
        onComplete?.Invoke();
    }

    public void FadeOut(float duration = 0.5f, Action onComplete = null)
    {
        // 페이드 효과 기능 제거: 즉시 콜백 실행
        onComplete?.Invoke();
    }
}
