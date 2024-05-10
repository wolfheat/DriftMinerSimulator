using UnityEngine;

public class Tree : Carryable, Interactable
{
    public void Interract()
    {
        Debug.Log("Interract with Tree");
        ItemSpawner.Instance.CreateItemAt(ItemType.Wood,transform.position);
        Destroy(gameObject);    
    }

}
