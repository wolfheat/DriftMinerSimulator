using UnityEngine;

public class Shovel : Tool, Interactable
{
    public void Interract()
    {
        Debug.Log("Interract with Shovel");

        if (PlayerPickupAreaController.Instance.Carry(this))
        {
            Debug.Log("Picking Up Shovel");
            DisablePhysics();
        }
        else
            Debug.Log("Can Not pick Up Shovel");

    }
}
