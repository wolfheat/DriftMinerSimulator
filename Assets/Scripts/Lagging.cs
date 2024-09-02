using System.Collections;
using UnityEngine;

public class Lagging : Carryable, Interactable,IGhost
{
    [SerializeField] GameObject[] cutPoints;
    public Transform Placement { get; set; }
    public void Interract(Carryable carryable)
    {
        Debug.Log("Interract with Lagging holding" + carryable?.name);
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
