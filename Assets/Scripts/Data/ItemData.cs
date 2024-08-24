using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Items")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite sprite;
    public int max;
    public float carrySize;
}
