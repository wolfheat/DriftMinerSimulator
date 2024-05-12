using UnityEngine;
public class ItemsHolder : MonoBehaviour
{
	public static ItemsHolder Instance { get; private set; }

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
