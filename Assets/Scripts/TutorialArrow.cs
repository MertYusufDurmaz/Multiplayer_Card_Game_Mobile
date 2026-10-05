using UnityEngine;
using DG.Tweening;

public class TutorialArrow : MonoBehaviour
{
    private float startY;

    void Start()
    {
        startY = transform.position.y;
        
        // --- DOTWEEN ŞOVU ---
        // Y ekseninde 0.4 birim yukari cikip inmesini sagliyoruz.
        // SetLoops(-1, LoopType.Yoyo) komutu bu animasyonun sonsuza kadar git-gel yapmasini saglar.
        transform.DOMoveY(startY + 0.4f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}