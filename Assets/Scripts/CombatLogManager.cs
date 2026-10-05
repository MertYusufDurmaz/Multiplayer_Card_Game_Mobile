using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CombatLogManager : MonoBehaviour
{
    public static CombatLogManager Instance;

    private const int MaxLogEntries = 50;
    private readonly System.Collections.Generic.List<GameObject> logEntries = new System.Collections.Generic.List<GameObject>();

    [Header("UI Baglantilari")]
    public GameObject logTextPrefab;
    public Transform contentTransform;
    public ScrollRect scrollRect;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddLog(string message, Color textColor)
    {
        if (logTextPrefab == null || contentTransform == null || scrollRect == null)
        {
            return;
        }

        if (logEntries.Count >= MaxLogEntries)
        {
            GameObject oldest = logEntries[0];
            logEntries.RemoveAt(0);
            if (oldest != null)
            {
                Destroy(oldest);
            }
        }

        GameObject newLog = Instantiate(logTextPrefab, contentTransform);
        logEntries.Add(newLog);

        TMP_Text txt = newLog.GetComponent<TMP_Text>();
        if (txt != null)
        {
            txt.text = message;
            txt.color = textColor;
        }

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}