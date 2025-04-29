using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class AnimatedButton : MonoBehaviour, IPointerClickHandler
{
    [Header("Animation Settings")]
    public float scaleUp = 1.1f;
    public float animDuration = 0.2f;

    [Header("Delayed Action")]
    public UnityEvent onClick;

    private Button button;
    private Vector3 originalScale;
    private bool isAnimating = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;
    }

    private void OnDisable()
    {
        transform.localScale = originalScale;
        isAnimating = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!button.interactable || isAnimating)
            return;

        StartCoroutine(AnimateAndClick());
    }

    private IEnumerator AnimateAndClick()
    {
        isAnimating = true;

        // Scale up
        yield return ScaleTo(originalScale * scaleUp, animDuration / 2f);

        // Scale back
        yield return ScaleTo(originalScale, animDuration / 2f);

        onClick?.Invoke();
        isAnimating = false;
    }

    private IEnumerator ScaleTo(Vector3 target, float duration)
    {
        Vector3 start = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localScale = target;
    }
}
