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

    public string GetFrontSpriteName()
    {
        return frontImage.sprite != null ? frontImage.sprite.name : "";
    }

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

        // Swap and play flip sound
        isFlipped = showFront;
        frontImage.gameObject.SetActive(showFront);
        backImage.gameObject.SetActive(!showFront);

        AudioManager.Instance.PlaySound(SoundType.Flip);

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

    /// <summary>
    /// Immediately sets a card's visual state to either face-up or face-down without flip animation.
    /// </summary>
    /// <param name="showFront">If true, shows the card's front (face-up). If false, shows the back (face-down).</param>
    public void ForceFlip(bool showFront)
    {
        isFlipped = showFront;
        frontImage.gameObject.SetActive(showFront);
        backImage.gameObject.SetActive(!showFront);
    }

    public void OnClick()
    {
        if (!isFlipped && GameManager.Instance.CanFlip(this))
        {
            Flip(true);
            GameManager.Instance.RegisterFlip(this);
        }
    }
    public void Hide() => gameObject.SetActive(false);
}
