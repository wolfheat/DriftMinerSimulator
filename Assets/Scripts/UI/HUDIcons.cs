using System;
using UnityEngine;

public enum HUDIconType {Generic,PickUp, LeftClick, Interact };
public class HUDIcons : MonoBehaviour
{
    [SerializeField] private HUDIcon[] iconList;
    //Interactable objectToFollow;
    public void Disable()
    {
        HideAll();
    }

    private void HideAll()
    {
        foreach (var t in iconList)
            t.gameObject.SetActive(false);
    }

    private void Update()
    {
        //if(objectToFollow != null)
            //transform.position = Camera.main.WorldToScreenPoint(objectToFollow.transform.position);
    }
}
