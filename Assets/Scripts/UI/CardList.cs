

using System.Collections.Generic;
using UnityEngine;


// 缺少动效


public class CardList : MonoBehaviour
{
    public List<CardUIItem> cardUIItems = new List<CardUIItem>();
    public GameObject cardPreb;
    public Transform container;

    // 选中的卡牌
    public BaseCard selectedCard;

    void Start()
    {
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

        EventCenter.Register("UI_DrawCard", (param) =>
        {
            DrawCard(CardFactory.GetRandomCard());
        });

        EventCenter.Register("UI_PlayCard", (param) =>
        {
            PlayCard(param as BaseCard);
        });
    }

    public void DrawCard(BaseCard card)
    {
        // 创建新的CardUIItem实例
        GameObject cardUIObj = Instantiate(cardPreb, container);
        CardUIItem cardUIItem = cardUIObj.GetComponent<CardUIItem>();
        cardUIItem.SetData(card);

        cardUIItems.Add(cardUIItem);
    }

    public void PlayCard(BaseCard card)
    {
        var cui = Card2UIItem(card);

        cardUIItems.Remove(cui);
        Destroy(cui.gameObject);
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