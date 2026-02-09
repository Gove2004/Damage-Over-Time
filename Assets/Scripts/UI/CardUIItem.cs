
using DG.Tweening;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUIItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool JustUIShow = false;
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
    
    private bool dragging;
    public bool IsDragging => dragging; // Public property for CardList to access
    
    private Vector2 dragOffset;
    private Vector2 originalTooltipAnchored;
    
    // Layout
    public Vector2 targetPosition;
    private CardList cardList;

    public void Init(CardList list)
    {
        this.cardList = list;
    }

    void Awake()
    {
        var tooltipRect = (RectTransform)tooltip.transform;
        originalTooltipAnchored = tooltipRect.anchoredPosition;
        tooltip.SetActive(false);
        outline.enabled = false;

        SetData(cardData);
    }

    void Start()
    {
        if (cardList == null)
        {
            cardList = GetComponentInParent<CardList>();
        }
    }

    void Update()
    {
        if (JustUIShow) return;

        // Smooth movement to target position when not dragging
        if (!dragging)
        {
            var rect = (RectTransform)transform;
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPosition, Time.deltaTime * 15f);
        }

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
        if (card == null)
        {
            Debug.LogWarning("尝试设置空卡牌数据");
            return;
        }

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
        if (JustUIShow) return;

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
        if (JustUIShow) return;
        
        if (sequence.IsActive())
        {
            sequence.Kill(true);
        }

        var tooltipRect = (RectTransform)tooltip.transform;
        sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(Vector3.one, 0.2f));
        sequence.Join(tooltipRect.DOAnchorPosY(originalTooltipAnchored.y, 0.2f));
        sequence.Join(tooltipCanvasGroup.DOFade(0, 0.2f));

        sequence.AppendCallback(() =>
        {
            tooltip.SetActive(false);
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
         if (JustUIShow) return;

        if (sequence.IsActive())
        {
            sequence.Kill(true);
        }

        tooltip.SetActive(true);

        var tooltipRect = (RectTransform)tooltip.transform;
        tooltipRect.anchoredPosition = originalTooltipAnchored;
        sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(Vector3.one * 1.1f, 0.2f));
        sequence.Join(tooltipRect.DOAnchorPosY(originalTooltipAnchored.y + 20, 0.2f));
        sequence.Join(tooltipCanvasGroup.DOFade(1, 0.2f));
    }
    #endregion


    public void OnBeginDrag(PointerEventData eventData)
    {
         if (JustUIShow) return;

        dragging = true;
        var rect = (RectTransform)transform;
        
        // Remove old layout/placeholder logic
        
        outline.enabled = true;
        tooltip.SetActive(false);
        ((RectTransform)tooltip.transform).anchoredPosition = originalTooltipAnchored;
        
        // Ensure dragged card renders on top
        transform.SetAsLastSibling();
        
        var parentRect = transform.parent as RectTransform;
        if (parentRect != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out localPoint);
            dragOffset = rect.anchoredPosition - localPoint;
        }
        else
        {
            dragOffset = Vector2.zero;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
         if (JustUIShow) return;

        var rect = (RectTransform)transform;
        var parentRect = transform.parent as RectTransform;
        if (parentRect != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out localPoint);
            rect.anchoredPosition = localPoint + dragOffset;
            
            // Notify layout system
            if (cardList != null)
            {
                cardList.OnCardDrag(this);
            }
        }
        else
        {
            transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
         if (JustUIShow) return;

        dragging = false;
        var player = BattleManager.Instance?.player as Player;
        bool canPlay = player != null && player.isReady && cardData != null && cardData.Cost <= player.mana;
        
        // Improved validation: Check if card is dragged high enough (e.g., > 150 pixels above original position or specific screen Y threshold)
        // Using a threshold relative to screen height is often safer for different resolutions.
        // Let's assume the hand area is at the bottom. We require the drag to be significantly upwards.
        
        bool draggedOut = false;
        
        // Option 1: Check distance from original position (vertical only)
        // if (transform.position.y - originalParent.position.y > 200) ... 
        
        // Option 2: Check absolute screen Y position. 
        // Assuming hand is at bottom, let's say it needs to be in the top 3/4 of the screen or above a certain line.
        float playThresholdY = Screen.height * 0.5f; // Must be above bottom 30% of screen
        
        if (eventData.position.y > playThresholdY)
        {
            draggedOut = true;
        }

        // Also respect the container check if needed, but Y threshold is usually better for "playing" cards.
        // Alternatively, use the old check BUT with a buffer? 
        // The user complained "once dragged... cannot cancel". This implies existing check is too loose (draggedOut is true too easily).
        // By adding a Y threshold, we ensure they must drag UP, not just wiggle left/right.

        if (canPlay && draggedOut)
        {
            player.UI_PlayCard(cardData);
        }
        else
        {
            // Just let the update loop handle the return animation
            outline.enabled = IsSelected;
        }
    }

}
