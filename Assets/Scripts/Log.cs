using System;
using UnityEngine;

public class Log : MonoBehaviour, Interactable
{
    [SerializeField] CapsuleCollider capsuleCollider;
    [SerializeField] Rigidbody rb;

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

    public void Drop()
    {
        EnablePhysics();
    }
    private void EnablePhysics()
    {
        rb.isKinematic = false;
        capsuleCollider.enabled = true;
    }
    private void DisablePhysics()
    {
        rb.isKinematic = true;
        capsuleCollider.enabled = false;
    }
}
