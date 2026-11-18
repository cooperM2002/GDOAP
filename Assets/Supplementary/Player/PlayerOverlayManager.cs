using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerOverlayManager : SneedSpace {

    [Space]
    [Header("Sprites")]
    [SerializeField] private RectTransform overlayRoot;
    [SerializeField] private Sprite[] healthOverlaySprites;
    [SerializeField] private Color healthOverlayColor = Color.white;

    [Space]
    [Header("Triggered Overlays")]
    [SerializeField] private Color triggeredOverlayColor = Color.white;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.5f;

    private readonly List<Image> healthOverlayImages = new();


    private void Awake() {
        if (overlayRoot == null) 
            overlayRoot = transform as RectTransform;

        CreateHealthOverlayImages();
        SetHealth(1f, 1f);
    }


    public void SetHealth(float currentHealth, float maxHealth) {
        if (maxHealth <= 0f) return;

        /*  Health  Calculation     visibleOverlayCount
         *  5/5     (1 - 1.0) x 5   0
         *  4/5     (1 - 0.8) x 5   1
         *  3/5     (1 - 0.6) x 5   2
         *  2/5     (1 - 0.4) x 5   3
         *  1/5     (1 - 0.2) x 5   4
         */

        float healthNormalized = Mathf.Clamp01(currentHealth / maxHealth);
        int numImages = healthOverlayImages.Count + 1; // should be 5
        int healthStage = Mathf.RoundToInt((1f - healthNormalized) * numImages);
        healthStage = Mathf.Clamp(healthStage, 0, healthOverlayImages.Count);

        for(int i = 0; i < healthOverlayImages.Count; i++) {
            healthOverlayImages[i].gameObject.SetActive(i < healthStage);
        }
    }


    private void CreateHealthOverlayImages() {
        healthOverlayImages.Clear();

        if(overlayRoot == null) {
            PrintError("no overlayroot");
            return;
        }

        for(int i = 0; i < healthOverlaySprites.Length; i++) {
            Sprite sprite = healthOverlaySprites[i];
            if(sprite == null) continue;

            Image overlayImage = CreateOverlayImage(
                $"Health blood overlay {i + 1}",
                sprite,
                healthOverlayColor
            );

            overlayImage.gameObject.SetActive(false);
            healthOverlayImages.Add(overlayImage);
        }
    }


    public GameObject ShowOverlay(Sprite sprite, float duration = -1f) {
        if (sprite == null || overlayRoot == null) 
            return null;

        Image overlayImage = CreateOverlayImage(
            "Triggered Blood Overlay",
            sprite,
            triggeredOverlayColor
        );

        GameObject overlayObject = overlayImage.gameObject;

        // negative duration = permanent
        if (duration >= 0f) {
            StartCoroutine(TemporaryOverlayRoutine(overlayImage, duration));
        }

        return overlayObject;
    }


    public void RemoveOverlay(GameObject overlay) {
        if (overlay != null) Destroy(overlay);
    }


    private IEnumerator TemporaryOverlayRoutine(Image overlayImage, float duration) {
        yield return new WaitForSeconds(duration);

        if (overlayImage == null) 
            yield break;

        float startingAlpha = overlayImage.color.a;

        if (fadeOutDuration > 0f) {
            float elapsed = 0f;

            while (elapsed < fadeOutDuration) {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startingAlpha, 0f, elapsed / fadeOutDuration);
                Color color = overlayImage.color;
                color.a = alpha;
                overlayImage.color = color;

                yield return null;
            }

            if(overlayImage != null) 
                Destroy(overlayImage.gameObject);
        }
    }


    private Image CreateOverlayImage(string objectName, Sprite sprite, Color color) {
        GameObject overlayObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        RectTransform rectTransform = overlayObject.GetComponent<RectTransform>();

        rectTransform.SetParent(overlayRoot,false);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image overlayImage = overlayObject.GetComponent<Image>();

        overlayImage.sprite = sprite;
        overlayImage.color = color;
        overlayImage.raycastTarget = false;
        overlayImage.preserveAspect = false;

        return overlayImage;
    }


}
