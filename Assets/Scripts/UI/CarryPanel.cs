using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarryPanel : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TextMeshProUGUI mainCarryName;
    [SerializeField] TextMeshProUGUI mainCarryAmt;
    [SerializeField] Image mainCarryImage;


    // Update is called once per frame
    public void SetData(ItemData data, int amt)
    {
        panel.SetActive(true);
        Debug.Log("Setting sprite for carry"); 
        mainCarryImage.sprite = data.sprite;
        mainCarryName.text = data.name;
        mainCarryAmt.text = amt > 1 ? amt.ToString():"";
        //mainCarryAmt.text = amt > 1 ? (amt+"/"+data.max):"";
    }
    public void Hide()
    {
        panel.SetActive(false);
    }
}
