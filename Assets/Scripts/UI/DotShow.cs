using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DotShow : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI descriptionText;


    void Start()
    {
        image = GetComponent<Image>();
        descriptionText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetData(Dot dot)
    {
        descriptionText.text = dot.description();
    }
}
