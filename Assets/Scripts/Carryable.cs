using System;
using System.Collections;
using UnityEngine;

public class Carryable : Item, IDroppable
{
    [SerializeField] Collider col;
    [SerializeField] Rigidbody rb;
    [SerializeField] Collider[] interractColliders;
    [SerializeField] bool isGhost;
    [SerializeField] GameObject placement;
    [SerializeField] GameObject visibles;
    public bool IsGhost => isGhost;
    public bool Placed { get; private set; } = false;   
    public Transform Placement { get { return placement.transform;}}

    public void Drop()
    {
        EnablePhysics();
    }
    private void EnablePhysics()
    {
        rb.isKinematic = false;
        col.enabled = true;
        foreach (Collider interactCollider in interractColliders)
            interactCollider.enabled = true;
    }
    public void DisablePhysics()
    {
        rb.isKinematic = true;
        col.enabled = false;
        foreach (Collider interactCollider in interractColliders)
            interactCollider.enabled = false;
        Placed = false;
    }

    Coroutine coroutine = null;
    private float timer = 0;
    private const float CountTime = 0.2f;
    public void ActivateVisibleCountDown()
    {
        Debug.Log("Activating Ghost Timer");
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
        transform.position = Vector3.up * 30f;
    }

    public bool DidPosition(Vector3 newPosition, Quaternion newRotation)
    {
        // If attempting to place on occupied position abort
        if (!StructuresHolder.PositionFree<Post>(newPosition))
        {
            Debug.Log("Position is not Free");
            return false;
        }
        transform.position = newPosition;
        transform.rotation = newRotation;
        Debug.Log("Placing the item ");
        //Place();

        // IF this is a ghost then its not Placed in the scene only positioned

        Debug.Log("Placing the item at " + newPosition + " with rotation " + newRotation + " item is at " + transform.position);

        if (isGhost)
        {

            ActivateVisibleCountDown();
        }
        else
        {
            transform.SetParent(StructuresHolder.Instance.transform, true);
        }

        return true;
    }


    public void Place()
    {
        rb.isKinematic = true;
        col.enabled = true;
        foreach (Collider interactCollider in interractColliders)
            interactCollider.enabled = true;
        Placed = true;
    }

    internal bool TryToPosition(Vector3 playerPos, Carryable activeCarryable, Transform placePoint)
    {
        Vector3 playerAngle = playerPos - placePoint.position;
        // Places aligned to other objects
        Vector3 cardinalTowardsPlayer = Wolfheat.Convert.AlignCardinal(playerAngle);

        Debug.Log("Trying to Place item "+activeCarryable.name+" at target carryable item "+this.name);
        
        // Away movement is only for Laggings
        Vector3 away = Vector3.zero;

        Quaternion newRotation = Quaternion.LookRotation(cardinalTowardsPlayer, Wolfheat.Convert.Away(transform.position - placePoint.position));
        Vector3 newPosition = Vector3.zero;
        Vector3 forward = transform.forward;
        Vector3 planeTowards = Wolfheat.Convert.PlaneTowards(playerAngle - playerPos);        


        Debug.Log("Forward for the already placed item is "+forward );
        if (activeCarryable is Lagging)
        {
            if (Math.Abs(forward.y) < 0.1f)
            {
                Debug.Log("The already placed item is in the plane, adding lagging   ");
                away = Vector3.up * 0.2f;
                newRotation = Quaternion.LookRotation(planeTowards, Vector3.up);
            }
            else
            {
                Debug.Log("The already placed item is vertical");
                away = Wolfheat.Convert.Away(playerAngle) * 0.2f;
                Debug.Log("Away became "+away);
                newRotation = Quaternion.LookRotation(planeTowards, away);

            }
        }
        newPosition = placePoint.position - activeCarryable.Placement.transform.localPosition.z * cardinalTowardsPlayer - activeCarryable.Placement.transform.localPosition.y * cardinalTowardsPlayer;
        //newPosition = placePoint.position + away - activeCarryable.transform.forward* activeCarryable.Placement.transform.localPosition.z;
        newPosition = newPosition + away;
        // Place if Lagging?
        /*
        else
        {
            // Places unrelated
            Carryable placeObject = carrying[0];

            newPosition = placePoint.position - placeObject.Placement.transform.localPosition.y * placePoint.transform.up;
            newRotation = placePoint.rotation;
            placeObject.transform.parent = StructuresHolder.Instance.transform;
        }*/

        // Item now has a position and rotation to go to - check if its valid and unoccupied
        return activeCarryable.DidPosition(newPosition, newRotation);
    }
}
