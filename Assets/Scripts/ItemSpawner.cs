using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ItemType { Wood, ShortLog, Log, Post, Lagging };

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] GameObject holder;

    [SerializeField] GameObject logPrefab;
    [SerializeField] GameObject shortLogPrefab;
    [SerializeField] GameObject postPrefab;
    [SerializeField] GameObject laggingPrefab;

    public static ItemSpawner Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    public void CreateItemAt(ItemType type, Transform pos)
    {
        Debug.Log("Creating "+type+" at position "+pos);
        switch (type)
        {
            case ItemType.Wood:
                break;
            case ItemType.ShortLog:
                Instantiate(shortLogPrefab, pos.position, pos.rotation, holder.transform);
                break;
            case ItemType.Log:
                Instantiate(logPrefab, pos.position + Vector3.up * 5f, pos.rotation, holder.transform);
                break;
            case ItemType.Post:
                Instantiate(postPrefab, pos.position, pos.rotation, holder.transform);
                break;
            case ItemType.Lagging:
                Instantiate(laggingPrefab, pos.position, pos.rotation, holder.transform);
                break;
            default:
                break;
        }
    }
}
