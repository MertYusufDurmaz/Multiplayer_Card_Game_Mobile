using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    [Header("Havuz Ayarlari")]
    public GameObject cardPrefab;
    public int poolSize = 15;

    private readonly Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (cardPrefab == null)
        {
            Debug.LogWarning("ObjectPooler: cardPrefab is not assigned.");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(cardPrefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetCardFromPool(Vector3 position, Quaternion rotation)
    {
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(cardPrefab, position, rotation);

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        CardInteraction interaction = obj.GetComponent<CardInteraction>();
        if (interaction != null)
        {
            interaction.isPlayed = false;
        }

        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

        obj.SetActive(false);

        CardInteraction interaction = obj.GetComponent<CardInteraction>();
        if (interaction != null)
        {
            interaction.isPlayed = false;
        }

        if (!pool.Contains(obj))
        {
            pool.Enqueue(obj);
        }
    }
}