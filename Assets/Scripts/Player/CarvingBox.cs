using System.Collections;
using UnityEngine;

public class CarvingBox : MonoBehaviour
{
    private const float DestructTime = 0.5f;
    [SerializeField] BoxCollider box;

    public Vector3 StartBounds { get; private set; }
    public Vector3 EndBounds { get; private set; }

    private void OnEnable()
    {
        StartCoroutine(DelayedDestruction());
        AlignMe();
    }

    [SerializeField] GameObject gridPointGroundPrefab;
    [SerializeField] GameObject gridPointAirPrefab;
    private void AlignMe()
    {
        Debug.Log("Align Box");

        Debug.Log("Box is aligned from position: " + box.transform.position);

        Vector3 startBounds = box.bounds.min;
        Vector3 endBounds = box.bounds.max;
        /*
        // Visualize the bounding box by placing small objects at corners
        GameObject gridPoint = Instantiate(gridPointGroundPrefab, transform);
        gridPoint.transform.position = startBounds;
        GameObject gridPoint2 = Instantiate(gridPointGroundPrefab, transform);
        gridPoint2.transform.position = endBounds;
        */

        // Align center to middle of a tile
        transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));


        // Set Properties
        StartBounds = box.bounds.min;
        EndBounds = box.bounds.max;        

        /*
        Debug.Log("Box is aligned to position: " + box.transform.position);

        // Visualize the bounding box by placing small objects at corners
        GameObject gridPoint3 = Instantiate(gridPointAirPrefab, transform);
        gridPoint3.transform.position = startBounds;
        GameObject gridPoint4 = Instantiate(gridPointAirPrefab, transform);
        gridPoint4.transform.position = endBounds;
        */

    }

    private IEnumerator DelayedDestruction()
    {
        yield return new WaitForSeconds(DestructTime);
        Destroy(gameObject);
    }
}
