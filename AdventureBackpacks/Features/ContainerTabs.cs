using System;
using AdventureBackpacks.Components;
using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using AdventureBackpacks.Patches;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdventureBackpacks.Features;

public static class ContainerTabs
{
    public static bool FeatureInitialized = false;

    private static Container _cachedExternalContainer;
    private static bool _isViewingBackpack;

    private static GameObject _tabBar;
    private static Button _chestTabButton;
    private static Button _backpackTabButton;

    private static Vector2 _originalContainerSizeDelta;
    private static Vector2 _originalContainerAnchoredPos;
    private static Vector2 _originalGridSizeDelta;
    private static Vector2 _originalGridAnchoredPos;
    private static RectTransform _originalGridDirectChild;
    private static bool _isContainerStretched;
    private const float ContainerExtraHeight = 44f;

    public static bool HasActiveExternalContainer => _cachedExternalContainer != null && (bool)_cachedExternalContainer;
    public static bool IsViewingBackpack => _isViewingBackpack;
    public static Container CachedExternalContainer => _cachedExternalContainer;

    public static void InitializeWithContainer(Container container, InventoryGui gui, Player player)
    {
        if (container == null || !container || gui == null || player == null)
            return;

        _cachedExternalContainer = container;
        _isViewingBackpack = false;
        InventoryGuiPatches.BackpackIsOpen = false;

        EnsureTabBar(gui);

        if (_tabBar != null && _tabBar)
        {
            EnsureContainerStretched(gui);
            PositionTabBar(gui);

            string chestTabLabel = Localization.instance != null ? Localization.instance.Localize("$piece_chestwood") : "Chest";
            string backpackTabLabel = Localization.instance != null ? Localization.instance.Localize("$vapok_mod_backpack") : "Backpack";

            SetTabButtonText(_chestTabButton, chestTabLabel);
            SetTabButtonText(_backpackTabButton, backpackTabLabel);

            _tabBar.SetActive(true);

            UpdateTabVisuals();
        }
    }

    private static void PositionTabBar(InventoryGui gui, RectTransform tabBarRect = null)
    {
        if (gui == null || gui.m_container == null)
            return;

        if (tabBarRect == null)
        {
            if (_tabBar == null || !_tabBar)
                return;
            tabBarRect = _tabBar.GetComponent<RectTransform>();
        }

        if (tabBarRect == null)
            return;

        tabBarRect.anchorMin = new Vector2(0.5f, 1f);
        tabBarRect.anchorMax = new Vector2(0.5f, 1f);
        tabBarRect.pivot = new Vector2(0.5f, 1f);
        tabBarRect.sizeDelta = new Vector2(280f, 34f);

        RectTransform nameRect = gui.m_containerName != null ? gui.m_containerName.rectTransform : null;
        if (nameRect != null)
        {
            float nameHeight = nameRect.rect.height > 10f ? nameRect.rect.height : 30f;
            float nameBottom = nameRect.anchoredPosition.y - (nameHeight * nameRect.pivot.y);
            tabBarRect.anchoredPosition = new Vector2(0f, nameBottom - 6f);
        }
        else
        {
            tabBarRect.anchoredPosition = new Vector2(0f, -60f);
        }
    }

