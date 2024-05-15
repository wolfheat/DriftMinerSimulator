using UnityEngine;

public class MakeInitialPlaced : MonoBehaviour
{
    void Start()
    {
        foreach(var c in transform.GetComponentsInChildren<Carryable>())
        {
            c.Place();
        }        
    }

}
