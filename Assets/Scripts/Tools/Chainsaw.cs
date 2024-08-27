using UnityEngine;

public class Chainsaw : Tool, Interactable
{
    public Transform Placement { get; set; }
    public void Interract()
    {
        Debug.Log("Interract with Chainsaw");

        if (PlayerPickupAreaController.Instance.Carry(this))
        {
            Debug.Log("Picking Up Chainsaw");
            DisablePhysics();
        }
        else
            Debug.Log("Can Not pick Up Chainsaw");

    }
}
