using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    public TMP_Text textComponent;

    public void Setup(string message, Color textColor)
    {
        if (textComponent == null) textComponent = GetComponent<TMP_Text>();
        
        textComponent.text = message;
        textComponent.color = textColor;

        // --- HAYAT KURTARAN SATIR (BILLBOARD EFEKTI) ---
        // Yazi dogdugu an kendi donus acisini, ana kameranin donus acisina esitler.
        // Boylece kamera tepeden de baksa, yandan da baksa yazi hep kameraya donuk (okunakli) olur!
        transform.rotation = Camera.main.transform.rotation;

        // --- DOTWEEN ŞOVU ---
        // 1. Kameraya (Y ekseninde) dogru yuksel
        transform.DOMoveY(transform.position.y + 1.5f, 1f).SetEase(Ease.OutQuad);
        
        // 2. Havaya kalkarken hafifce buyu.
        transform.DOScale(transform.localScale * 1.5f, 0.5f).SetLoops(2, LoopType.Yoyo);

        // 3. Yarim saniye sonra yazinin gorunurlugunu sifira indir (Fade Out) ve sil.
        textComponent.DOFade(0, 1f).SetDelay(0.5f).OnComplete(() => 
        {
            Destroy(gameObject); 
        });
    }
}