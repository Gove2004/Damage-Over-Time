

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardList : MonoBehaviour
{
    public List<CardUIItem> cardUIItems = new List<CardUIItem>();
    public GameObject cardPreb;
    public Transform container;

    [Header("Layout Settings")]
    public float cardSpacing = 100f; // Reduced from 150f to fit 7 cards
    public float moveSpeed = 15f;
    public float cardWidth = 180f; // Default width, can be adjusted or auto-detected

    // 选中的卡牌
    public bool AblePlay => selectedCard != null && ((Player)BattleManager.Instance?.player)?.mana >= selectedCard.Cost;
    public BaseCard selectedCard;

    void Start()
    {
        // 尝试禁用容器上的 LayoutGroup，因为我们将手动控制布局
        LayoutGroup layoutGroup = container.GetComponent<LayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.enabled = false;
        }

        // 监听卡牌选择事件
        EventCenter.Register("CardSelected", (param) =>
        {
            // 取消原本选中的卡牌
            if (selectedCard != null)
            {
                Card2UIItem(selectedCard).Deselected();
            }
            selectedCard = param as BaseCard;
        });

        // 监听卡牌取消选择事件
        EventCenter.Register("CardDeselected", (param) =>
        {
            selectedCard = null;
        });

        EventCenter.Register("Player_DrawCard", (param) =>
        {
            DrawCard(param as BaseCard);
        });

        EventCenter.Register("Player_PlayCard", (param) =>
        {
            selectedCard = null;  // 出牌后取消选中状态
            PlayCard(param as BaseCard);
        });
    }

    void Update()
    {
        UpdateCardLayout();
    }

    private void UpdateCardLayout()
    {
        if (cardUIItems.Count == 0) return;

        // 简单的水平布局计算
        // 总宽度 = (数量 - 1) * 间距
        // 起始 X = -总宽度 / 2
        
        float spacing = cardSpacing;
        if (cardUIItems.Count > 0)
        {
            var firstRect = (RectTransform)cardUIItems[0].transform;
            float w = firstRect != null ? firstRect.rect.width : cardWidth;
            if (w > 0f) cardWidth = w;
        }
        var containerRect = container as RectTransform;
        if (containerRect != null && cardUIItems.Count > 1)
        {
            float availableWidth = containerRect.rect.width;
            float maxSpacing = (availableWidth - cardWidth) / (cardUIItems.Count - 1);
            if (maxSpacing > 0f)
            {
                spacing = Mathf.Min(cardSpacing, maxSpacing);
            }
        }

        float minSpacing = cardWidth * 1.05f;
        if (spacing < minSpacing)
        {
            spacing = minSpacing;
        }

        float totalWidth = (cardUIItems.Count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cardUIItems.Count; i++)
        {
            CardUIItem item = cardUIItems[i];
            if (item == null) continue;

            float targetX = startX + i * spacing;
            
            // 设置卡牌的目标位置（Y轴保持0，或者可以根据需要调整）
            item.targetPosition = new Vector2(targetX, 0);
            
            // 更新卡牌的渲染层级（除了正在拖拽的）
            // 注意：这可能会导致频繁的 SetSiblingIndex 调用，如果性能有问题可以优化
            // 但为了保证遮挡关系正确（左边压右边或者右边压左边），通常需要排序
            // 这里我们假设拖拽的卡牌自己会处理 SetAsLastSibling
            if (!item.IsDragging) 
            {
                item.transform.SetSiblingIndex(i);
            }
        }
    }

    // 当卡牌被拖拽时调用
    public void OnCardDrag(CardUIItem draggedItem)
    {
        if (draggedItem == null) return;

        // 根据拖拽卡牌的 X 位置更新列表顺序
        int currentIndex = cardUIItems.IndexOf(draggedItem);
        if (currentIndex < 0) return;

        float currentX = ((RectTransform)draggedItem.transform).anchoredPosition.x;

        // 检查是否需要向左交换
        if (currentIndex > 0)
        {
            CardUIItem leftItem = cardUIItems[currentIndex - 1];
            // 阈值可以稍微调整，比如 cardSpacing / 2
            if (currentX < ((RectTransform)leftItem.transform).anchoredPosition.x)
            {
                SwapCards(currentIndex, currentIndex - 1);
                return; // 一次只交换一个，避免混乱
            }
        }

        // 检查是否需要向右交换
        if (currentIndex < cardUIItems.Count - 1)
        {
            CardUIItem rightItem = cardUIItems[currentIndex + 1];
            if (currentX > ((RectTransform)rightItem.transform).anchoredPosition.x)
            {
                SwapCards(currentIndex, currentIndex + 1);
                return;
            }
        }
    }

    private void SwapCards(int indexA, int indexB)
    {
        CardUIItem temp = cardUIItems[indexA];
        cardUIItems[indexA] = cardUIItems[indexB];
        cardUIItems[indexB] = temp;
    }

    public void DrawCard(BaseCard card)
    {
        // 创建新的CardUIItem实例
        GameObject cardUIObj = Instantiate(cardPreb, container);
        CardUIItem cardUIItem = cardUIObj.GetComponent<CardUIItem>();
        cardUIItem.SetData(card);
        // 初始化引用
        cardUIItem.Init(this);

        cardUIItems.Add(cardUIItem);
    }

    public void PlayCard(BaseCard card)
    {
        var cui = Card2UIItem(card);

        if (cui != null)
        {
            cardUIItems.Remove(cui);
            Destroy(cui.gameObject);
        }
    }



    private CardUIItem Card2UIItem(BaseCard card)
    {
        foreach (var cui in cardUIItems)
        {
            if (cui.cardData == card)
            {
                return cui;
            }
        }
        return null;
    }
}
