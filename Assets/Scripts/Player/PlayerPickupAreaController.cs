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

    private void OnTriggerStay(Collider other)
    {
       
    }

    private void OnEnable() => Inputs.Instance.Controls.Player.RClick.started += RightClick;
    private void OnDisable() => Inputs.Instance.Controls.Player.RClick.started -= RightClick;
    private void RightClick(InputAction.CallbackContext context) => DetermineClickPoint();

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

}
