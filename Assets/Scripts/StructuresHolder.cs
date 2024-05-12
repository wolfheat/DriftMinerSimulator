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

}
