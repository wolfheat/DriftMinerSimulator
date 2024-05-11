using UnityEngine;

public class Lagging : Carryable, Interactable
{
    [SerializeField] GameObject[] cutPoints;
    public void Interract()
    {
        Debug.Log("Interract with Lagging");
        // Start Carry and disable collider

        if (PlayerPickupAreaController.Instance.Carry(this))
        {
            Debug.Log("Picking Up Lagging");
            DisablePhysics();
        }
        else
            Debug.Log("Can Not pick Up Lagging");

    }

    public void Cut()
    {
        Debug.Log("Can not Cut Lagging any smaller");
    }

}
