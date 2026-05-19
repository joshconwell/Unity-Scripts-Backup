using System.Collections;
using UnityEngine;

public class DayEndUiSlider : MonoBehaviour
{
    [Header("Panel State Watch")]
    [SerializeField] private GameObject endDayPanel;

    [Header("Panels To Slide Up")]
    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform customerArea;
    [SerializeField] private RectTransform counterArea;
    [SerializeField] private RectTransform bagArea;
    [SerializeField] private RectTransform registerPanel;
    [SerializeField] private RectTransform bottomBar;

    [Header("Animation")]
    [SerializeField] private float gameplaySlideDuration = 0.45f;
    [SerializeField] private float endDayPanelSlideDuration = 0.45f;
    [SerializeField] private float offscreenDistance = 2200f;

    private RectTransform endDayPanelRect;

    private Vector2 topBarStart;
    private Vector2 customerAreaStart;
    private Vector2 counterAreaStart;
    private Vector2 bagAreaStart;
    private Vector2 registerPanelStart;
    private Vector2 bottomBarStart;
    private Vector2 endDayPanelStart;

    private bool lastEndDayPanelState;
    private Coroutine activeRoutine;

    private void Awake()
    {
        if (endDayPanel != null)
        {
            endDayPanelRect = endDayPanel.GetComponent<RectTransform>();
            lastEndDayPanelState = endDayPanel.activeSelf;
        }

        CacheStartPositions();
    }

    private void Update()
    {
        if (endDayPanel == null)
        {
            return;
        }

        bool currentEndDayPanelState = endDayPanel.activeSelf;

        if (currentEndDayPanelState == lastEndDayPanelState)
        {
            return;
        }

        lastEndDayPanelState = currentEndDayPanelState;

        if (currentEndDayPanelState)
        {
            StartDayEndSequence();
        }
        else
        {
            StartReturnSequence();
        }
    }

    private void StartDayEndSequence()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        activeRoutine = StartCoroutine(DayEndSequenceRoutine());
    }

    private void StartReturnSequence()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        activeRoutine = StartCoroutine(ReturnSequenceRoutine());
    }

    private IEnumerator DayEndSequenceRoutine()
    {
        Vector2 upOffset = new Vector2(0f, offscreenDistance);
        Vector2 downOffset = new Vector2(0f, -offscreenDistance);

        if (endDayPanelRect != null)
        {
            endDayPanelRect.anchoredPosition = endDayPanelStart + downOffset;
        }

        yield return SlideGameplayPanelsTo(
            topBarStart + upOffset,
            customerAreaStart + upOffset,
            counterAreaStart + upOffset,
            bagAreaStart + upOffset,
            registerPanelStart + upOffset,
            bottomBarStart + upOffset,
            gameplaySlideDuration
        );

        yield return SlideEndDayPanelTo(endDayPanelStart, endDayPanelSlideDuration);

        activeRoutine = null;
    }

    private IEnumerator ReturnSequenceRoutine()
    {
        yield return SlideGameplayPanelsTo(
            topBarStart,
            customerAreaStart,
            counterAreaStart,
            bagAreaStart,
            registerPanelStart,
            bottomBarStart,
            gameplaySlideDuration
        );

        activeRoutine = null;
    }

    private IEnumerator SlideGameplayPanelsTo(
        Vector2 topBarTarget,
        Vector2 customerAreaTarget,
        Vector2 counterAreaTarget,
        Vector2 bagAreaTarget,
        Vector2 registerPanelTarget,
        Vector2 bottomBarTarget,
        float duration)
    {
        Vector2 topBarFrom = GetAnchoredPosition(topBar);
        Vector2 customerAreaFrom = GetAnchoredPosition(customerArea);
        Vector2 counterAreaFrom = GetAnchoredPosition(counterArea);
        Vector2 bagAreaFrom = GetAnchoredPosition(bagArea);
        Vector2 registerPanelFrom = GetAnchoredPosition(registerPanel);
        Vector2 bottomBarFrom = GetAnchoredPosition(bottomBar);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float rawT = Mathf.Clamp01(elapsed / duration);
            float easedT = EaseInOut(rawT);

            SetAnchoredPosition(topBar, Vector2.LerpUnclamped(topBarFrom, topBarTarget, easedT));
            SetAnchoredPosition(customerArea, Vector2.LerpUnclamped(customerAreaFrom, customerAreaTarget, easedT));
            SetAnchoredPosition(counterArea, Vector2.LerpUnclamped(counterAreaFrom, counterAreaTarget, easedT));
            SetAnchoredPosition(bagArea, Vector2.LerpUnclamped(bagAreaFrom, bagAreaTarget, easedT));
            SetAnchoredPosition(registerPanel, Vector2.LerpUnclamped(registerPanelFrom, registerPanelTarget, easedT));
            SetAnchoredPosition(bottomBar, Vector2.LerpUnclamped(bottomBarFrom, bottomBarTarget, easedT));

            yield return null;
        }

        SetAnchoredPosition(topBar, topBarTarget);
        SetAnchoredPosition(customerArea, customerAreaTarget);
        SetAnchoredPosition(counterArea, counterAreaTarget);
        SetAnchoredPosition(bagArea, bagAreaTarget);
        SetAnchoredPosition(registerPanel, registerPanelTarget);
        SetAnchoredPosition(bottomBar, bottomBarTarget);
    }

    private IEnumerator SlideEndDayPanelTo(Vector2 targetPosition, float duration)
    {
        if (endDayPanelRect == null)
        {
            yield break;
        }

        Vector2 startPosition = endDayPanelRect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float rawT = Mathf.Clamp01(elapsed / duration);
            float easedT = EaseInOut(rawT);

            endDayPanelRect.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, easedT);

            yield return null;
        }

        endDayPanelRect.anchoredPosition = targetPosition;
    }

    private void CacheStartPositions()
    {
        topBarStart = GetAnchoredPosition(topBar);
        customerAreaStart = GetAnchoredPosition(customerArea);
        counterAreaStart = GetAnchoredPosition(counterArea);
        bagAreaStart = GetAnchoredPosition(bagArea);
        registerPanelStart = GetAnchoredPosition(registerPanel);
        bottomBarStart = GetAnchoredPosition(bottomBar);
        endDayPanelStart = GetAnchoredPosition(endDayPanelRect);
    }

    private Vector2 GetAnchoredPosition(RectTransform rectTransform)
    {
        if (rectTransform == null)
        {
            return Vector2.zero;
        }

        return rectTransform.anchoredPosition;
    }

    private void SetAnchoredPosition(RectTransform rectTransform, Vector2 position)
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchoredPosition = position;
    }

    private float EaseInOut(float t)
    {
        return t * t * (3f - 2f * t);
    }
}