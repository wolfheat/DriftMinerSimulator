using UnityEngine;

public class Carryable : MonoBehaviour, IDroppable
{
    [SerializeField] Collider col;
    [SerializeField] Rigidbody rb;
    public bool Placed { get; private set; }

    public void Drop()
    {
        EnablePhysics();
    }
    private void EnablePhysics()
    {
        rb.isKinematic = false;
        col.enabled = true;
    }
    protected void DisablePhysics()
    {
        rb.isKinematic = true;
        col.enabled = false;
    }
    public void Place()
    {
        rb.isKinematic = true;
        col.enabled = true;
    }

}
