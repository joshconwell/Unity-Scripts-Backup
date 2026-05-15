using UnityEngine;

public class MiniBossDisplayName : MonoBehaviour
{
    [SerializeField] private string displayName = "THE WARDEN";

    public string DisplayName
    {
        get { return displayName; }
    }
}