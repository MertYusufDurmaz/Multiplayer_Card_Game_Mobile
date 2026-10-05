using UnityEngine;
using DG.Tweening;

public class AtmosphereManager : MonoBehaviour
{
    public static AtmosphereManager Instance;

    [Header("Hareket Edecek Işık")]
    public Transform spotlight; 
    public float moveDuration = 1.0f; // Işığın dönme/gitme süresi

    [Header("Işık Hedefleri (Hedef Pozisyon ve Açı)")]
    public Transform targetPlayer;
    public Transform targetOpponent;
    public Transform targetCenter;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void FocusOnPlayer()
    {
        MoveLight(targetPlayer);
    }

    public void FocusOnOpponent()
    {
        MoveLight(targetOpponent);
    }

    public void FocusOnCenter()
    {
        MoveLight(targetCenter);
    }

    private void MoveLight(Transform target)
    {
        if (spotlight != null && target != null)
        {
            // Mevcut animasyonları durdur ki üst üste binmesin
            spotlight.DOKill();

            // Işığı yavaşça yeni pozisyona ve yeni açıya kaydır
            spotlight.DOMove(target.position, moveDuration).SetEase(Ease.InOutSine);
            spotlight.DORotateQuaternion(target.rotation, moveDuration).SetEase(Ease.InOutSine);
        }
    }
}