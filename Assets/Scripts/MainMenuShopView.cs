using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class MainMenuShopView
{
    public const string RootName = "shop-menu-root";
    public const string BackButtonName = "shop-back-button";

    private const string CoinLabelName = "shop-coin-label";
    private const string CoinIconName = "shop-coin-icon";
    private const string ItemScrollName = "shop-item-scroll";
    private const string DetailTitleName = "shop-detail-title";
    private const string DetailDescriptionName = "shop-detail-description";
    private const string DetailIconName = "shop-detail-icon";
    private const string DetailRankName = "shop-detail-rank";
    private const string DetailCostName = "shop-detail-cost";
    private const string BuyButtonName = "shop-buy-button";
    private const string ItemCardClassName = "shop-menu__item-card";
    private const string ItemCardSelectedClassName = "shop-menu__item-card--selected";
    private const string RankPipFilledClassName = "shop-menu__rank-pip--filled";
    private const int MaxDescriptionCharacters = 105;

    private readonly VisualElement _root;
    private readonly Label _coinLabel;
    private readonly ScrollView _itemScroll;
    private readonly Label _detailTitle;
    private readonly Label _detailDescription;
    private readonly Image _detailIcon;
    private readonly Label _detailRank;
    private readonly Label _detailCost;
    private readonly Button _buyButton;
    private readonly List<ShopPowerUpDefinition> _items = new();
    private readonly Dictionary<ShopPowerUpDefinition, Button> _cardByItem = new();

    private ShopPowerUpDefinition _selectedItem;

    public MainMenuShopView(VisualElement root, IReadOnlyList<ShopPowerUpDefinition> configuredItems, Sprite coinSprite)
    {
        _root = root;
        _coinLabel = root.Q<Label>(CoinLabelName);
        _itemScroll = root.Q<ScrollView>(ItemScrollName);
        _detailTitle = root.Q<Label>(DetailTitleName);
        _detailDescription = root.Q<Label>(DetailDescriptionName);
        _detailIcon = root.Q<Image>(DetailIconName);
        _detailRank = root.Q<Label>(DetailRankName);
        _detailCost = root.Q<Label>(DetailCostName);
        _buyButton = root.Q<Button>(BuyButtonName);

        var coinIcon = root.Q<Image>(CoinIconName);
        if (coinIcon != null)
        {
            coinIcon.sprite = coinSprite;
        }

        LoadItems(configuredItems);
        BuildItemCards();

        if (_buyButton != null)
        {
            _buyButton.clicked += BuySelectedItem;
        }

        SelectItem(_items.Count > 0 ? _items[0] : null);
        Hide();
    }

    public void Show()
    {
        Refresh();
        _root.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        _root.style.display = DisplayStyle.None;
    }

    private void LoadItems(IReadOnlyList<ShopPowerUpDefinition> configuredItems)
    {
        _items.Clear();

        if (configuredItems != null)
        {
            foreach (var item in configuredItems)
            {
                if (item != null)
                {
                    _items.Add(item);
                }
            }
        }

        if (_items.Count == 0)
        {
            _items.Add(CreatePlaceholder("might", "Might", "Raises inflicted damage by 5% per rank.", 200, 120, 5));
            _items.Add(CreatePlaceholder("max_health", "Max Health", "Adds a small permanent boost to maximum health.", 180, 110, 5));
            _items.Add(CreatePlaceholder("move_speed", "Move Speed", "Increases movement speed by 4% per rank.", 160, 100, 5));
            _items.Add(CreatePlaceholder("magnet", "Magnet", "Expands pickup attraction range for coins, food, and XP.", 150, 90, 4));
            _items.Add(CreatePlaceholder("luck", "Luck", "Improves future drop and reward odds.", 220, 140, 3));
            _items.Add(CreatePlaceholder("recovery", "Recovery", "Improves healing received from food pickups.", 140, 95, 4));
        }
    }

    private void BuildItemCards()
    {
        _itemScroll?.contentContainer.Clear();
        _cardByItem.Clear();

        foreach (var item in _items)
        {
            var card = new Button { text = item.DisplayName };
            card.AddToClassList(ItemCardClassName);
            card.clicked += () => SelectItem(item);

            _itemScroll?.contentContainer.Add(card);
            _cardByItem[item] = card;
        }
    }

    private void SelectItem(ShopPowerUpDefinition item)
    {
        _selectedItem = item;

        foreach (var pair in _cardByItem)
        {
            pair.Value.EnableInClassList(ItemCardSelectedClassName, pair.Key == _selectedItem);
        }

        RefreshDetails();
    }

    private void BuySelectedItem()
    {
        if (_selectedItem == null)
        {
            return;
        }

        var rank = GetRank(_selectedItem);
        if (rank >= _selectedItem.MaxRank)
        {
            return;
        }

        var cost = _selectedItem.GetCostForRank(rank);
        if (!MetaCurrency.TrySpendCoins(cost))
        {
            return;
        }

        PlayerPrefs.SetInt(GetRankKey(_selectedItem), rank + 1);
        PlayerPrefs.Save();
        Refresh();
    }

    private void Refresh()
    {
        if (_coinLabel != null)
        {
            _coinLabel.text = MetaCurrency.TotalCoins.ToString();
        }

        RefreshDetails();
    }

    private void RefreshDetails()
    {
        if (_selectedItem == null)
        {
            return;
        }

        var rank = GetRank(_selectedItem);
        var isMaxed = rank >= _selectedItem.MaxRank;
        var cost = _selectedItem.GetCostForRank(rank);

        if (_detailTitle != null) _detailTitle.text = _selectedItem.DisplayName;
        if (_detailDescription != null) _detailDescription.text = GetStableDescription(_selectedItem.Description);
        if (_detailIcon != null) _detailIcon.sprite = _selectedItem.Icon;
        if (_detailRank != null) _detailRank.text = $"Rank {rank}/{_selectedItem.MaxRank}";
        if (_detailCost != null) _detailCost.text = isMaxed ? "Max" : cost.ToString();

        if (_buyButton != null)
        {
            _buyButton.text = isMaxed ? "Maxed" : "Buy";
            _buyButton.SetEnabled(!isMaxed && MetaCurrency.TotalCoins >= cost);
        }

        RefreshCardRanks();
    }

    private void RefreshCardRanks()
    {
        foreach (var pair in _cardByItem)
        {
            var rank = GetRank(pair.Key);
            pair.Value.Clear();
            pair.Value.text = string.Empty;

            var row = new VisualElement();
            row.AddToClassList("shop-menu__item-row");

            var iconFrame = new VisualElement();
            iconFrame.AddToClassList("shop-menu__item-icon-frame");

            var icon = new Image { sprite = pair.Key.Icon };
            icon.AddToClassList("shop-menu__item-icon");
            iconFrame.Add(icon);

            var copy = new VisualElement();
            copy.AddToClassList("shop-menu__item-copy");

            var title = new Label(pair.Key.DisplayName);
            title.AddToClassList("shop-menu__item-title");
            copy.Add(title);

            var pips = new VisualElement();
            pips.AddToClassList("shop-menu__rank-row");

            for (var i = 0; i < pair.Key.MaxRank; i++)
            {
                var pip = new VisualElement();
                pip.AddToClassList("shop-menu__rank-pip");
                pip.EnableInClassList(RankPipFilledClassName, i < rank);
                pips.Add(pip);
            }

            copy.Add(pips);
            row.Add(iconFrame);
            row.Add(copy);
            pair.Value.Add(row);
        }
    }

    private static int GetRank(ShopPowerUpDefinition item)
    {
        return PlayerPrefs.GetInt(GetRankKey(item), 0);
    }

    private static string GetRankKey(ShopPowerUpDefinition item)
    {
        var id = string.IsNullOrWhiteSpace(item.PowerUpId) ? item.name : item.PowerUpId;
        return $"Shop.PowerUp.{id}.Rank";
    }

    private static string GetStableDescription(string description)
    {
        if (string.IsNullOrEmpty(description) || description.Length <= MaxDescriptionCharacters)
        {
            return description;
        }

        return $"{description[..(MaxDescriptionCharacters - 3)]}...";
    }

    private static ShopPowerUpDefinition CreatePlaceholder(string id, string displayName, string description, int baseCost, int costIncrease, int maxRank)
    {
        var item = ScriptableObject.CreateInstance<ShopPowerUpDefinition>();
        item.name = $"Placeholder_{displayName}";
        item.InitializeRuntime(id, displayName, description, baseCost, costIncrease, maxRank);
        return item;
    }
}
