using System.Collections;
using UnityEngine;

public class CarvingBox : MonoBehaviour
{
    private const float DestructTime = 0.5f;
    [SerializeField] BoxCollider box;

    public Vector3 StartBounds { get; private set; }
    public Vector3 EndBounds { get; private set; }
    public float SetScale { get; private set; }

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
        Debug.Log("Parent is "+transform.parent.name);        

        float scaleBy = transform.parent?.GetComponent<Chunk>()?.GridScaling ?? 1f;

        SetScale = scaleBy;

        //Debug.Log("Parent scale is "+scaleBy);

        box.transform.localScale = Vector3.one * scaleBy;

        Vector3 startBounds = box.bounds.min - transform.position;
        Vector3 endBounds = box.bounds.max - transform.position;
        /*
        // Visualize the bounding box by placing small objects at corners
        GameObject gridPoint = Instantiate(gridPointGroundPrefab, transform);
        gridPoint.transform.position = startBounds;
        GameObject gridPoint2 = Instantiate(gridPointGroundPrefab, transform);
        gridPoint2.transform.position = endBounds;
        */

        // Align center to middle of a tile
        transform.position = new Vector3(Mathf.RoundToInt(transform.position.x/scaleBy)*scaleBy, Mathf.RoundToInt(transform.position.y / scaleBy) * scaleBy, Mathf.RoundToInt(transform.position.z / scaleBy) * scaleBy);
        //Debug.Log("Box is aligned to position: " + box.transform.position);



        // Set Properties
        StartBounds = box.bounds.min;
        EndBounds = box.bounds.max;
        
        //Debug.LogError("PLaced Box");

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
