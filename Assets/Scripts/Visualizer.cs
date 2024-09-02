using UnityEngine;

public class Visualizer : MonoBehaviour
{
public static Visualizer Instance { get; private set; }

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }



    public static void ShowAt(Vector3 pos) => Instance.transform.position = pos;
}