    private static void EnsureTabBar(InventoryGui gui)
    {
        if (gui == null || gui.m_container == null)
            return;

        if (_tabBar != null && _tabBar)
            return;

        GameObject tabBarObj = new GameObject("AB_ContainerTabs", typeof(RectTransform));
        RectTransform tabBarRect = tabBarObj.GetComponent<RectTransform>();
        tabBarRect.SetParent(gui.m_container, false);

        HorizontalLayoutGroup layout = tabBarObj.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 8f;

        PositionTabBar(gui, tabBarRect);

        Button template = gui.m_tabCraft != null ? gui.m_tabCraft : gui.m_takeAllButton;
        if (template == null)
            return;

        Button chestButton = UnityEngine.Object.Instantiate(template, tabBarRect, false);
        chestButton.name = "ChestTab";
        chestButton.onClick.RemoveAllListeners();
        chestButton.onClick.AddListener(() => SwitchToChest(gui, Player.m_localPlayer));

        UITooltip chestTooltip = chestButton.GetComponent<UITooltip>();
        if (chestTooltip != null)
        {
            UnityEngine.Object.Destroy(chestTooltip);
        }

        Localize[] chestLocalizers = chestButton.GetComponentsInChildren<Localize>(true);
        for (int i = 0; i < chestLocalizers.Length; i++)
        {
            UnityEngine.Object.Destroy(chestLocalizers[i]);
        }

        Image chestImage = chestButton.GetComponent<Image>();
        if (chestImage != null)
        {
            chestButton.targetGraphic = chestImage;
        }

        LayoutElement chestLayout = chestButton.gameObject.GetComponent<LayoutElement>();
        if (chestLayout == null)
        {
            chestLayout = chestButton.gameObject.AddComponent<LayoutElement>();
        }
        chestLayout.minWidth = 110f;
        chestLayout.preferredWidth = 135f;
        chestLayout.minHeight = 30f;
        chestLayout.preferredHeight = 34f;
        chestLayout.flexibleWidth = 0f;
        chestLayout.flexibleHeight = 0f;

        TMP_Text chestLabel = chestButton.GetComponentInChildren<TMP_Text>();
        if (chestLabel != null)
        {
            chestLabel.text = Localization.instance != null ? Localization.instance.Localize("$piece_chestwood") : "Chest";
            chestLabel.enableAutoSizing = true;
            chestLabel.fontSizeMin = 11f;
            chestLabel.fontSizeMax = 18f;
            chestLabel.color = new Color(1f, 0.85f, 0.35f, 1f);
        }

        Button backpackButton = UnityEngine.Object.Instantiate(template, tabBarRect, false);
        backpackButton.name = "BackpackTab";
        backpackButton.onClick.RemoveAllListeners();
        backpackButton.onClick.AddListener(() => SwitchToBackpack(gui, Player.m_localPlayer));

        UITooltip bpTooltip = backpackButton.GetComponent<UITooltip>();
        if (bpTooltip != null)
        {
            UnityEngine.Object.Destroy(bpTooltip);
        }

        Localize[] bpLocalizers = backpackButton.GetComponentsInChildren<Localize>(true);
        for (int i = 0; i < bpLocalizers.Length; i++)
        {
            UnityEngine.Object.Destroy(bpLocalizers[i]);
        }

        Image bpImage = backpackButton.GetComponent<Image>();
        if (bpImage != null)
        {
            backpackButton.targetGraphic = bpImage;
        }

        LayoutElement bpLayout = backpackButton.gameObject.GetComponent<LayoutElement>();
        if (bpLayout == null)
        {
            bpLayout = backpackButton.gameObject.AddComponent<LayoutElement>();
        }
        bpLayout.minWidth = 110f;
        bpLayout.preferredWidth = 135f;
        bpLayout.minHeight = 30f;
        bpLayout.preferredHeight = 34f;
        bpLayout.flexibleWidth = 0f;
        bpLayout.flexibleHeight = 0f;

        TMP_Text bpLabel = backpackButton.GetComponentInChildren<TMP_Text>();
        if (bpLabel != null)
        {
            bpLabel.text = Localization.instance != null ? Localization.instance.Localize("$vapok_mod_backpack") : "Backpack";
            bpLabel.enableAutoSizing = true;
            bpLabel.fontSizeMin = 11f;
            bpLabel.fontSizeMax = 18f;
            bpLabel.color = new Color(1f, 0.85f, 0.35f, 1f);
        }

        _tabBar = tabBarObj;
        _chestTabButton = chestButton;
        _backpackTabButton = backpackButton;
    }

