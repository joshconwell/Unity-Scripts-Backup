using UnityEngine;

public class PlaySFXOnEnable : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] private SFXType sfxType = SFXType.PlayerShoot;

    [Range(0f, 1f)]
    [SerializeField] private float volumeMultiplier = 1f;

    [Header("Safety")]
    [SerializeField] private bool skipFirstEnable = false;

    private bool hasEnabledBefore;

    private void OnEnable()
    {
        if (skipFirstEnable && !hasEnabledBefore)
        {
            hasEnabledBefore = true;
            return;
        }

        hasEnabledBefore = true;

        if (!AudioManager.HasInstance)
            return;

        AudioManager.Instance.PlaySFX(sfxType, transform.position, volumeMultiplier);
    }
}