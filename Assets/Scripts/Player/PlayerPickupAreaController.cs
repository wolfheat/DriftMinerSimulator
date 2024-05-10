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
    [SerializeField] Log carrying;


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
    }

    private void OnDisable()
    {
        Inputs.Instance.Controls.Player.RClick.started -= RightClick;
        Inputs.Instance.Controls.Player.Click.started -= Click;
    }

    private void RightClick(InputAction.CallbackContext context) => DetermineClickPoint();
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
            DetermineClickPoint();
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

            if(interactable == null)
            {
                Debug.LogWarning("This hit does not contain a GridVisualizer");
                return;
            }

            allowAction = false;
            allowTimer = AllowTime;

            interactable.Interract();
        }
            
    }
    
    private void DetermineClickPoint()
    {

        if (!allowAction) return;

        // Raycast forward from camera
        if (Physics.Raycast(tilt.transform.position, tilt.transform.forward, out RaycastHit hit, PlayerStats.PlayerReach, mask))
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
        else
        {
            Debug.Log("Not carving try to drop log if carrying");
            if(carrying!= null)
            {
                Debug.Log("Carrying a Log drop it");
                carrying.transform.parent = ItemsHolder.Instance.transform;
                carrying.Drop();
                carrying = null;
            }

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

    internal bool Carry(Log log)
    {
        if (carrying != null) return false;

        Debug.Log("Start Carrying Log");
        carrying = log;
        carrying.transform.parent = carryPosition.transform;
        carrying.transform.position = carryPosition.transform.position;
        carrying.transform.rotation = carryPosition.transform.rotation;

        return true;
    }
}
