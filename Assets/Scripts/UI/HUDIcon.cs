using UnityEngine;

public class HUDIcon : MonoBehaviour
{
    [SerializeField] GameObject available;
    [SerializeField] GameObject unavailable;

    public void SetAvailable(bool set)
    {
        available.SetActive(set);
        unavailable.SetActive(!set);
    }
}
