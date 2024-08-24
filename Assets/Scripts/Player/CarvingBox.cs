using System.Collections;
using UnityEngine;

public class CarvingBox : MonoBehaviour
{
    private const float DestructTime = 0.5f;
    [SerializeField] BoxCollider box;
    [SerializeField] GameObject gridPointGroundPrefab;
    [SerializeField] GameObject gridPointAirPrefab;

    public Vector3 StartBounds { get; private set; }
    public Vector3 EndBounds { get; private set; }
    public float SetScale { get; private set; }

    private void OnEnable()
    {
        StartCoroutine(DelayedDestruction());
        AlignMe();
    }

    private void AlignMe()
    {
        // Define Scale
        SetScale = transform.parent.GetComponent<Chunk>()?.GridScaling ?? 1f;

        // Scale Collider
        //box.transform.localScale = Vector3.one * SetScale;

        // Align center to a grid point
        transform.position = new Vector3(Mathf.RoundToInt(transform.position.x/ SetScale) * SetScale, Mathf.RoundToInt(transform.position.y / SetScale) * SetScale, Mathf.RoundToInt(transform.position.z / SetScale) * SetScale);

        // Set Properties
        StartBounds = box.bounds.min;
        EndBounds = box.bounds.max;        
    }

    private IEnumerator DelayedDestruction()
    {
        yield return new WaitForSeconds(DestructTime);
        Destroy(gameObject);
        Debug.Log("Carving box removed");
    }
}
