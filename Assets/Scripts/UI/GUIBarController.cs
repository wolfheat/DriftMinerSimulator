using UnityEngine;

public class GUIBarController : MonoBehaviour
{
    [SerializeField] BarController oxygenBar;
    [SerializeField] BarController healthBar;
    [SerializeField] PlayerStats playerHealth;

    public static float Barwidth;

    private void OnEnable()
    {
        Barwidth = oxygenBar.GetComponent<RectTransform>().rect.size.x;
        playerHealth.OxygenUpdated += SetOxygen;
        playerHealth.HealthUpdated += SetHealth;
    }

    private void SetHealth(float health, int maxHealth)
    {
        healthBar.SetBar(health / maxHealth, ((int)health).ToString());
    }
    
    private void SetOxygen(float oxygen, int maxOxygen)
    {
        oxygenBar.SetBar(oxygen / maxOxygen, ((int)oxygen).ToString());
    }

}
