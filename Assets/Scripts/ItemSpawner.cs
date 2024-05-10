using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ItemType { Wood};

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] GameObject holder;

    [SerializeField] GameObject woodPrefab;

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


    public void CreateItemAt(ItemType type, Vector3 pos)
    {
        Debug.Log("Creating "+type+" at position "+pos);
        Instantiate(woodPrefab,pos+Vector3.up*5f,Quaternion.identity,holder.transform);
    }
}
