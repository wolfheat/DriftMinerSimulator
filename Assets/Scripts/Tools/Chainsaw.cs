using UnityEngine;

public class Chainsaw : Tool, Interactable
{
    public void Interract()
    {
        Debug.Log("Interract with Chainsaw");

        if (PlayerPickupAreaController.Instance.Carry(this))
        {
            Debug.Log("Picking Up Log");
            DisablePhysics();
        }
        else
            Debug.Log("Can Not pick Up log");

    }
}
