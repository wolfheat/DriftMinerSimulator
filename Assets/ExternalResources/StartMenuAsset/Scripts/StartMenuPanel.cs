using UnityEngine;

namespace Wolfheat.StartMenu
{
    public class StartMenuPanel : MonoBehaviour
{
    [SerializeField] GameObject panel;
    public Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void Disable()
    {
        panel.SetActive(false);
    }
    public void AnimationComplete()
    {
        StartMenuController.Instance.AnimationComplete();
    }
}
}
