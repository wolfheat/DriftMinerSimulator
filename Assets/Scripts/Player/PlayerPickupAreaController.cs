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

    private void DetermineClickPoint()
    {
        Debug.Log("Raycasting forward");
        // Raycast forward from camera
        if(Physics.Raycast(tilt.transform.position, tilt.transform.forward, out RaycastHit hit, PlayerStats.PlayerReach, mask))
        {
            Debug.Log("Hit something: "+hit.collider.name);

            GridVisualizer hitChunk = hit.collider.GetComponent<GridVisualizer>();

            if(hitChunk == null)
            {
                Debug.LogWarning("This hit does not contain a GridVisualizer");
                return;
            }

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
