using UnityEngine;

public class Log : Carryable, ICutable, Interactable
{
    [SerializeField] GameObject[] cutPoints;
    public void Interract(Carryable carryable)
    {
        Debug.Log("Interract with Log holding "+carryable?.name);
        // Start Carry and disable collider

        if (PlayerPickupAreaController.Instance.Carry(this))
        {
            Debug.Log("Picking Up Log");
            DisablePhysics();
        }else
            Debug.Log("Can Not pick Up log");

    }

    public void Cut()
    {
        Debug.Log("Cutting Log");
        foreach (GameObject pos in cutPoints)
        {
            ItemSpawner.Instance.CreateItemAt(ItemType.ShortLog, pos.transform);
        }
        Destroy(gameObject);
    }

}
