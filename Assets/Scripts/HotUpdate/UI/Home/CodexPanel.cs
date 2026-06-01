using System.Collections.Generic;
using System.Linq;
using GoveKits.Runtime.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodexPanel : MonoBehaviour
{
    public GameObject choiceItemPrefab;
    public Transform contentParent;
    public ScrollRect scrollRect;

    private readonly List<GameObject> spawnedItems = new();

    private const float CardWidth = 90f;
    private const float CardHeight = 120f;
    private const float RowHeight = 130f;
    private const float Spacing = 8f;
    private const float PadX = 10f;

    private static readonly string[] SeriesOrder =
    {
        "初始", "七罪", "血族", "坚固", "科技", "种子", "暗影", "时序"
    };

    private static readonly Color[] SeriesColors =
    {
        new(0.4f, 0.7f, 0.4f), new(0.8f, 0.3f, 0.3f), new(0.7f, 0.2f, 0.4f), new(0.5f, 0.5f, 0.5f),
        new(0.3f, 0.5f, 0.8f), new(0.6f, 0.8f, 0.3f), new(0.3f, 0.3f, 0.5f), new(0.6f, 0.4f, 0.8f)
    };

    public void Show()
    {
        gameObject.SetActive(true);
        ClearSpawned();
        PopulateCards();
    }

    private void PopulateCards()
    {
        List<CardConfigData> allCards = ConfigCore.LoadAll<CardConfigData>();
        if (allCards == null || allCards.Count == 0)
        {
            return;
        }

        Dictionary<string, List<CardConfigData>> grouped = new();
        foreach (CardConfigData card in allCards)
        {
            if (card.id <= 0 || string.IsNullOrEmpty(card.名称))
            {
                continue;
            }

            string series = string.IsNullOrEmpty(card.系列) ? "其他" : card.系列;
            if (!grouped.ContainsKey(series))
            {
                grouped[series] = new List<CardConfigData>();
            }

            grouped[series].Add(card);
        }

        for (int i = 0; i < SeriesOrder.Length; i++)
        {
            string series = SeriesOrder[i];
            if (!grouped.ContainsKey(series))
            {
                continue;
            }

            Color color = i < SeriesColors.Length ? SeriesColors[i] : new Color(0.4f, 0.4f, 0.4f);
            CreateSeriesSection(series, grouped[series], color);
        }

        foreach (var pair in grouped)
        {
            if (!SeriesOrder.Contains(pair.Key))
            {
                CreateSeriesSection(pair.Key, pair.Value, new Color(0.4f, 0.4f, 0.4f));
            }
        }
    }

    private void CreateSeriesSection(string seriesName, List<CardConfigData> cards, Color color)
    {
        GameObject headerGo = new GameObject($"Header_{seriesName}");
        headerGo.transform.SetParent(contentParent, false);
        LayoutElement headerLe = headerGo.AddComponent<LayoutElement>();
        headerLe.preferredHeight = 30;

        Image headerBg = headerGo.AddComponent<Image>();
        headerBg.color = color;
        spawnedItems.Add(headerGo);

        int rows = Mathf.CeilToInt(cards.Count / 8f);
        float totalRowHeight = rows * RowHeight;

        GameObject rowGo = new GameObject($"Row_{seriesName}");
        rowGo.transform.SetParent(contentParent, false);
        LayoutElement rowLe = rowGo.AddComponent<LayoutElement>();
        rowLe.preferredHeight = totalRowHeight;
        spawnedItems.Add(rowGo);

        for (int i = 0; i < cards.Count; i++)
        {
            int col = i % 8;
            int row = i / 8;
            float x = PadX + col * (CardWidth + Spacing);
            float y = -(row * (CardHeight + Spacing));

            SpawnCodexCard(cards[i], rowGo.transform, x, y);
        }
    }

    private void SpawnCodexCard(CardConfigData config, Transform parent, float x, float y)
    {
        if (choiceItemPrefab == null)
        {
            return;
        }

        GameObject go = Instantiate(choiceItemPrefab, parent);
        spawnedItems.Add(go);

        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(CardWidth, CardHeight);
            rt.localScale = Vector3.one;
        }

        BaseCard card = CardFactoryCore.CreateCard(config.id);

        SetCardImage(go, config.名称);

        TMP_Text nameText = go.transform.Find("Text (TMP)")?.GetComponent<TMP_Text>();
        TMP_Text descText = go.transform.Find("Text (TMP) (1)")?.GetComponent<TMP_Text>();

        if (card != null)
        {
            if (nameText != null) nameText.text = $"{card.Name}\n费:{card.Cost}";
            if (descText != null) descText.text = card.Description();
        }
        else
        {
            if (nameText != null) nameText.text = $"{config.名称}\n费:{config.费用}";
            if (descText != null) descText.text = config.描述;
        }

        Button btn = go.GetComponent<Button>();
        if (btn != null)
        {
            btn.interactable = false;
        }
    }

    private static void SetCardImage(GameObject go, string cardName)
    {
        if (string.IsNullOrEmpty(cardName))
        {
            return;
        }

        Transform imageTransform = go.transform.Find("Image");
        if (imageTransform == null)
        {
            return;
        }

        Image img = imageTransform.GetComponent<Image>();
        if (img == null)
        {
            return;
        }

        Sprite sprite = LoadCardSprite(cardName);
        if (sprite != null)
        {
            img.sprite = sprite;
            img.color = Color.white;
        }
    }

    private static Sprite LoadCardSprite(string cardName)
    {
        var handle = ResCore.LoadAssetSync<Sprite>($"Card_{cardName}");
        return handle?.GetAssetObject<Sprite>();
    }

    private void ClearSpawned()
    {
        foreach (GameObject go in spawnedItems)
        {
            if (go != null)
            {
                Destroy(go);
            }
        }

        spawnedItems.Clear();
    }

    private void OnDisable()
    {
        ClearSpawned();
    }
}
