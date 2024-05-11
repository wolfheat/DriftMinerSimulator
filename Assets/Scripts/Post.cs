using UnityEngine;

public class Post : Carryable, Interactable
{
    [SerializeField] GameObject[] cutPoints;
    public void Interract()
    {
        Debug.Log("Interract with Post");
        // Start Carry and disable collider

        if (PlayerPickupAreaController.Instance.Carry(this))
        {
            Debug.Log("Picking Up Post");
            DisablePhysics();
        }else
            Debug.Log("Can Not pick Up Post");

    }

    public void Cut()
    {
        Debug.Log("Cutting Post");
        foreach (GameObject pos in cutPoints)
        {
            ItemSpawner.Instance.CreateItemAt(ItemType.Lagging, pos.transform);
        }
        Destroy(gameObject);
    }

}
