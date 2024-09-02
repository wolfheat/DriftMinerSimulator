using UnityEngine;

public class StructuresHolder : MonoBehaviour
{
	public static StructuresHolder Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

    public static bool PositionFree<T>(Vector3 pos) where T : Carryable
    {
        // Check if posution is allready Occupied 
        foreach (var item in Instance.transform.GetComponentsInChildren<T>())
        {
            // Ignore collide with ghosts
            if (item.IsGhost)
                continue;
            if (Vector3.Distance(item.transform.position, pos) < 0.1f)
            {
                Debug.Log("Placing Lagging on another lagging, abort", item);
                return false;
            }

        }
        return true;

    }


}
