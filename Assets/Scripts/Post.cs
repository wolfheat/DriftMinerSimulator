using System;
using UnityEngine;

public class Post : Carryable, Interactable
{
    [SerializeField] GameObject[] cutPoints;
    [SerializeField] Transform[] connectPoints;
    public Transform placement;

    public void Interract()
    {
        Debug.Log("Interract with Post");
        // Start Carry and disable collider

        if (PlayerPickupAreaController.Instance.Carry(this))
        {
            Debug.Log("Picking Up Post");
            DisablePhysics();
        }else
            Debug.Log("Can Not pick Up Post");

    }

    public void Cut()
    {
        Debug.Log("Cutting Post");
        foreach (GameObject pos in cutPoints)
        {
            ItemSpawner.Instance.CreateItemAt(ItemType.Lagging, pos.transform);
        }
        Destroy(gameObject);
    }
    
    public Transform GetConnectpoint(Vector3 refPoint)
    {
        Debug.Log("Ref point is "+refPoint);
        Transform bestConnect = transform;
        float bestDistance = 10f;
        foreach (Transform t in connectPoints)
        {
            Debug.Log("checking point " + t.position);
            float dist = Vector3.Distance(refPoint, t.position);
            if (dist < bestDistance)
            {
                bestConnect = t;
                bestDistance = dist;
            }
        }
        return bestConnect;
    }

}
