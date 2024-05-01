using UnityEngine;
using UnityEngine.EventSystems;

namespace Wolfheat.StartMenu
{
    public class MenuButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public void AnimationComplete()
        {

        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //SoundMaster.Instance.PlaySound(SoundName.MenuClick);
            Debug.Log("Click in Button: "+Time.realtimeSinceStartup);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (StartMenuController.lastButton == this) return;

            StartMenuController.lastButton = this;
            SoundMaster.Instance.PlaySound(SoundName.MenuOver, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StartMenuController.lastButton = null;
        }
}
}