    public static void SwitchToBackpack(InventoryGui gui, Player player)
    {
        if (gui == null || player == null || !player.CanOpenBackpack())
            return;

        BackpackComponent backpack = player.GetEquippedBackpack();
        if (backpack == null)
            return;

        Container backpackContainer = player.GetBackpackContainerProxy();
        if (backpackContainer == null)
            return;

        backpack.UpdateContainerSizing(ref backpackContainer);

        if (gui.m_dragItem != null)
        {
            gui.SetupDragItem(null, null, 1);
        }

        _isViewingBackpack = true;
        InventoryGuiPatches.BackpackIsOpen = true;

        gui.m_currentContainer = backpackContainer;
        gui.m_firstContainerUpdate = true;
        gui.m_containerGrid.ResetView();

        PositionTabBar(gui);
        UpdateTabVisuals();
    }

    public static void SwitchToChest(InventoryGui gui, Player player)
    {
        if (gui == null || player == null)
            return;

        if (_cachedExternalContainer == null || !_cachedExternalContainer)
        {
            Reset(gui);
            return;
        }

        if (gui.m_dragItem != null)
        {
            gui.SetupDragItem(null, null, 1);
        }

        _isViewingBackpack = false;
        InventoryGuiPatches.BackpackIsOpen = false;

        gui.m_currentContainer = _cachedExternalContainer;
        gui.m_firstContainerUpdate = true;
        gui.m_containerGrid.ResetView();

        PositionTabBar(gui);
        UpdateTabVisuals();
    }

    public static void ToggleTab(InventoryGui gui, Player player)
    {
        if (_isViewingBackpack)
        {
            SwitchToChest(gui, player);
        }
        else
        {
            SwitchToBackpack(gui, player);
        }
    }

    public static void Update(InventoryGui gui, Player player)
    {
        if (gui == null || player == null)
            return;

        if (_cachedExternalContainer == null || !_cachedExternalContainer)
        {
            Reset(gui);
            return;
        }

        if (!player.CanOpenBackpack())
        {
            if (_isViewingBackpack)
            {
                SwitchToChest(gui, player);
            }
            Reset(gui);
            return;
        }

        if (_isViewingBackpack)
        {
            float distance = Vector3.Distance(_cachedExternalContainer.transform.position, player.transform.position);
            if (distance > gui.m_autoCloseDistance)
            {
                gui.CloseContainer();
                return;
            }
        }

        if (!InventoryGuiPatches.CheckForTextInput() && (gui.m_splitDialog == null || !gui.m_splitDialog.IsActive))
        {
            if (ZInput.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (_isViewingBackpack)
                {
                    SwitchToChest(gui, player);
                }
            }
            else if (ZInput.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (!_isViewingBackpack)
                {
                    SwitchToBackpack(gui, player);
                }
            }
            else if (ZInput.GetButtonDown("JoyRStick"))
            {
                ToggleTab(gui, player);
            }
        }
    }

    public static void ClearExternalContainer(InventoryGui gui = null)
    {
        if (_cachedExternalContainer != null)
        {
            if (_cachedExternalContainer)
            {
                _cachedExternalContainer.SetInUse(false);
            }
            _cachedExternalContainer = null;
        }

        _isViewingBackpack = false;

        if (gui == null)
        {
            gui = InventoryGui.instance;
        }

        if (gui != null)
        {
            if (_tabBar != null && _tabBar)
            {
                _tabBar.SetActive(false);
            }

            if (gui.m_containerName != null && gui.m_containerName)
            {
                gui.m_containerName.gameObject.SetActive(true);
            }

            RestoreContainerSize(gui);
        }
    }

    public static void Reset(InventoryGui gui = null)
    {
        ClearExternalContainer(gui);

        if (gui == null)
        {
            gui = InventoryGui.instance;
        }

        if (gui != null && gui.m_currentContainer != null && gui.m_currentContainer.IsBackpackProxy())
        {
            InventoryGuiPatches.BackpackIsOpen = true;
        }
        else
        {
            InventoryGuiPatches.BackpackIsOpen = false;
        }

        BackpackKeyHints.HideHudHints();
    }

