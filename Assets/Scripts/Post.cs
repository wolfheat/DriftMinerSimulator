using System;
using System.Collections;
using UnityEngine;

public class Post : Carryable, Interactable
{
    [SerializeField] GameObject visibles;
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

    Coroutine coroutine = null;
    internal void ActivateVisibleCountDown()
    {
        // reset the timer
        timer = CountTime;
        if(coroutine == null)
            coroutine = StartCoroutine(MakeVisibleCO());
    }

    private float timer = 0;
    private const float CountTime = 0.2f;

    private IEnumerator MakeVisibleCO()
    {
        visibles.SetActive(true);
        while (timer > 0)
        {
            yield return null;
            timer -= Time.deltaTime;
        }

        visibles.SetActive(false);
        coroutine = null;
    }
}
