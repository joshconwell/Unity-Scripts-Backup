using UnityEngine;

public class PlayerExperienceSFX : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] private SFXType levelUpSFX = SFXType.LevelUp;

    [Range(0f, 1f)]
    [SerializeField] private float volumeMultiplier = 1f;

    private PlayerExperience playerExperience;

    private void Awake()
    {
        playerExperience = GetComponent<PlayerExperience>();
    }

    private void OnEnable()
    {
        if (playerExperience == null)
            playerExperience = GetComponent<PlayerExperience>();

        if (playerExperience == null)
            return;

        playerExperience.OnLevelUp += HandleLevelUp;
    }

    private void OnDisable()
    {
        if (playerExperience == null)
            return;

        playerExperience.OnLevelUp -= HandleLevelUp;
    }

    private void HandleLevelUp(int newLevel)
    {
        if (!AudioManager.HasInstance)
            return;

        AudioManager.Instance.PlaySFX(levelUpSFX, transform.position, volumeMultiplier);
    }
}