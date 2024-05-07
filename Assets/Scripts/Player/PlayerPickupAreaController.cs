using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerPickupAreaController : MonoBehaviour
{
    [SerializeField] UIController uIController;
    [SerializeField] CarvingBox hitShowPrefab;
    [SerializeField] GameObject hitShowParent;
    [SerializeField] GameObject tilt;
    [SerializeField] LayerMask mask;

    private void OnTriggerStay(Collider other)
    {
       
    }

    private void OnEnable() => Inputs.Instance.Controls.Player.RClick.started += RightClick;
    private void OnDisable() => Inputs.Instance.Controls.Player.RClick.started -= RightClick;
    private void RightClick(InputAction.CallbackContext context) => DetermineClickPoint();

    bool allowCarve = true;
    float allowTimer = 0f;
    const float AllowTime = 0.1f;

    private void Update()
    {
        if (allowTimer > 0) { 
            allowTimer -= Time.deltaTime;
            if(allowTimer <= 0)
                allowCarve = true;
        }
        if (Inputs.Instance.Controls.Player.RClick.ReadValue<float>() != 0f) 
            DetermineClickPoint();
    }

    private void DetermineClickPoint()
    {
        if (!allowCarve) return;

        // Raycast forward from camera
        if (Physics.Raycast(tilt.transform.position, tilt.transform.forward, out RaycastHit hit, PlayerStats.PlayerReach, mask))
        {
            Chunk hitChunk = hit.collider.GetComponent<Chunk>();

            if(hitChunk == null)
            {
                Debug.LogWarning("This hit does not contain a GridVisualizer");
                return;
            }
            allowCarve = false;
            allowTimer = AllowTime;

            // Paint a box here
            CarvingBox box = Instantiate(hitShowPrefab, hit.point, Quaternion.identity, hitChunk.transform);

            // Request carve
            hitChunk.Carve(box);

        }

        Debug.DrawLine(tilt.transform.position, tilt.transform.position + tilt.transform.forward * PlayerStats.PlayerReach, Color.cyan, 2f);


    }


    private void SelectClosest()
    {
       
    }

}
