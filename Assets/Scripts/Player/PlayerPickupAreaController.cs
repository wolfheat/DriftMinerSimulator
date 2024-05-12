using System;
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
    [SerializeField] Carryable carrying;


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
        if (Inputs.Instance.Controls.Player.RClick.ReadValue<float>() != 0f)
        {
            RightClickActionAtPoint();
        }
    }

    private void PlaceTorchAt(Vector3 pos) => Instantiate(torchPrefab, pos, Quaternion.identity, itemsParent.transform);


    private void Interact()
    {

        if (!allowAction) return;

        // Raycast forward from camera
        if (Physics.Raycast(tilt.transform.position, tilt.transform.forward, out RaycastHit hit, PlayerStats.PlayerReach, interactablesMask))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable == null)
            {
                Debug.LogWarning("This hit does not contain a GridVisualizer");
                return;
            }

            allowAction = false;
            allowTimer = AllowTime;


            if (carrying != null)
            {
                if (carrying is Chainsaw && interactable is Log)
                {
                    Debug.Log("Cut the Log into pieces");
                    Log log = interactable as Log;
                    log.Cut();
                }
                else if (carrying is Chainsaw && interactable is ShortLog)
                {
                    Debug.Log("Cut the ShortLog into pieces");
                    ShortLog log = interactable as ShortLog;
                    log.Cut();
                }
                else if (carrying is Chainsaw && interactable is Post)
                {
                    Debug.Log("Cut the Post into pieces");
                    Post log = interactable as Post;
                    log.Cut();
                }
                else if (carrying is Post && interactable is Post)
                {
                    if (!((Post)interactable).Placed)
                    {
                        Debug.Log("Target Post is not placed");
                        return;
                    }

                    Debug.Log("Connect the posts");
                    Debug.Log("Place the Post");
                    Post post = carrying as Post;

                    Post stationary = (Post)interactable;
                    Transform placePoint = stationary.GetConnectpoint(hit.point);
                    // if this gameobject has children toggle through them when scrolling
                    ConnectPoints[] spots = placePoint.GetComponentsInChildren<ConnectPoints>();

                    // Place depending on scroll
                    placePoint = spots[scrollValue % spots.Length].transform;   

                    // Place acording to best direction
                    Vector3 playerDirection = transform.position-placePoint.position;

                    float dot = -1;

                    foreach(var spot in spots)
                    {
                        float newDot = Vector3.Dot(playerDirection, spot.transform.up);
                        if (newDot > dot)
                        {
                            dot = newDot;
                            placePoint = spot.transform;
                        }
                    }



                    post.transform.position = placePoint.position + 1.5f * placePoint.transform.up;
                    post.transform.rotation = placePoint.rotation;
                    post.transform.parent = StructuresHolder.Instance.transform;
                    post.Place();
                    carrying = null;
                }

            }
            else
                interactable.Interract();
        }// Raycast forward from camera
        else if (Physics.Raycast(tilt.transform.position, tilt.transform.forward, out hit, PlayerStats.PlayerReach, mask))
        {
            Chunk hitChunk = hit.collider.GetComponent<Chunk>();

            if (hitChunk == null)
            {
                Debug.LogWarning("This hit does not contain a GridVisualizer");
                return;
            }
                        
            if (carrying != null)
            {
                if (carrying is Post)
                {
                    Debug.Log("Place the Post");
                    Post post = carrying as Post;
                    // Convert position to closest grid point ALIGN
                    post.transform.position = Wolfheat.Convert.Align(hit.point-post.placement.localPosition);
                    post.transform.rotation = Quaternion.identity;
                    post.transform.parent = StructuresHolder.Instance.transform;
                    post.Place();
                    carrying = null;
                }

            }
        }
    }
    private void RightClickActionAtPoint()
    {

        if (!allowAction) return;

        // Raycast forward from camera
        
        if (carrying != null){
                Debug.Log("Carrying a Log drop it");
                carrying.transform.parent = ItemsHolder.Instance.transform;
                carrying.Drop();
                carrying = null;

            allowAction = false;
            allowTimer = AllowTime;
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

    private void CarveAt(Vector3 pos, Chunk chunk)
    {
        // Paint a box here
        CarvingBox box = Instantiate(hitShowPrefab, pos, Quaternion.identity, chunk.transform);

        // Request carve
        chunk.Carve(box);
    }

    private void SelectClosest()
    {
       
    }

    internal bool Carry(Carryable carryable)
    {
        if (carrying != null) return false;

        Debug.Log("Start Carrying");
        carrying = carryable;

        if(carryable is Tool)
        {
            Debug.Log("This is a tool");
            carrying.transform.parent = toolPosition.transform;
        }
        else
        {
            carrying.transform.parent = carryPosition.transform;
        }

        //carrying.transform.position = carryPosition.transform.position;
        carrying.transform.localPosition = Vector3.zero;
        //carrying.transform.rotation = carryPosition.transform.rotation;
        carrying.transform.localRotation = Quaternion.identity;

        return true;
    }
}
