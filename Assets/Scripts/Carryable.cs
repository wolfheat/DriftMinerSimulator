using UnityEngine;

public class Carryable : MonoBehaviour, IDroppable
{
    [SerializeField] Collider col;
    [SerializeField] Rigidbody rb;
    [SerializeField] Collider[] interractColliders;
    [SerializeField] bool isGhost;
    public bool IsGhost => isGhost;
    public bool Placed { get; private set; } = false;

    public void Drop()
    {
        EnablePhysics();
    }
    private void EnablePhysics()
    {
        rb.isKinematic = false;
        col.enabled = true;
        foreach (Collider interactCollider in interractColliders)
            interactCollider.enabled = true;
    }
    public void DisablePhysics()
    {
        rb.isKinematic = true;
        col.enabled = false;
        foreach (Collider interactCollider in interractColliders)
            interactCollider.enabled = false;
        Placed = false;
    }
    public void Place()
    {
        rb.isKinematic = true;
        col.enabled = true;
        foreach (Collider interactCollider in interractColliders)
            interactCollider.enabled = true;
        Placed = true;
    }   

}
