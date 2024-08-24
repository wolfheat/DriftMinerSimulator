using System.Collections;
using UnityEngine;

public class Lagging : Carryable, Interactable,IGhost
{
    [SerializeField] GameObject visibles;
    [SerializeField] GameObject[] cutPoints;
    public Transform Placement { get; set; }
    public void Interract()
    {
        Debug.Log("Interract with Lagging");
        // Start Carry and disable collider

        if (PlayerPickupAreaController.Instance.Carry(this))
        {
            Debug.Log("Picking Up Lagging");
            DisablePhysics();
        }
        else
            Debug.Log("Can Not pick Up Lagging");

    }

    public void Cut()
    {
        Debug.Log("Can not Cut Lagging any smaller");
    }

    Coroutine coroutine = null;
    public void ActivateVisibleCountDown()
    {
        // reset the timer
        timer = CountTime;
        if (coroutine == null)
            coroutine = StartCoroutine(MakeVisibleCO());
    }

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
    private float timer = 0;
    private const float CountTime = 0.2f;
    
}
