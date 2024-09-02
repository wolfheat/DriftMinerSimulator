using UnityEngine;

public class Chainsaw : Tool, Interactable
{
    public Transform Placement { get; set; }
    public void Interract(Carryable carryable)
    {
        Debug.Log("Interract with Chainsaw holding " + carryable?.name);

        if (PlayerPickupAreaController.Instance.Carry(this))
        {
            Debug.Log("Picking Up Chainsaw");
            DisablePhysics();
        }
        else
            Debug.Log("Can Not pick Up Chainsaw");

    }
}
