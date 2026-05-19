using UnityEngine;

[CreateAssetMenu(fileName = "New Item Data", menuName = "Dungeon Checkout/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemId = "new_item";
    public string itemName = "New Item";
    public string iconText = "ITEM";
    public int baseValue = 1;

    [Header("Difficulty")]
    public int minDay = 1;

    [TextArea(2, 4)] public string scanDescription = "A mysterious item.";
    public Color cardColor = Color.white;

    [Header("Rules")]
    public bool mustBeBaggedAlone = false;
    public bool mustBeRejected = false;
    public string[] explodesWithItemIdsInSameBag;
}