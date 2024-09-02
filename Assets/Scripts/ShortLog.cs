using UnityEngine;

public class ShortLog : Carryable, ICutable, Interactable
{
    [SerializeField] GameObject[] cutPoints;

    public Transform Placement { get; set; }

    public void Interract(Carryable carryable)
    {
        Debug.Log("Interract with ShortLog holding " + carryable?.name);
        // Start Carry and disable collider

        if (PlayerPickupAreaController.Instance.Carry(this))
        {
            Debug.Log("Picking Up ShortLog");
            DisablePhysics();
        }else
            Debug.Log("Can Not pick Up ShortLog");

    }

    public void Cut()
    {
        Debug.Log("Cutting ShortLog");
        foreach (GameObject pos in cutPoints)
        {
            ItemSpawner.Instance.CreateItemAt(ItemType.Post, pos.transform);
        }
        Destroy(gameObject);
    }

}
