using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerPickupAreaController : MonoBehaviour
{
    [SerializeField] UIController uIController;
    [SerializeField] CarvingBox hitShowPrefab;
    [SerializeField] GameObject hitShowParent;
    [SerializeField] GameObject itemsParent;
    [SerializeField] Torch torchPrefab;
    [SerializeField] GameObject tilt;
    [SerializeField] LayerMask mask;
    [SerializeField] LayerMask interactablesMask;

    [SerializeField] GameObject carryPosition;
    [SerializeField] GameObject toolPosition;
    [SerializeField] List<Carryable> carrying = new();


    public static PlayerPickupAreaController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }



    private void OnTriggerStay(Collider other)
    {
       
    }

    private void OnEnable()
    {
        Inputs.Instance.Controls.Player.RClick.started += RightClick;
        Inputs.Instance.Controls.Player.Click.started += Click;
        Inputs.Instance.Controls.Player.Scroll.started += Scroll;
    }

    private void OnDisable()
    {
        Inputs.Instance.Controls.Player.RClick.started -= RightClick;
        Inputs.Instance.Controls.Player.Click.started -= Click;
        Inputs.Instance.Controls.Player.Scroll.started -= Scroll;
    }

    private void Scroll(InputAction.CallbackContext context)
    {
        Vector2 val = Mouse.current.scroll.ReadValue();
        Debug.Log("Scroll: " + val);
        scrollValue += val.y > 0 ? 1 : -1;
        if(scrollValue<0) scrollValue = 0;
        UIController.Instance.SetScrollStateInfo(scrollValue);
    }
    int scrollValue = 0;

    private void RightClick(InputAction.CallbackContext context) => RightClickActionAtPoint();
    private void Click(InputAction.CallbackContext context) => Interact();

    bool allowAction = true;
    float allowTimer = 0f;
    const float AllowTime = 0.1f;

    private void Update()
    {
        if (allowTimer > 0) { 
            allowTimer -= Time.deltaTime;
            if(allowTimer <= 0)
                allowAction = true;
        }
        if (shrink)
        {
            ShrinkStack();
        }

        if (Inputs.Instance.Controls.Player.RClick.ReadValue<float>() != 0f)
        {
            RightClickActionAtPoint();
        }
        GhostUpdate();
    }

    private void PlaceTorchAt(Vector3 pos) => Instantiate(torchPrefab, pos, Quaternion.identity, itemsParent.transform);


    private void GhostUpdate()
    {
        if (carrying.Count == 0) return;
        Interact(true);
    }

    [SerializeField] Post ghostpost;
    [SerializeField] Lagging ghostlagging;
    [SerializeField] Log ghostLog;
    [SerializeField] ShortLog ghostShortLog;
    [SerializeField] Carryable activeGhost;

    private bool shrink = false;

    private void Interact(bool useGhost=false)
    {
        if (!allowAction) return;

        if (!useGhost)
            Debug.Log("Interract: ");

        // Raycast forward from camera
        if (Physics.Raycast(tilt.transform.position, tilt.transform.forward, out RaycastHit hit, PlayerStats.PlayerReach, interactablesMask))
        {
            // Check if item hit is an interactable item
            Interactable interactable = hit.collider.GetComponent<Interactable>() ?? hit.collider.GetComponentInParent<Interactable>();

            // Return if interactable is null
            if (interactable == null)
            {
                Debug.LogWarning("This hit does not contain a GridVisualizer");
                return;
            }

            // Timer when interacting with item
            SetTimer(useGhost);

            // Is the interactable item not placed in world or are you intereacting to show a ghost view as placement?
            //if (!((Carryable)interactable).Placed || useGhost)
            //      return;

            // Check if player is holding an item

            if (carrying.Count == 0)
            {
                // Interacting with object without holding an item
                if (!useGhost)
                    interactable.Interract();
                return;
            }

            Carryable carryable = (interactable as Carryable);

            // Carrying something and interacting with something
            if (!useGhost)
            {
                if (carrying[0] is Chainsaw)
                {
                    Debug.Log("Using chainsaw on "+interactable);

                    ICutable cutable = interactable as ICutable;
                    if (cutable != null)
                    {
                        Debug.Log("Cutting Cuttable");
                        cutable.Cut();
                    }
                    return;
                }
                // IF item is the same as the carried pick it up if its not placed
                /*
                if (!carryable.Placed && SameType(carrying[0], interactable))
                        interactable.Interract();
                */
            }



            // Try to pick up the item
            if (!useGhost && TryPickUp(interactable,carryable))
                return;

            // Dont show shost on unplaced objects
            if (useGhost && !carryable.Placed)
                return;
                        
            
            // Placing item on a place point that belongs to the target
            Transform placePoint = (interactable as IHAveConnectionPoint).GetConnectpoint(hit.point);

            // if this gameobject has children toggle through them when scrolling
            ConnectPoints[] spots = placePoint.GetComponentsInChildren<ConnectPoints>();

            // Place depending on scroll
            placePoint = spots[scrollValue % spots.Length].transform;

            // Place acording to best direction
            Vector3 playerDirection = transform.position - placePoint.position;

            float dot = -1;

            foreach (var spot in spots)
            {
                float newDot = Vector3.Dot(playerDirection, spot.transform.up);
                if (newDot > dot)
                {
                    dot = newDot;
                    placePoint = spot.transform;
                }
            }

            Carryable activeCarryable = useGhost ? SetGhostItem(carrying[0]) : carrying[0];

            Vector3 newPosition = new();
            Quaternion newRotation = new();


            // Show what should be shown at the decided position

            //activeGhost = SetGhostItem(carrying[0]);
            // Set correct ghostitem
            if (useGhost && activeCarryable == null)
                return;

            // Places aligned to other objects
            Vector3 cardinalTowardsPlayer = Wolfheat.Convert.AlignCardinal(transform.position - placePoint.position);
            newPosition = placePoint.position - activeCarryable.Placement.transform.localPosition.z * cardinalTowardsPlayer - activeCarryable.Placement.transform.localPosition.y * cardinalTowardsPlayer;
            newRotation = Quaternion.LookRotation(cardinalTowardsPlayer, placePoint.transform.up);

            /*
            else
            {
                // Places unrelated
                Carryable placeObject = carrying[0];

                newPosition = placePoint.position - placeObject.Placement.transform.localPosition.y * placePoint.transform.up;
                newRotation = placePoint.rotation;
                placeObject.transform.parent = StructuresHolder.Instance.transform;
            }*/

            // If attempting to place on occupied position abort
            if (!useGhost && !PositionFree<Post>(newPosition))
            {
                Debug.Log("Position is not Free");
                return;
            }

            activeCarryable.transform.position = newPosition;
            activeCarryable.transform.rotation = newRotation;

            Debug.Log("Showing "+ activeCarryable.name+" at "+newPosition);

            //activeCarryable.transform.parent = StructuresHolder.Instance.transform;
            
            if(!useGhost)
                activeCarryable.transform.SetParent(StructuresHolder.Instance.transform,true);
            
            Debug.Log("Placing the item at "+newPosition+" with rotation "+newRotation+" item is at "+activeCarryable.transform.position);

            if (useGhost)
                ghostpost.ActivateVisibleCountDown();
            else
            {
                carrying[0].Place();
                carrying.RemoveAt(0);

                UpdateCarrierUI();
                ghostpost.transform.position = Vector3.up * 10f;
            }

            // END
            /// Does this place lagging on a post?
            /// 

            /*

            if (carrying.Count>0 && carrying[0] is Lagging && interactable is Post)
            {
                Post stationary = (Post)interactable;
                if (!stationary.Placed)
                {
                    Debug.Log("Target Post is not placed");
                    return;
                }

                bool topSide = true;
                if (Math.Abs(Vector3.Dot(stationary.transform.up, Vector3.up)) > 0.5f)
                {
                    //return;
                    topSide = false;
                }

                // Check for valid placement point in post
                placePoint = stationary.GetLaggingConnectpoint(hit.point);
                if (placePoint == null)
                {
                    Debug.Log("No Valid place point for lagging on this post");
                    return;
                }




                // Get rotation in best cardinal direction

                Vector3 cardinalTowardsPlayer = Wolfheat.Convert.AlignCardinal(transform.position - placePoint.position);

                if (Math.Abs(Vector3.Dot(stationary.transform.up, cardinalTowardsPlayer)) > 0.2f)
                {
                    //Debug.Log("PLayer is not perpendicular to the post");
                    return;
                }

                newPosition = new();
                newRotation = new();

                // Show what should be shown at the decided position
                if (topSide)
                {
                    newPosition = placePoint.position + stationary.Radius * Vector3.up + cardinalTowardsPlayer * 1.5f;
                    newRotation = Quaternion.LookRotation(cardinalTowardsPlayer, Vector3.up);
                }
                else
                {
                    Vector3 away = Wolfheat.Convert.Away(transform.position - placePoint.position);
                    // Place on outside (furthest side) on wall
                    newPosition = placePoint.position + stationary.Radius * away + cardinalTowardsPlayer * 1.5f;
                    newRotation = Quaternion.LookRotation(cardinalTowardsPlayer, away);
                }

                // If attempting to place on occupied position abort
                if (!useGhost && !PositionFree<Lagging>(newPosition))
                {
                    Debug.Log("Position is not Free");
                    return;
                }

                activeCarryable.transform.position = newPosition;
                activeCarryable.transform.rotation = newRotation;
                activeCarryable.transform.parent = StructuresHolder.Instance.transform;

                if (useGhost)
                    ghostlagging.ActivateVisibleCountDown();
                else
                {
                    carrying[0].Place();
                    carrying.RemoveAt(0);
                    UpdateCarrierUI();
                    ghostlagging.transform.position = Vector3.up * 10f;
                }

            }*/

            // Interact with soil? 

            // Raycast forward from camera
            if (Physics.Raycast(tilt.transform.position, tilt.transform.forward, out hit, PlayerStats.PlayerReach, mask))
            {
                Chunk hitChunk = hit.collider.GetComponent<Chunk>();

                if (hitChunk == null)
                {
                    Debug.LogWarning("This hit does not contain a GridVisualizer");
                    return;
                }

                if (carrying.Count > 0)
                {
                    if (!useGhost && carrying[0] is Shovel)
                    {
                        if (Inputs.Instance.Controls.Player.Shift.ReadValue<float>() > 0f)
                        {
                            Debug.Log("Placing dirt");
                            CarveAt(hit.point, hitChunk, set: 1);
                        }
                        else
                        {
                            Debug.Log("Shoveling dirt");
                            CarveAt(hit.point, hitChunk);
                        }
                    }
                    else if (carrying[0] is Post)
                    {
                        Debug.Log("Place the Post");

                        if (!useGhost)
                        {
                            Post post = carrying[0] as Post;
                            // Convert position to closest grid point ALIGN
                            post.transform.position = Wolfheat.Convert.Align(hit.point - post.Placement.localPosition);
                            post.transform.rotation = Quaternion.identity;
                            post.transform.parent = StructuresHolder.Instance.transform;
                            post.Place();
                            carrying.RemoveAt(0);
                            UpdateCarrierUI();
                        }
                    }

                }
            }
        }
    }

    private bool SameType(Carryable carryable, Interactable interactable) => carryable.GetType() == interactable.GetType();

    private Carryable SetGhostItem(Carryable carryable)
    {
        if (carryable is Post)
        {
            return ghostpost;
        }
        if (carryable is Lagging)
        {
            return ghostlagging;
        }
        if (carryable is Log)
            return ghostLog;
        return null;
    }

    private bool TryPickUp(Interactable interactable, Carryable carryable)
    {   
        // Check if Picking up the same item that is held
        if (carrying[0].data != null && carryable.data != null && carrying[0].data.name == carryable.data.name)
        {
            Debug.Log("Name are equal, try to pick up");
            if (!carryable.Placed)
            {
                interactable.Interract();
                return true;
            }
            return false;
        }
        return false;

    }

    private void SetTimer(bool useGhost)
    {
        if (!useGhost)
        {
            allowAction = false;
            allowTimer = AllowTime;
        }
    }

    private bool PositionFree<T>(Vector3 pos) where T : Carryable
    {
        // Check if posution is allready Occupied 
        foreach (var item in StructuresHolder.Instance.transform.GetComponentsInChildren<T>())
        {
            // Ignore collide with ghosts
            if (item.IsGhost)
                continue;
            if (Vector3.Distance(item.transform.position, pos) < 0.2f)
            {
                Debug.Log("Placing Lagging on another lagging, abort", item);
                return false;
            }

        }
        return true;

    }

    private void RightClickActionAtPoint()
    {

        if (!allowAction) return;

        // Raycast forward from camera
        
        if (carrying.Count > 0){
            Debug.Log("Carrying a Log drop it");
            ShrinkRestOfStack();            
        }
        else if (Physics.Raycast(tilt.transform.position, tilt.transform.forward, out RaycastHit hit, PlayerStats.PlayerReach, mask))
        {
            Chunk hitChunk = hit.collider.GetComponent<Chunk>();

            if(hitChunk == null)
            {
                Debug.LogWarning("This hit does not contain a GridVisualizer");
                return;
            }


            allowAction = false;
            allowTimer = AllowTime;

            if (Inputs.Instance.Controls.Player.Shift.ReadValue<float>() != 0f)
            {
                // Place a torch here
                PlaceTorchAt(hit.point);
                return;
            }

            CarveAt(hit.point,hitChunk);
        }
        

        Debug.DrawLine(tilt.transform.position, tilt.transform.position + tilt.transform.forward * PlayerStats.PlayerReach, Color.cyan, 2f);


    }

    private bool ShrinkRestOfStack()
    {
        if (carrying.Count == 0)
            return false;
        carrying[0].transform.parent = ItemsHolder.Instance.transform;
        carrying[0].Drop();
        carrying.RemoveAt(0);

        if(carrying.Count>0)
            shrink = true;

        // Next action timed
        allowAction = false;
        allowTimer = AllowTime;
        UpdateCarrierUI();
        return true;
    }

    private void UpdateCarrierUI()
    {
        if (carrying.Count > 0)
            uIController.UpdateCarryPanel(carrying[0].data, carrying.Count);
        else
        {
            Debug.Log("Hiding");
            uIController.HideCarryPanel();
        }
    }

    private void ShrinkStack()
    {
        Vector3 acc = new Vector3(0, 0,2f);
        Vector3 vel = new Vector3(0,0,0);
        if(carrying.Count > 0 && carrying[0].transform.localPosition.z < 0)
        {
            vel += Time.deltaTime * acc;
            foreach(var c in carrying)
            {
                c.transform.localPosition += vel;
            }
        }
        else
        {
            shrink = false;
            Debug.Log("Shrink ended");
        }
    }

    private void CarveAt(Vector3 pos, Chunk chunk, int set = 0)
    {
        // Paint a box here
        CarvingBox box = Instantiate(hitShowPrefab, pos, Quaternion.identity, chunk.transform);

        // Request carve
        chunk.Carve(box,set);
    }

    private void SelectClosest()
    {
       
    }

    internal bool Carry(Carryable carryable)
    {        
        if (carrying.Count > 0 && carrying.Count >= carrying[0].data.max)
            return false;
    
           
        Debug.Log("Start Carrying");
        carrying.Add(carryable);
        UpdateCarrierUI();

        if (carryable is Tool)
        {
            Debug.Log("This is a tool");
            carryable.transform.parent = toolPosition.transform;
        }
        else
        {
            carryable.transform.parent = carryPosition.transform;
        }
        
        // Set Place and Rotation for the carried item
        //carrying.transform.position = carryPosition.transform.position;
        carryable.transform.localPosition = Vector3.zero+ Vector3.back*(carryable.data?.carrySize ?? 0)* (carrying.Count-1);
        //carrying.transform.rotation = carryPosition.transform.rotation;
        carryable.transform.localRotation = Quaternion.identity;

        return true;
    }
}
