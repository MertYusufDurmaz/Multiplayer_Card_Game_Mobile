using UnityEngine;
using DG.Tweening;

public class CardInteraction : MonoBehaviour
{
    private Vector3 originalHandPosition;
    private Quaternion originalHandRotation; 
    private Vector3 originalScale; 
    
    private bool isDragging = false;
    private bool isReturning = false; 
    
    private Camera mainCamera;
    private TurnManager turnManager;
    private TutorialArrow tutorialArrow;
    
    [Header("Kart Durumu")]
    public bool isPlayed = false;

    [Header("Miknatis Ayarlari")]
    public Transform playAreaTarget; 
    public float magnetDistance = 2.5f; 

    void Start()
    {
        mainCamera = Camera.main;
        turnManager = FindObjectOfType<TurnManager>();
        tutorialArrow = FindObjectOfType<TutorialArrow>();
        originalScale = transform.localScale; 
        
        if (playAreaTarget == null)
        {
            GameObject pa = GameObject.Find("PlayArea");
            if (pa != null) playAreaTarget = pa.transform;
        }
    }

    public void UpdateOriginalPositionAndRotation(Vector3 pos, Quaternion rot)
    {
        originalHandPosition = pos;
        originalHandRotation = rot;
    }

    void OnMouseDown()
    {
        if (!CanInteract()) return;

        isDragging = true;
        transform.DOKill(); 

        if (tutorialArrow != null) tutorialArrow.Hide();

        transform.DOMoveY(originalHandPosition.y + 0.5f, 0.2f).SetEase(Ease.OutQuad);
        transform.DOScale(originalScale * 1.15f, 0.2f).SetEase(Ease.OutQuad); 
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, new Vector3(0, originalHandPosition.y + 1f, 0));
            
            if (plane.Raycast(ray, out float distance))
            {
                Vector3 newPos = ray.GetPoint(distance);
                transform.position = new Vector3(newPos.x, originalHandPosition.y + 1f, newPos.z);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * 10f);
            }
        }
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;
        
        if (playAreaTarget != null)
        {
            float dist = Vector3.Distance(transform.position, playAreaTarget.position);
            
            if (dist <= magnetDistance)
            {
                if (isPlayed || !turnManager.hasPlayedCardThisTurn)
                {
                    transform.DOKill(); 
                    Vector3 targetPos = playAreaTarget.position + new Vector3(0, 0.2f, 0);
                    
                    transform.DOMove(targetPos, 0.2f).SetEase(Ease.OutQuad);
                    transform.DORotate(new Vector3(90f, 0f, 0f), 0.2f);
                    transform.DOScale(originalScale, 0.2f); 

                    if (!isPlayed) 
                    {
                        isPlayed = true;
                        turnManager.RegisterPlayedCard(this); 
                        
                        transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f, 10, 1).SetDelay(0.2f);

                    }
                }
                else
                {
                    ReturnToHand();
                }
            }
            else
            {
                if (isPlayed)
                {
                    isPlayed = false;
                    turnManager.UnregisterPlayedCard(); 
                    
                    // Opsiyonel: Kart masadan geri cekilirse de log atabiliriz
                    // CombatLogManager.Instance.AddLog("Kart geri çekildi.", Color.gray);
                }
                ReturnToHand();
            }
        }
        else
        {
            ReturnToHand(); 
        }
    }

    private bool CanInteract()
    {
        return !(turnManager != null && turnManager.currentState != GameState.PlayerTurn) && !isReturning;
    }

    void ReturnToHand()
    {
        transform.DOKill();
        isReturning = true; 
        
        transform.DOMove(originalHandPosition, 0.4f).SetEase(Ease.OutBack);
        transform.DORotateQuaternion(originalHandRotation, 0.4f).SetEase(Ease.OutBack);
        
        transform.DOScale(originalScale, 0.4f).OnComplete(() => 
        {
            isReturning = false; 
        });
    }
}