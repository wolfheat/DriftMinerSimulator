using System;
using UnityEngine;

public class Log : Carryable, Interactable
{

    public void Interract()
    {
        Debug.Log("Interract with Log");
        // Start Carry and disable collider

        if (PlayerPickupAreaController.Instance.Carry(this))
        {
            Debug.Log("Picking Up Log");
            DisablePhysics();
        }else
            Debug.Log("Can Not pick Up log");

    }

}
