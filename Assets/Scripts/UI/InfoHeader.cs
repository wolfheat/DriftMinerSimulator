using TMPro;
using UnityEngine;

public class InfoHeader : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI health;
    [SerializeField] private TextMeshProUGUI oxygen;
    [SerializeField] private TextMeshProUGUI speed;

    public void SetInfo(int healthIn, int oxygenIn, int speedIN)
    {
        health.text = "Health: " + healthIn;
        oxygen.text = "Oxygen: " + oxygenIn;
        speed.text = "Speed: " + speedIN;
    }
}
