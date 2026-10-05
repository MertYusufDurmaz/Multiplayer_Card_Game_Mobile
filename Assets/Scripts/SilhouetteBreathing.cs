using UnityEngine;
using DG.Tweening;

public class SilhouetteBreathing : MonoBehaviour
{
    void Start()
    {
        // 1. Y ekseninde (yukarı-aşağı) yavaşça nefes alıp verme
        transform.DOMoveY(transform.position.y + 0.15f, 2f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        // 2. Çok hafif ileri-geri esneme (Gerginlik hissi)
        transform.DOScale(transform.localScale * 1.02f, 2.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
}