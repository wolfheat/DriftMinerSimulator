using UnityEngine;

public class Tree : Carryable, Interactable
{
    public void Interract(Carryable carryable)
    {
        Debug.Log("Interract with Tree holding" + carryable.name);
        ItemSpawner.Instance.CreateItemAt(ItemType.Log,transform);
        Destroy(gameObject);    
    }

}