    private static void UpdateTabVisuals()
    {
        if (_chestTabButton == null || !_chestTabButton || _backpackTabButton == null || !_backpackTabButton)
            return;

        if (_isViewingBackpack)
        {
            _chestTabButton.interactable = true;
            _backpackTabButton.interactable = false;
        }
        else
        {
            _chestTabButton.interactable = false;
            _backpackTabButton.interactable = true;
        }

        Color goldColor = new Color(1f, 0.85f, 0.35f, 1f);
        SetTabButtonColor(_chestTabButton, goldColor);
        SetTabButtonColor(_backpackTabButton, goldColor);
    }

    private static void SetTabButtonText(Button button, string text)
    {
        if (button == null || !button)
            return;

        TMP_Text[] labels = button.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < labels.Length; i++)
        {
            labels[i].text = text;
            labels[i].color = new Color(1f, 0.85f, 0.35f, 1f);
        }
    }

    private static void SetTabButtonColor(Button button, Color color)
    {
        if (button == null || !button)
            return;

        TMP_Text[] labels = button.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < labels.Length; i++)
        {
            labels[i].color = color;
        }
    }

    private static void EnsureContainerStretched(InventoryGui gui)
    {
        if (gui == null || !gui || gui.m_container == null || !gui.m_container || _isContainerStretched)
            return;

        RectTransform containerRect = gui.m_container;
        _originalContainerSizeDelta = containerRect.sizeDelta;
        _originalContainerAnchoredPos = containerRect.anchoredPosition;

        _originalGridDirectChild = null;
        if (gui.m_containerGrid != null && gui.m_containerGrid)
        {
            RectTransform current = gui.m_containerGrid.transform as RectTransform;
            while (current != null && current.parent != null && current.parent != containerRect)
            {
                current = current.parent as RectTransform;
            }
            if (current != null && current.parent == containerRect)
            {
                _originalGridDirectChild = current;
                _originalGridSizeDelta = current.sizeDelta;
                _originalGridAnchoredPos = current.anchoredPosition;
            }
        }

        containerRect.sizeDelta = new Vector2(_originalContainerSizeDelta.x, _originalContainerSizeDelta.y + ContainerExtraHeight);
        containerRect.anchoredPosition = new Vector2(_originalContainerAnchoredPos.x, _originalContainerAnchoredPos.y - (ContainerExtraHeight * (1f - containerRect.pivot.y)));

        if (_originalGridDirectChild != null)
        {
            float anchorHeightSpan = _originalGridDirectChild.anchorMax.y - _originalGridDirectChild.anchorMin.y;
            _originalGridDirectChild.sizeDelta = new Vector2(_originalGridSizeDelta.x, _originalGridSizeDelta.y - (anchorHeightSpan * ContainerExtraHeight));
            _originalGridDirectChild.anchoredPosition = new Vector2(_originalGridAnchoredPos.x, _originalGridAnchoredPos.y - (ContainerExtraHeight * _originalGridDirectChild.anchorMin.y));
        }

        _isContainerStretched = true;
    }

    private static void RestoreContainerSize(InventoryGui gui)
    {
        if (!_isContainerStretched)
            return;

        if (gui == null)
        {
            gui = InventoryGui.instance;
        }

        if (gui != null && gui && gui.m_container != null && gui.m_container)
        {
            RectTransform containerRect = gui.m_container;
            containerRect.sizeDelta = _originalContainerSizeDelta;
            containerRect.anchoredPosition = _originalContainerAnchoredPos;

            if (_originalGridDirectChild != null)
            {
                _originalGridDirectChild.sizeDelta = _originalGridSizeDelta;
                _originalGridDirectChild.anchoredPosition = _originalGridAnchoredPos;
                _originalGridDirectChild = null;
            }

            _isContainerStretched = false;
        }
    }
}
