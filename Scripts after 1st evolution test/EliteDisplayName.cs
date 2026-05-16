using UnityEngine;

public class EliteDisplayName : MonoBehaviour
{
    [SerializeField] private string displayName = "ELITE BRUTE";

    public string DisplayName
    {
        get { return displayName; }
    }
}