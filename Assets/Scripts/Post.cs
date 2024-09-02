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
    public Transform GetConnectpoint(Vector3 vector3, Carryable placedItem);

}

public class Post : Carryable, ICutable, Interactable, IHAveConnectionPoint, IGhost
{
    [SerializeField] GameObject visibles;
    [SerializeField] GameObject[] cutPoints;
    [SerializeField] Transform[] postConnectPoints;
    [SerializeField] Transform[] laggingConnectPoints;


    public void Interract(Carryable carryable)
    {
        Debug.Log("Interract with Post holding "+carryable?.name);
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
        if (!Placed)
        {
            Debug.Log("Item is not placed");
            return;
        }
        foreach (GameObject pos in cutPoints)
        {
            ItemSpawner.Instance.CreateItemAt(ItemType.Lagging, pos.transform);
        }
        Destroy(gameObject);
    }
        
    public Transform GetConnectpoint(Vector3 refPoint, Carryable itemToPlace)
    {
        Transform bestConnect = transform;
        if(itemToPlace is Lagging)
            Debug.Log("Item to place is Lagging");
        if (!(itemToPlace is Post || itemToPlace is Lagging))
            return bestConnect;

        float bestDistance = 10f;
        foreach (Transform t in (itemToPlace is Post ? postConnectPoints : laggingConnectPoints))
        {
            float dist = Vector3.Distance(refPoint, t.position);
            if (dist < bestDistance)
            {
                bestConnect = t;
                bestDistance = dist;
            }
        }
        Debug.Log("Determined best connect point at "+bestConnect+" distance from "+refPoint+" = "+bestDistance+" showing Visualizer at "+bestConnect.position);
        Visualizer.ShowAt(bestConnect.position);
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
