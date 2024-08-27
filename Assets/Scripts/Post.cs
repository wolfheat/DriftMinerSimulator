using System;
using System.Collections;
using UnityEngine;

public interface IGhost
{
    public Transform Placement { get;}
    public void ActivateVisibleCountDown();

}

public interface IHAveConnectionPoint
{
    public Transform GetConnectpoint(Vector3 vector3);

}

public class Post : Carryable, ICutable, Interactable, IHAveConnectionPoint, IGhost
{
    [SerializeField] GameObject visibles;
    [SerializeField] GameObject[] cutPoints;
    [SerializeField] Transform[] connectPoints;
    [SerializeField] Transform[] laggingConnectPoints;

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
    
    public Transform GetLaggingConnectpoint(Vector3 refPoint)
    {
        Transform bestConnect = null;
        
        float bestDistance = 10f;
        foreach (Transform t in laggingConnectPoints)
        {
            float dist = Vector3.Distance(refPoint, t.position);
            if (dist < bestDistance)
            {
                bestConnect = t;
                bestDistance = dist;
            }
        }
        
        return bestConnect;
    }
    
    public Transform GetConnectpoint(Vector3 refPoint)
    {
        Transform bestConnect = transform;
        
        float bestDistance = 10f;
        foreach (Transform t in connectPoints)
        {
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
    public void ActivateVisibleCountDown()
    {
        // reset the timer
        timer = CountTime;
        if(coroutine == null)
            coroutine = StartCoroutine(MakeVisibleCO());
    }

    private float timer = 0;
    private const float CountTime = 0.2f;
    private const float radius = 0.2f;

    public float Radius { get => radius; }

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
