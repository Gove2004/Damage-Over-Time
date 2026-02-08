
using DG.Tweening;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUIItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // UI元素引用
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardDescriptionText;
    public GameObject tooltip;
    public CanvasGroup tooltipCanvasGroup;
    public Outline outline;
    public TextMeshProUGUI costText;

    public Image image;
    // Card数据
    public BaseCard cardData = new 测试();
    private string lastDescription = "";
    private int lastCost = int.MinValue;
    private Vector3 originalPosition;
    private Transform originalParent;
    private bool dragging;

    void Awake()
    {
        tooltip.SetActive(false);
        outline.enabled = false;

        SetData(cardData);
    }

    void Start()
    {

    }

    void Update()
    {
        if (cardData == null) return;
        string description = cardData.GetDynamicDescription();
        if (description != lastDescription)
        {
            cardDescriptionText.text = description;
            lastDescription = description;
        }
        if (cardData.Cost != lastCost)
        {
            costText.text = cardData.Cost.ToString();
            lastCost = cardData.Cost;
        }
    }


    public void SetData(BaseCard card)
    {
        cardData = card;

        cardNameText.text = card.Name;
        cardDescriptionText.text = card.GetDynamicDescription();
        lastDescription = cardDescriptionText.text;
        try
        {
            image.sprite = Resources.Load<Sprite>(card.ImagePath);
        }
        catch
        {
            Debug.LogWarning($"无法加载卡牌图片: {card.ImagePath}");
        }
        costText.text = card.Cost.ToString();
        lastCost = card.Cost;
    }

    #region 点击选中卡牌
    public bool IsSelected = false;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsSelected)  // 第一次点击，选中卡牌
        {
            Selected();
            EventCenter.Publish("CardSelected", cardData);
        }
        else  // 再次点击，取消选中
        {
            Deselected();
            EventCenter.Publish("CardDeselected", cardData);
        }
    }

    public void Selected()
    {
        IsSelected = true;
        outline.enabled = true;
    }

    public void Deselected()
    {
        IsSelected = false;
        outline.enabled = false;   
    }
    #endregion


    # region 鼠标悬停动画
    Sequence sequence;
    public void OnPointerExit(PointerEventData eventData)
    {
        if (sequence.IsActive())
        {
            sequence.Kill();
        }

        sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(Vector3.one, 0.2f));
        sequence.Join(tooltip.transform.DOMoveY(tooltip.transform.position.y - 20, 0.2f));
        sequence.Join(tooltipCanvasGroup.DOFade(0, 0.2f));

        sequence.AppendCallback(() =>
        {
            tooltip.SetActive(false);
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (sequence.IsActive())
        {
            sequence.Kill();
        }

        tooltip.SetActive(true);

        sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(Vector3.one * 1.1f, 0.2f));
        sequence.Join(tooltip.transform.DOMoveY(tooltip.transform.position.y + 20, 0.2f));
        sequence.Join(tooltipCanvasGroup.DOFade(1, 0.2f));
    }
    #endregion


    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
        originalPosition = transform.position;
        originalParent = transform.parent;
        outline.enabled = true;
        tooltip.SetActive(false);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
        var player = BattleManager.Instance?.player as Player;
        bool canPlay = player != null && player.isReady && cardData != null && cardData.Cost <= player.mana;
        var containerRect = originalParent as RectTransform;
        bool outside = true;
        if (containerRect != null)
        {
            outside = !RectTransformUtility.RectangleContainsScreenPoint(containerRect, eventData.position, eventData.pressEventCamera);
        }
        if (canPlay && outside)
        {
            player.UI_PlayCard(cardData);
        }
        else
        {
            transform.DOMove(originalPosition, 0.2f);
            outline.enabled = IsSelected;
        }
    }

}
