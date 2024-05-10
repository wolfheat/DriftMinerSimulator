using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Interactable
{
    public void Interract();
}

public class Tree : MonoBehaviour, Interactable
{
    public void Interract()
    {
        Debug.Log("Interract with Tree");
        ItemSpawner.Instance.CreateItemAt(ItemType.Wood,transform.position);
        Destroy(gameObject);    
    }

}
