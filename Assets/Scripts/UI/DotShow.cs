using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DotShow : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI descriptionText;
    public float padding = 6f;
    public float minHeight = 25f;

    private RectTransform rootRect;
    private RectTransform textRect;


    void Awake()
    {
        ResolveRefs();
    }

    public void SetData(Dot dot)
    {
        ResolveRefs();
        descriptionText.text = dot.description();
        ResizeToText();
    }

    private void ResolveRefs()
    {
        if (rootRect == null)
        {
            rootRect = transform as RectTransform;
        }
        if (image == null)
        {
            image = GetComponent<Image>();
        }
        if (descriptionText == null)
        {
            descriptionText = GetComponentInChildren<TextMeshProUGUI>();
        }
        if (textRect == null && descriptionText != null)
        {
            textRect = descriptionText.rectTransform;
        }
    }

    private void ResizeToText()
    {
        if (descriptionText == null || rootRect == null || textRect == null) return;

        descriptionText.ForceMeshUpdate();
        float width = textRect.rect.width;
        if (width <= 0.1f)
        {
            width = rootRect.rect.width - padding * 2f;
        }

        Vector2 preferred = descriptionText.GetPreferredValues(descriptionText.text, width, 0f);
        float targetHeight = Mathf.Max(minHeight, preferred.y + padding * 2f);
        rootRect.sizeDelta = new Vector2(rootRect.sizeDelta.x, targetHeight);
        textRect.offsetMin = new Vector2(padding, padding);
        textRect.offsetMax = new Vector2(-padding, -padding);
    }
}
