using UnityEngine;
using UnityEngine.UI;

public class UIButtonClickSFX : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] private SFXType buttonClickSFX = SFXType.ButtonClick;

    [Range(0f, 1f)]
    [SerializeField] private float volumeMultiplier = 0.7f;

    [Header("Setup")]
    [SerializeField] private bool includeInactiveButtons = true;

    private Button[] buttons;

    private void Start()
    {
        buttons = GetComponentsInChildren<Button>(includeInactiveButtons);

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                buttons[i].onClick.AddListener(PlayButtonClick);
            }
        }
    }

    private void OnDestroy()
    {
        if (buttons == null)
            return;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                buttons[i].onClick.RemoveListener(PlayButtonClick);
            }
        }
    }

    private void PlayButtonClick()
    {
        if (!AudioManager.HasInstance)
            return;

        AudioManager.Instance.PlaySFX(buttonClickSFX, Vector3.zero, volumeMultiplier);
    }
}