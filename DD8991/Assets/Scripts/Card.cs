using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class Card : MonoBehaviour
{
    public Image frontImage;
    public Image backImage;
    public Button button;

    private int id;
    private bool isFlipped = false;
    private GameManager manager;

    public void Setup(CardData data, GameManager gm)
    {
        id = data.id;
        frontImage.sprite = data.frontSprite;
        manager = gm;
    }

    public int GetID() => id;

    public void Flip(bool showFront)
    {
        if (gameObject.activeInHierarchy) // ensure object is alive
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

        AudioManager.Instance.PlaySound(SoundType.Flip); // Play flip sound exactly at swap

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
        if (!isFlipped && manager.CanFlip(this))
        {
            Flip(true);
            manager.RegisterFlip(this);
        }
    }

    public void Lock() => button.interactable = false;
    public void Unlock() => button.interactable = true;

    public void Hide() => gameObject.SetActive(false);
    public void Show() => gameObject.SetActive(true);
}
