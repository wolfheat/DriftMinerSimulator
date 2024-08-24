using System;
using System.Collections;
using System.Collections.Generic;
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
    private bool shrink = false;

    private void Interact(bool useGhost=false)
    {
        if (!allowAction) return;

        // Raycast forward from camera
        if (Physics.Raycast(tilt.transform.position, tilt.transform.forward, out RaycastHit hit, PlayerStats.PlayerReach, interactablesMask))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>() ?? hit.collider.GetComponentInParent<Interactable>();
            if (interactable == null)
            {
                Debug.LogWarning("This hit does not contain a GridVisualizer");
                return;
            }

            // Timer when placing item
            if (!useGhost)
            {
                allowAction = false;
                allowTimer = AllowTime;
            }

            // Change this to be correct
            if (((Carryable)interactable).Placed || useGhost)
                return;

            if (carrying.Count > 0)
            {
                ICutable cutable = interactable as ICutable;
                if (cutable != null)
                {
                    cutable.Cut();
                    return;
                }

                // Check if Picking up the same item that is held
                if ((interactable is Carryable) && carrying[0].data.name == (interactable as Carryable)?.data.name)
                {
                    Debug.Log("Name are equal");
                    Carryable carryable = interactable as Carryable;
                    if (!carryable.Placed)
                    {
                        // Try to Pick up another of same type
                        interactable.Interract();
                    }
                    return;
                }

                // Placing item

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

                // ghostpost is specific, make it general
                Carryable activePost = useGhost ? ghostpost : carrying[0];


                Vector3 newPosition = new();
                Quaternion newRotation = new();


                // Show what should be shown at the decided position
                if (useGhost)
                {
                    // Places aligned to other objects
                    Vector3 cardinalTowardsPlayer = Wolfheat.Convert.AlignCardinal(transform.position - placePoint.position);
                    newPosition = placePoint.position - ghostpost.Placement.transform.localPosition.z * cardinalTowardsPlayer;
                    newRotation = Quaternion.LookRotation(cardinalTowardsPlayer, placePoint.transform.up);
                    ghostpost.transform.parent = StructuresHolder.Instance.transform;

                }
                else
                {
                    // PLaces unrelated
                    Post post = carrying[0] as Post;
                    newPosition = placePoint.position - post.Placement.transform.localPosition.y * placePoint.transform.up;
                    newRotation = placePoint.rotation;
                    post.transform.parent = StructuresHolder.Instance.transform;
                }

                // If attempting to place on occupied position abort
                if (!useGhost && !PositionFree<Post>(newPosition))
                {
                    Debug.Log("Position is not Free");
                    return;
                }

                activePost.transform.position = newPosition;
                activePost.transform.rotation = newRotation;
                activePost.transform.parent = StructuresHolder.Instance.transform;



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

                if (carrying[0] is Lagging && interactable is Post)
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

                    Carryable activeLagging = useGhost ? ghostlagging : carrying[0];


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

                    activeLagging.transform.position = newPosition;
                    activeLagging.transform.rotation = newRotation;
                    activeLagging.transform.parent = StructuresHolder.Instance.transform;

                    if (useGhost)
                        ghostlagging.ActivateVisibleCountDown();
                    else
                    {
                        carrying[0].Place();
                        carrying.RemoveAt(0);
                        UpdateCarrierUI();
                        ghostlagging.transform.position = Vector3.up * 10f;
                    }

                }
                else if (carrying[0] is Lagging && interactable is Lagging)
                {
                    if (!useGhost)
                    {
                        Debug.Log("Try to pick up another Target Post");
                        interactable.Interract();
                    }
                    return;
                }
            }
            else
            {
                // Interacting with object
                if (!useGhost)
                    interactable.Interract();
            }


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
