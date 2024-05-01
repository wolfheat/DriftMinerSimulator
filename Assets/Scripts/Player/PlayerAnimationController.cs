using UnityEngine;

public enum PlayerState {Idle,Hit,Drill,Shoot}

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] Animator animator;

    public PlayerState State{ get; private set; }
    public void SetState(PlayerState newState)
    {
        switch (newState)
        {
            case PlayerState.Idle:
                animator.CrossFade("Idle", 0.1f);
                break;
            case PlayerState.Hit:
                animator.CrossFade("Hit", 0.1f);
                break;
            case PlayerState.Drill:
                animator.CrossFade("Drill", 0.1f);
                break;
            case PlayerState.Shoot:
                animator.CrossFade("Shoot", 0.1f);
                break;
            default:
                break;
        }
        State = newState;
    }
}
