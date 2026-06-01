using System;
using System.Collections.Generic;
using System.Linq;
using GoveKits.Runtime.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CodexPanel : MonoBehaviour
{
    public GameObject choiceItemPrefab;
    public Transform contentParent;
    public ScrollRect scrollRect;

    private const int PageSize = 8;
    private const int ColumnCount = 4;
    private const int RowCount = 2;
    private const float DefaultCardWidth = 90f;
    private const float DefaultCardHeight = 120f;

    private static readonly string[] SeriesOrder =
    {
        "初始", "七罪", "血族", "坚固", "科技", "种子", "暗影", "时序"
    };

    private static readonly Color SelectedButtonColor = new(0.9f, 0.78f, 0.35f, 1f);
    private static readonly Color NormalButtonColor = new(1f, 1f, 1f, 0.92f);
    private static readonly Color SectionColor = new(0.16f, 0.16f, 0.16f, 0.82f);

    private readonly List<GameObject> spawnedCardItems = new();
    private readonly List<CardConfigData> allCards = new();
    private readonly List<CardConfigData> filteredCards = new();
    private readonly Dictionary<string, Button> seriesButtons = new();
    private readonly Dictionary<string, Button> manaButtons = new();

    private RectTransform rootRect;
    private RectTransform panelRect;
    private RectTransform topAreaRect;
    private RectTransform middleAreaRect;
    private RectTransform bottomAreaRect;
    private RectTransform seriesButtonRoot;
    private RectTransform manaButtonRoot;
    private RectTransform detailOverlayRoot;
    private RectTransform detailCardAnchor;
    private RectTransform hoverTooltipRoot;
    private RectTransform cardGridRoot;
    private GridLayoutGroup contentGrid;
    private Button backButton;
    private Button prevPageButton;
    private Button nextPageButton;
    private TMP_Text pageText;
    private TMP_Text hoverTooltipText;
    private TMP_Text detailRemarkText;
    private TMP_InputField searchInput;
    private GameObject detailCardInstance;
    private TMP_FontAsset defaultFontAsset;

    private string selectedSeries = "全部";
    private int? selectedMana;
    private string searchText = string.Empty;
    private int pageIndex;
    private CardConfigData hoveredCard;
    private bool uiBuilt;

    public void Show()
    {
        gameObject.SetActive(true);
        EnsureUiBuilt();
        ReloadAndRender();
    }

    private void Update()
    {
        if (rootRect == null || hoverTooltipRoot == null || !hoverTooltipRoot.gameObject.activeSelf)
        {
            return;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rootRect, Input.mousePosition, null, out Vector2 localPoint))
        {
            return;
        }

        Vector2 size = hoverTooltipRoot.sizeDelta;
        hoverTooltipRoot.anchoredPosition = new Vector2(
            Mathf.Clamp(localPoint.x - 24f, rootRect.rect.xMin + size.x, rootRect.rect.xMax),
            Mathf.Clamp(localPoint.y + 20f, rootRect.rect.yMin + size.y, rootRect.rect.yMax));
    }

    private void EnsureUiBuilt()
    {
        if (uiBuilt)
        {
            return;
        }

        rootRect = transform as RectTransform;
        panelRect = FindOrCreateRectTransform(transform, "Panel");
        panelRect.SetParent(transform, false);
        Stretch(panelRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        Image rootImage = GetOrAddComponent<Image>(gameObject);
        rootImage.color = new Color(0.05f, 0.05f, 0.05f, 0.9f);

        scrollRect ??= GetComponentInChildren<ScrollRect>(true);
        if (scrollRect == null)
        {
            Debug.LogError("CodexPanel: 未找到 ScrollRect。");
            return;
        }

        backButton = panelRect.GetComponentInChildren<Button>(true);
        if (backButton == null)
        {
            backButton = GetComponentInChildren<Button>(true);
        }

        topAreaRect = FindOrCreateRectTransform(panelRect, "顶部系列栏");
        middleAreaRect = FindOrCreateRectTransform(panelRect, "中部区域");
        bottomAreaRect = FindOrCreateRectTransform(panelRect, "底部区域");

        SetupSection(topAreaRect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -84f), new Vector2(-24f, -24f));
        SetupSection(middleAreaRect, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(24f, 104f), new Vector2(-24f, -94f));
        SetupSection(bottomAreaRect, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(24f, 20f), new Vector2(-24f, 84f));

        ConfigureBackButton();
        ConfigureScrollView();
        ConfigureTopArea();
        ConfigureBottomArea();
        ConfigureMiddleArea();
        EnsureOverlayUi();

        uiBuilt = true;
    }

    private void ConfigureBackButton()
    {
        if (backButton == null)
        {
            return;
        }

        RectTransform backRect = backButton.transform as RectTransform;
        if (backRect == null)
        {
            return;
        }

        backRect.SetParent(panelRect, false);
        Stretch(backRect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-116f, 20f), new Vector2(-24f, 80f));

        Image image = backButton.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.18f, 0.18f, 0.18f, 0.95f);
        }

        TMP_Text tmpText = backButton.GetComponentInChildren<TMP_Text>(true);
        if (tmpText != null)
        {
            tmpText.text = "返回";
            tmpText.fontSize = 28;
            tmpText.color = Color.white;
        }
    }

    private void ConfigureScrollView()
    {
        RectTransform scrollRectTransform = scrollRect.transform as RectTransform;
        if (scrollRectTransform == null)
        {
            return;
        }

        scrollRectTransform.SetParent(middleAreaRect, false);
        Stretch(scrollRectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(72f, 52f), new Vector2(-72f, -24f));

        scrollRect.horizontal = false;
        scrollRect.vertical = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        RectTransform viewportRect = scrollRect.viewport;
        if (viewportRect == null)
        {
            viewportRect = FindOrCreateRectTransform(scrollRectTransform, "Viewport");
            Stretch(viewportRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            GetOrAddComponent<Image>(viewportRect.gameObject).color = new Color(1f, 1f, 1f, 0.001f);
            GetOrAddComponent<Mask>(viewportRect.gameObject).showMaskGraphic = false;
            scrollRect.viewport = viewportRect;
        }

        if (scrollRect.horizontalScrollbar != null)
        {
            scrollRect.horizontalScrollbar.gameObject.SetActive(false);
        }

        if (scrollRect.verticalScrollbar != null)
        {
            scrollRect.verticalScrollbar.gameObject.SetActive(false);
        }

        Transform originalContent = scrollRect.content;
        if (originalContent != null && originalContent != viewportRect && originalContent.name != "CardGridRoot")
        {
            RectTransform originalContentRect = originalContent as RectTransform;
            if (originalContentRect != null)
            {
                // 旧场景里的 Content 是给纵向滚动列表用的，高度靠旧布局撑开。
                // April-Fool 图鉴是独立网格根节点，这里直接复用一个新的满屏网格层，避免 0 高裁剪。
                originalContentRect.gameObject.SetActive(false);
            }
        }

        cardGridRoot = FindOrCreateRectTransform(viewportRect, "CardGridRoot");
        cardGridRoot.gameObject.SetActive(true);
        Stretch(cardGridRoot, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f));
        cardGridRoot.SetAsLastSibling();
        contentParent = cardGridRoot;
        scrollRect.content = cardGridRoot;

        contentGrid = contentParent.GetComponent<GridLayoutGroup>();
        if (contentGrid == null)
        {
            contentGrid = contentParent.gameObject.AddComponent<GridLayoutGroup>();
        }

        if (contentGrid == null)
        {
            Debug.LogError("CodexPanel: 无法为 Content 挂载 GridLayoutGroup。");
            return;
        }

        contentGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        contentGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
        contentGrid.childAlignment = TextAnchor.UpperCenter;
        contentGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        contentGrid.constraintCount = ColumnCount;
        contentGrid.spacing = new Vector2(24f, 28f);
        contentGrid.padding = new RectOffset(4, 4, 4, 4);

        LayoutElement gridLayoutElement = contentParent.GetComponent<LayoutElement>();
        if (gridLayoutElement != null)
        {
            Destroy(gridLayoutElement);
        }
    }

    private void ConfigureTopArea()
    {
        Image sectionImage = GetOrAddComponent<Image>(topAreaRect.gameObject);
        sectionImage.color = SectionColor;

        seriesButtonRoot = FindOrCreateRectTransform(topAreaRect, "系列按钮容器");
        Stretch(seriesButtonRoot, Vector2.zero, Vector2.one, new Vector2(16f, 8f), new Vector2(-16f, -8f));

        HorizontalLayoutGroup layout = GetOrAddComponent<HorizontalLayoutGroup>(seriesButtonRoot.gameObject);
        layout.spacing = 10f;
        layout.padding = new RectOffset(4, 4, 0, 0);
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    private void ConfigureBottomArea()
    {
        Image sectionImage = GetOrAddComponent<Image>(bottomAreaRect.gameObject);
        sectionImage.color = SectionColor;

        manaButtonRoot = FindOrCreateRectTransform(bottomAreaRect, "法力值栏");
        Stretch(manaButtonRoot, new Vector2(0f, 0f), new Vector2(0.62f, 1f), new Vector2(12f, 8f), new Vector2(-8f, -8f));

        HorizontalLayoutGroup manaLayout = GetOrAddComponent<HorizontalLayoutGroup>(manaButtonRoot.gameObject);
        manaLayout.spacing = 8f;
        manaLayout.padding = new RectOffset(4, 4, 0, 0);
        manaLayout.childAlignment = TextAnchor.MiddleLeft;
        manaLayout.childControlWidth = false;
        manaLayout.childControlHeight = true;
        manaLayout.childForceExpandWidth = false;
        manaLayout.childForceExpandHeight = false;

        RectTransform searchRect = FindOrCreateRectTransform(bottomAreaRect, "搜索输入框");
        Stretch(searchRect, new Vector2(0.64f, 0.18f), new Vector2(1f, 0.82f), new Vector2(0f, 0f), new Vector2(0f, 0f));
        searchInput = EnsureSearchInput(searchRect);
    }

    private void ConfigureMiddleArea()
    {
        Image sectionImage = GetOrAddComponent<Image>(middleAreaRect.gameObject);
        sectionImage.color = SectionColor;

        prevPageButton = EnsureNavButton(middleAreaRect, "左翻页", "<", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(12f, -84f), new Vector2(64f, 84f));
        nextPageButton = EnsureNavButton(middleAreaRect, "右翻页", ">", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-64f, -84f), new Vector2(-12f, 84f));

        prevPageButton.onClick.RemoveAllListeners();
        prevPageButton.onClick.AddListener(PrevPage);
        nextPageButton.onClick.RemoveAllListeners();
        nextPageButton.onClick.AddListener(NextPage);

        RectTransform pageTextRect = FindOrCreateRectTransform(middleAreaRect, "页码文本");
        Stretch(pageTextRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-120f, 10f), new Vector2(120f, 46f));
        pageText = EnsureText(pageTextRect, "页码", 26, TextAlignmentOptions.Center);
    }

    private void ReloadAndRender()
    {
        allCards.Clear();
        List<CardConfigData> loadedCards = ConfigCore.LoadAll<CardConfigData>();
        if (loadedCards != null)
        {
            foreach (CardConfigData card in loadedCards)
            {
                if (card == null || card.id <= 0 || string.IsNullOrWhiteSpace(card.名称))
                {
                    continue;
                }

                allCards.Add(card);
            }
        }

        allCards.Sort(CompareCardOrder);

        BuildSeriesButtons();
        BuildManaButtons();

        if (searchInput != null)
        {
            searchInput.SetTextWithoutNotify(searchText);
        }

        ApplyFilter();
    }

    private void BuildSeriesButtons()
    {
        ClearChildren(seriesButtonRoot);
        seriesButtons.Clear();

        List<string> seriesLabels = new() { "全部" };
        IEnumerable<string> orderedSeries = allCards
            .Select(card => NormalizeSeries(card.系列))
            .Distinct()
            .OrderBy(GetSeriesSortIndex)
            .ThenBy(value => value);

        seriesLabels.AddRange(orderedSeries);

        if (!seriesLabels.Contains(selectedSeries))
        {
            selectedSeries = "全部";
        }

        foreach (string label in seriesLabels)
        {
            Button button = CreateFilterButton(seriesButtonRoot, label, 84f, () =>
            {
                selectedSeries = label;
                pageIndex = 0;
                ApplyFilter();
            });
            seriesButtons[label] = button;
        }
    }

    private void BuildManaButtons()
    {
        ClearChildren(manaButtonRoot);
        manaButtons.Clear();

        List<string> manaLabels = new() { "全部" };
        manaLabels.AddRange(allCards.Select(card => card.费用).Distinct().OrderBy(value => value).Select(value => value.ToString()));

        if (selectedMana.HasValue && !manaLabels.Contains(selectedMana.Value.ToString()))
        {
            selectedMana = null;
        }

        foreach (string label in manaLabels)
        {
            Button button = CreateFilterButton(manaButtonRoot, label, 60f, () =>
            {
                selectedMana = label == "全部" ? null : int.Parse(label);
                pageIndex = 0;
                ApplyFilter();
            });
            manaButtons[label] = button;
        }
    }

    private void ApplyFilter()
    {
        IEnumerable<CardConfigData> query = allCards;

        if (!string.IsNullOrEmpty(selectedSeries) && selectedSeries != "全部")
        {
            query = query.Where(card => NormalizeSeries(card.系列) == selectedSeries);
        }

        if (selectedMana.HasValue)
        {
            query = query.Where(card => card.费用 == selectedMana.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            string keyword = searchText.Trim();
            query = query.Where(card =>
                ContainsText(card.名称, keyword) ||
                ContainsText(card.描述, keyword) ||
                ContainsText(card.趣闻, keyword));
        }

        filteredCards.Clear();
        filteredCards.AddRange(query);

        UpdateFilterVisualState();
        RenderPage();
    }

    private void RenderPage()
    {
        ClearSpawnedCards();
        ConfigureGridCellSize();

        if (filteredCards.Count == 0)
        {
            pageIndex = 0;
            if (pageText != null)
            {
                pageText.text = "无匹配卡牌";
            }

            if (prevPageButton != null) prevPageButton.interactable = false;
            if (nextPageButton != null) nextPageButton.interactable = false;
            return;
        }

        int totalPages = Mathf.Max(1, Mathf.CeilToInt(filteredCards.Count / (float)PageSize));
        pageIndex = Mathf.Clamp(pageIndex, 0, totalPages - 1);

        int startIndex = pageIndex * PageSize;
        int endIndex = Mathf.Min(startIndex + PageSize, filteredCards.Count);
        for (int i = startIndex; i < endIndex; i++)
        {
            SpawnCodexCard(filteredCards[i], contentParent);
        }

        if (pageText != null)
        {
            pageText.text = $"{pageIndex + 1} / {totalPages}";
        }

        if (prevPageButton != null) prevPageButton.interactable = pageIndex > 0;
        if (nextPageButton != null) nextPageButton.interactable = pageIndex < totalPages - 1;
    }

    private void SpawnCodexCard(CardConfigData config, Transform parent)
    {
        if (choiceItemPrefab == null || parent == null)
        {
            return;
        }

        GameObject go = Instantiate(choiceItemPrefab, parent);
        go.name = $"卡牌_{config.名称}";
        spawnedCardItems.Add(go);

        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            LayoutElement layoutElement = GetOrAddComponent<LayoutElement>(go);
            layoutElement.preferredWidth = contentGrid != null ? contentGrid.cellSize.x : DefaultCardWidth;
            layoutElement.preferredHeight = contentGrid != null ? contentGrid.cellSize.y : DefaultCardHeight;
        }

        ApplyCardItemData(go, config);
        BindCardItemEvents(go, config);
    }

    private void ApplyCardItemData(GameObject go, CardConfigData config)
    {
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
            btn.interactable = true;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => ShowDetailOverlay(config));
        }
    }

    private void BindCardItemEvents(GameObject item, CardConfigData card)
    {
        if (item == null)
        {
            return;
        }

        EventTrigger trigger = GetOrAddComponent<EventTrigger>(item);
        trigger.triggers ??= new List<EventTrigger.Entry>();
        trigger.triggers.Clear();

        AddTriggerEntry(trigger, EventTriggerType.PointerEnter, _ => OnCardPointerEnter(card));
        AddTriggerEntry(trigger, EventTriggerType.PointerExit, _ => OnCardPointerExit(card));
    }

    private static void AddTriggerEntry(EventTrigger trigger, EventTriggerType type, Action<BaseEventData> callback)
    {
        EventTrigger.Entry entry = new() { eventID = type };
        entry.callback.AddListener(data => callback?.Invoke(data));
        trigger.triggers.Add(entry);
    }

    private void OnCardPointerEnter(CardConfigData card)
    {
        hoveredCard = card;
        ShowHoverTooltip(card);
    }

    private void OnCardPointerExit(CardConfigData card)
    {
        if (hoveredCard == card)
        {
            hoveredCard = null;
        }

        HideHoverTooltip();
    }

    private void ShowHoverTooltip(CardConfigData card)
    {
        if (hoverTooltipRoot == null || hoverTooltipText == null || card == null)
        {
            return;
        }

        string description = GetCardDescription(card);
        hoverTooltipText.text = string.IsNullOrWhiteSpace(description) ? "无描述" : description;
        hoverTooltipRoot.gameObject.SetActive(true);
    }

    private void HideHoverTooltip()
    {
        if (hoverTooltipRoot != null)
        {
            hoverTooltipRoot.gameObject.SetActive(false);
        }
    }

    private void ShowDetailOverlay(CardConfigData card)
    {
        if (detailOverlayRoot == null || detailCardAnchor == null || detailRemarkText == null || card == null)
        {
            return;
        }

        detailOverlayRoot.gameObject.SetActive(true);
        detailRemarkText.text = GetCardRemark(card);
        CreateOrRefreshDetailCard(card);
    }

    private void CreateOrRefreshDetailCard(CardConfigData card)
    {
        if (choiceItemPrefab == null || detailCardAnchor == null)
        {
            return;
        }

        if (detailCardInstance == null)
        {
            detailCardInstance = Instantiate(choiceItemPrefab, detailCardAnchor);
            detailCardInstance.name = "图鉴详情卡牌";
        }

        detailCardInstance.SetActive(true);
        RectTransform rect = detailCardInstance.transform as RectTransform;
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one * 2f;
        }

        ApplyCardItemData(detailCardInstance, card);
        Button btn = detailCardInstance.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.interactable = false;
        }
    }

    private void EnsureOverlayUi()
    {
        detailOverlayRoot = FindOrCreateRectTransform(panelRect, "图鉴详情遮罩");
        Stretch(detailOverlayRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        detailOverlayRoot.SetAsLastSibling();

        Image overlayImage = GetOrAddComponent<Image>(detailOverlayRoot.gameObject);
        overlayImage.color = new Color(0f, 0f, 0f, 0.95f);

        Button closeButton = GetOrAddComponent<Button>(detailOverlayRoot.gameObject);
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(HideDetailOverlay);

        detailCardAnchor = FindOrCreateRectTransform(detailOverlayRoot, "详情卡牌锚点");
        Stretch(detailCardAnchor, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), new Vector2(-120f, -180f), new Vector2(120f, 180f));

        RectTransform remarkRect = FindOrCreateRectTransform(detailOverlayRoot, "趣闻文本");
        Stretch(remarkRect, new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.36f), new Vector2(-420f, 0f), new Vector2(420f, 0f));
        detailRemarkText = EnsureText(remarkRect, "趣闻", 28, TextAlignmentOptions.TopGeoAligned);
        detailRemarkText.enableWordWrapping = true;

        detailOverlayRoot.gameObject.SetActive(false);

        hoverTooltipRoot = FindOrCreateRectTransform(panelRect, "图鉴悬停描述");
        Stretch(hoverTooltipRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-180f, -85f), new Vector2(180f, 85f));
        hoverTooltipRoot.pivot = new Vector2(1f, 0f);

        Image tooltipImage = GetOrAddComponent<Image>(hoverTooltipRoot.gameObject);
        tooltipImage.color = new Color(0f, 0f, 0f, 0.85f);
        tooltipImage.raycastTarget = false;

        hoverTooltipText = EnsureText(hoverTooltipRoot, "提示文本", 22, TextAlignmentOptions.MidlineLeft);
        hoverTooltipText.margin = new Vector4(16f, 14f, 16f, 14f);
        hoverTooltipText.enableWordWrapping = true;
        hoverTooltipText.raycastTarget = false;
        hoverTooltipRoot.gameObject.SetActive(false);
    }

    private void HideDetailOverlay()
    {
        if (detailOverlayRoot != null)
        {
            detailOverlayRoot.gameObject.SetActive(false);
        }
    }

    private Button EnsureNavButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        RectTransform buttonRect = FindOrCreateRectTransform(parent, name);
        Stretch(buttonRect, anchorMin, anchorMax, offsetMin, offsetMax);

        Image image = GetOrAddComponent<Image>(buttonRect.gameObject);
        image.color = new Color(0.18f, 0.18f, 0.18f, 0.95f);

        Button button = GetOrAddComponent<Button>(buttonRect.gameObject);
        TMP_Text buttonText = EnsureText(buttonRect, $"{name}_文本", 42, TextAlignmentOptions.Center);
        buttonText.text = label;
        buttonText.color = Color.white;

        return button;
    }

    private Button CreateFilterButton(Transform parent, string label, float width, Action onClick)
    {
        GameObject go = new(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, 44f);

        Image image = go.GetComponent<Image>();
        image.color = NormalButtonColor;

        Button button = go.GetComponent<Button>();
        button.onClick.AddListener(() => onClick?.Invoke());

        LayoutElement layout = go.GetComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.preferredHeight = 44f;

        RectTransform textRect = FindOrCreateRectTransform(go.transform, "文本");
        Stretch(textRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        TMP_Text text = EnsureText(textRect, label, 22, TextAlignmentOptions.Center);
        text.text = label;
        text.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        return button;
    }

    private TMP_InputField EnsureSearchInput(RectTransform searchRect)
    {
        Image bg = GetOrAddComponent<Image>(searchRect.gameObject);
        bg.color = new Color(0.95f, 0.95f, 0.95f, 0.95f);

        TMP_InputField inputField = GetOrAddComponent<TMP_InputField>(searchRect.gameObject);
        inputField.lineType = TMP_InputField.LineType.SingleLine;
        inputField.onValueChanged.RemoveListener(OnSearchChanged);
        inputField.onValueChanged.AddListener(OnSearchChanged);

        RectTransform viewport = FindOrCreateRectTransform(searchRect, "Text Area");
        Stretch(viewport, Vector2.zero, Vector2.one, new Vector2(14f, 8f), new Vector2(-14f, -8f));
        GetOrAddComponent<RectMask2D>(viewport.gameObject);

        RectTransform textRect = FindOrCreateRectTransform(viewport, "Text");
        Stretch(textRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        TMP_Text inputText = EnsureText(textRect, "搜索文本", 22, TextAlignmentOptions.Left);
        inputText.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        RectTransform placeholderRect = FindOrCreateRectTransform(viewport, "Placeholder");
        Stretch(placeholderRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        TMP_Text placeholderText = EnsureText(placeholderRect, "搜索卡牌名称、描述、趣闻", 22, TextAlignmentOptions.Left);
        placeholderText.fontStyle = FontStyles.Italic;
        placeholderText.color = new Color(0.45f, 0.45f, 0.45f, 0.9f);

        inputField.textViewport = viewport;
        inputField.textComponent = inputText as TextMeshProUGUI;
        inputField.placeholder = placeholderText;
        return inputField;
    }

    private TMP_Text EnsureText(RectTransform parent, string defaultText, float fontSize, TextAlignmentOptions alignment)
    {
        RectTransform textHost = parent;
        TMP_Text text = parent.GetComponent<TextMeshProUGUI>();
        if (text == null && parent.GetComponent<Graphic>() != null)
        {
            textHost = FindOrCreateRectTransform(parent, "Text");
            Stretch(textHost, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text = textHost.GetComponent<TextMeshProUGUI>();
        }

        if (text == null)
        {
            text = textHost.gameObject.AddComponent<TextMeshProUGUI>();
        }

        text.text = defaultText;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        text.enableWordWrapping = false;
        text.raycastTarget = false;

        if (defaultFontAsset == null)
        {
            defaultFontAsset = GetComponentInChildren<TMP_Text>(true)?.font;
            defaultFontAsset ??= TMP_Settings.defaultFontAsset;
        }

        if (defaultFontAsset != null)
        {
            text.font = defaultFontAsset;
        }

        return text;
    }

    private void UpdateFilterVisualState()
    {
        foreach ((string key, Button button) in seriesButtons)
        {
            SetButtonSelected(button, key == selectedSeries);
        }

        foreach ((string key, Button button) in manaButtons)
        {
            bool isSelected = selectedMana.HasValue ? key == selectedMana.Value.ToString() : key == "全部";
            SetButtonSelected(button, isSelected);
        }
    }

    private static void SetButtonSelected(Button button, bool selected)
    {
        if (button == null)
        {
            return;
        }

        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = selected ? SelectedButtonColor : NormalButtonColor;
        }
    }

    private void ConfigureGridCellSize()
    {
        if (contentGrid == null)
        {
            return;
        }

        RectTransform viewportRect = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.transform as RectTransform;
        if (viewportRect == null)
        {
            return;
        }

        float width = viewportRect.rect.width;
        float height = viewportRect.rect.height;
        if (width <= 0f || height <= 0f)
        {
            width = 860f;
            height = 540f;
        }

        float prefabWidth = DefaultCardWidth;
        float prefabHeight = DefaultCardHeight;
        RectTransform prefabRect = choiceItemPrefab != null ? choiceItemPrefab.GetComponent<RectTransform>() : null;
        if (prefabRect != null && prefabRect.rect.width > 0f && prefabRect.rect.height > 0f)
        {
            prefabWidth = prefabRect.rect.width;
            prefabHeight = prefabRect.rect.height;
        }

        float aspect = prefabWidth / prefabHeight;
        float availableWidth = width - contentGrid.padding.left - contentGrid.padding.right - contentGrid.spacing.x * (ColumnCount - 1);
        float availableHeight = height - contentGrid.padding.top - contentGrid.padding.bottom - contentGrid.spacing.y * (RowCount - 1);
        float cellWidth = availableWidth / ColumnCount;
        float cellHeight = cellWidth / aspect;
        float maxHeight = availableHeight / RowCount;
        if (cellHeight > maxHeight)
        {
            cellHeight = maxHeight;
            cellWidth = cellHeight * aspect;
        }

        contentGrid.cellSize = new Vector2(cellWidth, cellHeight);
    }

    private static int CompareCardOrder(CardConfigData left, CardConfigData right)
    {
        int seriesCompare = GetSeriesSortIndex(NormalizeSeries(left.系列)).CompareTo(GetSeriesSortIndex(NormalizeSeries(right.系列)));
        if (seriesCompare != 0)
        {
            return seriesCompare;
        }

        int manaCompare = left.费用.CompareTo(right.费用);
        if (manaCompare != 0)
        {
            return manaCompare;
        }

        return left.id.CompareTo(right.id);
    }

    private static int GetSeriesSortIndex(string series)
    {
        int index = Array.IndexOf(SeriesOrder, series);
        return index >= 0 ? index : SeriesOrder.Length + 1;
    }

    private static string NormalizeSeries(string series)
    {
        return string.IsNullOrWhiteSpace(series) ? "其他" : series;
    }

    private static bool ContainsText(string source, string keyword)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(keyword))
        {
            return false;
        }

        return source.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        rect.localScale = Vector3.one;
    }

    private static void SetupSection(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        Stretch(rect, anchorMin, anchorMax, offsetMin, offsetMax);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private static RectTransform FindOrCreateRectTransform(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            return child as RectTransform;
        }

        GameObject go = new(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    private static void ClearChildren(Transform parent)
    {
        if (parent == null)
        {
            return;
        }

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
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

    private string GetCardDescription(CardConfigData card)
    {
        BaseCard baseCard = CardFactoryCore.CreateCard(card.id);
        return baseCard != null ? baseCard.Description() : (string.IsNullOrWhiteSpace(card.描述) ? "无描述" : card.描述);
    }

    private string GetCardRemark(CardConfigData card)
    {
        BaseCard baseCard = CardFactoryCore.CreateCard(card.id);
        string remark = baseCard != null ? baseCard.Intresting : card.趣闻;
        return string.IsNullOrWhiteSpace(remark) ? "暂无趣闻" : remark;
    }

    private void OnSearchChanged(string value)
    {
        searchText = value ?? string.Empty;
        pageIndex = 0;
        ApplyFilter();
    }

    private void PrevPage()
    {
        pageIndex--;
        RenderPage();
    }

    private void NextPage()
    {
        pageIndex++;
        RenderPage();
    }

    private void ClearSpawnedCards()
    {
        foreach (GameObject go in spawnedCardItems)
        {
            if (go != null)
            {
                Destroy(go);
            }
        }

        spawnedCardItems.Clear();
    }

    private void OnDisable()
    {
        HideHoverTooltip();
        HideDetailOverlay();
    }
}
