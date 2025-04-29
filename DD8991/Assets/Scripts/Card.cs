using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public Image frontImage;
    public Image backImage;
    public Button button;

    private int id;
    public bool isFlipped = false;

    public void Setup(CardData data)
    {
        id = data.id;
        frontImage.sprite = data.frontSprite;
    }

    public int GetID() => id;

    public void Flip(bool showFront)
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(FlipAnimation(showFront));
    }

    private IEnumerator FlipAnimation(bool showFront)
    {
        float duration = 0.3f;
        float halfDuration = duration / 2f;
        float elapsed = 0f;

        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = new Vector3(0f, originalScale.y, originalScale.z);

        // Shrink to 0 X scale
        while (elapsed < halfDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;

        // 🔥 Swap and play flip sound
        isFlipped = showFront;
        frontImage.gameObject.SetActive(showFront);
        backImage.gameObject.SetActive(!showFront);

        AudioManager.Instance.PlaySound(SoundType.Flip); // Flip sound at midpoint

        // Expand back to normal
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
    }

    public void OnClick()
    {
        Debug.Log($"Card {id} clicked!");
        Debug.Log($"isFlipped: {isFlipped}");
        Debug.Log($"CanFlip: {GameManager.Instance.CanFlip(this)}");

        if (!isFlipped && GameManager.Instance.CanFlip(this))
        {
            Flip(true);
            GameManager.Instance.RegisterFlip(this);
        }
    }

    public void Lock() => button.interactable = false;
    public void Unlock() => button.interactable = true;
    public void Hide() => gameObject.SetActive(false);
    public void Show() => gameObject.SetActive(true);
}
