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
        if (gui == null || gui.m_container == null || !gui.m_container)
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
        tabBarRect.pivot = new Vector2(0.5f, 0f);
        tabBarRect.sizeDelta = new Vector2(280f, 34f);
        tabBarRect.anchoredPosition = new Vector2(0f, 0f);
    }

    private static void EnsureTabBar(InventoryGui gui)
    {
        if (gui == null || gui.m_container == null || !gui.m_container)
            return;

        if (_tabBar != null && _tabBar)
        {
            if (_tabBar.transform.parent != gui.m_container)
            {
                _tabBar.transform.SetParent(gui.m_container, false);
            }
            PositionTabBar(gui);

            if (_chestTabButton != null && _chestTabButton)
            {
                _chestTabButton.onClick.RemoveAllListeners();
                _chestTabButton.onClick.AddListener(() => SwitchToChest(gui, Player.m_localPlayer));
            }
            if (_backpackTabButton != null && _backpackTabButton)
            {
                _backpackTabButton.onClick.RemoveAllListeners();
                _backpackTabButton.onClick.AddListener(() => SwitchToBackpack(gui, Player.m_localPlayer));
            }
            return;
        }

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
        SanitizeTabButton(chestButton);
        chestButton.onClick.RemoveAllListeners();
        chestButton.onClick.AddListener(() => SwitchToChest(gui, Player.m_localPlayer));

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

        TMP_Text chestLabel = GetPrimaryLabel(chestButton);
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
        SanitizeTabButton(backpackButton);
        backpackButton.onClick.RemoveAllListeners();
        backpackButton.onClick.AddListener(() => SwitchToBackpack(gui, Player.m_localPlayer));

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

        TMP_Text bpLabel = GetPrimaryLabel(backpackButton);
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

    private static void SanitizeTabButton(Button button)
    {
        if (button == null || !button)
            return;

        button.navigation = new Navigation { mode = Navigation.Mode.None };
        button.onClick = new Button.ButtonClickedEvent();

        UITooltip tooltip = button.GetComponent<UITooltip>();
        if (tooltip != null)
        {
            UnityEngine.Object.Destroy(tooltip);
        }

        Localize[] localizers = button.GetComponentsInChildren<Localize>(true);
        for (int i = 0; i < localizers.Length; i++)
        {
            UnityEngine.Object.Destroy(localizers[i]);
        }

        UIGamePad[] gpHandlers = button.GetComponentsInChildren<UIGamePad>(true);
        for (int i = 0; i < gpHandlers.Length; i++)
        {
            if (gpHandlers[i].m_hint != null)
            {
                UnityEngine.Object.Destroy(gpHandlers[i].m_hint);
            }
            UnityEngine.Object.Destroy(gpHandlers[i]);
        }

        UIInputHint[] inputHints = button.GetComponentsInChildren<UIInputHint>(true);
        for (int i = 0; i < inputHints.Length; i++)
        {
            if (inputHints[i].m_gamepadHint != null)
            {
                UnityEngine.Object.Destroy(inputHints[i].m_gamepadHint);
            }
            if (inputHints[i].m_mouseKeyboardHint != null)
            {
                UnityEngine.Object.Destroy(inputHints[i].m_mouseKeyboardHint);
            }
            if (inputHints[i].m_gamepadMouseHint != null)
            {
                UnityEngine.Object.Destroy(inputHints[i].m_gamepadMouseHint);
            }
            UnityEngine.Object.Destroy(inputHints[i]);
        }

        UIInputHandler[] inputHandlers = button.GetComponentsInChildren<UIInputHandler>(true);
        for (int i = 0; i < inputHandlers.Length; i++)
        {
            UnityEngine.Object.Destroy(inputHandlers[i]);
        }

        for (int i = button.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = button.transform.GetChild(i);
            string childName = child.name.ToLower();
            if (childName.Contains("hint") || childName.Contains("key") || childName.Contains("gamepad") || childName.Contains("binding"))
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }
    }

    public static void SwitchToBackpack(InventoryGui gui, Player player)
    {
        if (gui == null || player == null || !player.CanOpenBackpack())
            return;

        if (gui.m_splitDialog != null && gui.m_splitDialog.gameObject.activeInHierarchy)
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
        if (gui.m_containerGrid != null)
        {
            gui.m_containerGrid.ResetView();
            gui.m_containerGrid.UpdateInventory(backpackContainer.GetInventory(), null, gui.m_dragItem);
        }
        if (gui.m_containerName != null)
        {
            gui.m_containerName.text = Localization.instance.Localize(backpackContainer.GetInventory().GetName());
        }

        if (ZInput.IsGamepadActive() && gui.m_containerGrid != null)
        {
            gui.m_containerGrid.SetGamepadSelection(new Vector2i(0, 0));
        }

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

        if (gui.m_splitDialog != null && gui.m_splitDialog.gameObject.activeInHierarchy)
            return;

        if (gui.m_dragItem != null)
        {
            gui.SetupDragItem(null, null, 1);
        }

        _isViewingBackpack = false;
        InventoryGuiPatches.BackpackIsOpen = false;

        gui.m_currentContainer = _cachedExternalContainer;
        gui.m_firstContainerUpdate = true;
        if (gui.m_containerGrid != null)
        {
            gui.m_containerGrid.ResetView();
            gui.m_containerGrid.UpdateInventory(_cachedExternalContainer.GetInventory(), null, gui.m_dragItem);
        }
        if (gui.m_containerName != null)
        {
            gui.m_containerName.text = Localization.instance.Localize(_cachedExternalContainer.GetInventory().GetName());
        }

        if (ZInput.IsGamepadActive() && gui.m_containerGrid != null)
        {
            gui.m_containerGrid.SetGamepadSelection(new Vector2i(0, 0));
        }

        PositionTabBar(gui);
        UpdateTabVisuals();
    }

    public static void ToggleTab(InventoryGui gui, Player player)
    {
        if (gui == null || player == null)
            return;

        if (gui.m_splitDialog != null && gui.m_splitDialog.gameObject.activeInHierarchy)
            return;

        if (gui.m_dragItem != null || gui.m_dragGo != null)
            return;

        if (_isViewingBackpack)
        {
            SwitchToChest(gui, player);
        }
        else
        {
            SwitchToBackpack(gui, player);
        }
    }

    public static bool IsGamepadButtonTriggered(string buttonConfig)
    {
        if (string.IsNullOrEmpty(buttonConfig) || buttonConfig.Equals("None", StringComparison.OrdinalIgnoreCase))
            return false;

        if (buttonConfig.Contains("+"))
        {
            string[] parts = buttonConfig.Split('+');
            if (parts.Length < 2)
                return false;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                string mod = parts[i].Trim();
                if (mod.Equals("JoyLTrigger", StringComparison.OrdinalIgnoreCase) || mod.Equals("JoyAltKeys", StringComparison.OrdinalIgnoreCase))
                {
                    if (!ZInput.GetButton("JoyLTrigger") && !ZInput.GetButton("JoyAltKeys"))
                        return false;
                }
                else if (!ZInput.GetButton(mod))
                {
                    return false;
                }
            }

            string mainBtn = parts[parts.Length - 1].Trim();
            return ZInput.GetButtonDown(mainBtn);
        }

        if (ZInput.GetButton("JoyLTrigger") || ZInput.GetButton("JoyAltKeys") || ZInput.GetButton("JoyRTrigger") ||
            Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ||
            Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            return false;
        }

        return ZInput.GetButtonDown(buttonConfig.Trim());
    }

    public static void ResetGamepadButtonStatus(string buttonConfig)
    {
        if (string.IsNullOrEmpty(buttonConfig) || buttonConfig.Equals("None", StringComparison.OrdinalIgnoreCase))
            return;

        if (buttonConfig.Contains("+"))
        {
            string[] parts = buttonConfig.Split('+');
            string mainBtn = parts[parts.Length - 1].Trim();
            ZInput.ResetButtonStatus(mainBtn);
        }
        else
        {
            ZInput.ResetButtonStatus(buttonConfig.Trim());
        }
    }

    public static bool IsContainerGroupActive(InventoryGui gui)
    {
        if (gui == null)
            return false;

        if (gui.ActiveGroup == 0)
            return true;

        if (gui.m_containerGrid != null && gui.m_containerGrid.m_uiGroup != null && gui.m_containerGrid.m_uiGroup.IsActive)
            return true;

        return false;
    }

    public static void CheckGamepadInput(Player player)
    {
        if (player == null || !ConfigRegistry.EnableContainerTabs.Value || !HasActiveExternalContainer)
            return;

        InventoryGui gui = InventoryGui.instance;
        if (gui == null || !InventoryGui.IsVisible())
            return;

        if (!player.CanOpenBackpack())
            return;

        if (InventoryGuiPatches.CheckForTextInput())
            return;

        if (gui.m_splitDialog != null && gui.m_splitDialog.gameObject.activeInHierarchy)
            return;

        if (gui.m_dragItem != null || gui.m_dragGo != null)
            return;

        if (ZInput.IsGamepadActive())
        {
            if (IsContainerGroupActive(gui))
            {
                string button = ConfigRegistry.ContainerTabsGamepadButton != null ? ConfigRegistry.ContainerTabsGamepadButton.Value : "JoyRTrigger + JoyRStick";
                if (IsGamepadButtonTriggered(button))
                {
                    ToggleTab(gui, player);
                    ResetGamepadButtonStatus(button);
                }
            }
        }
    }

    public static void Update(InventoryGui gui, Player player)
    {
        if (gui == null || player == null || !ConfigRegistry.EnableContainerTabs.Value)
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

        if (InventoryGuiPatches.CheckForTextInput())
            return;

        if (gui.m_splitDialog != null && gui.m_splitDialog.gameObject.activeInHierarchy)
            return;

        if (gui.m_dragItem != null || gui.m_dragGo != null)
            return;

        if (ZInput.GetButton("JoyLTrigger") || ZInput.GetButton("JoyAltKeys") || ZInput.GetButton("JoyRTrigger") ||
            Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ||
            Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            return;

        if (ZInput.IsGamepadActive())
        {
            CheckGamepadInput(player);
        }
        else
        {
            if (ZInput.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.LeftArrow) ||
                ZInput.GetKeyDown(KeyCode.Comma) || Input.GetKeyDown(KeyCode.Comma) ||
                Input.GetKeyDown(KeyCode.Less))
            {
                if (_isViewingBackpack)
                {
                    SwitchToChest(gui, player);
                }
            }
            else if (ZInput.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
                     ZInput.GetKeyDown(KeyCode.Period) || Input.GetKeyDown(KeyCode.Period) ||
                     Input.GetKeyDown(KeyCode.Greater))
            {
                if (!_isViewingBackpack)
                {
                    SwitchToBackpack(gui, player);
                }
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

    public static void OnConfigDisabled()
    {
        InventoryGui gui = InventoryGui.instance;
        if (gui != null)
        {
            if (_isViewingBackpack && Player.m_localPlayer != null)
            {
                SwitchToChest(gui, Player.m_localPlayer);
            }

            if (_tabBar != null && _tabBar)
            {
                _tabBar.SetActive(false);
            }

            Reset(gui);
        }
    }

    public static void OnConfigEnabled()
    {
        InventoryGui gui = InventoryGui.instance;
        Player localPlayer = Player.m_localPlayer;
        if (gui != null && gui.IsContainerOpen() && gui.m_currentContainer != null && !gui.m_currentContainer.IsBackpackProxy() && localPlayer != null && localPlayer.CanOpenBackpack())
        {
            InitializeWithContainer(gui.m_currentContainer, gui, localPlayer);
        }
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

    private static TMP_Text GetPrimaryLabel(Button button)
    {
        if (button == null || !button)
            return null;

        Transform textChild = button.transform.Find("Text");
        if (textChild != null)
        {
            TMP_Text tmp = textChild.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.raycastTarget = false;
                return tmp;
            }
        }

        TMP_Text childTmp = button.GetComponentInChildren<TMP_Text>();
        if (childTmp != null)
        {
            childTmp.raycastTarget = false;
            return childTmp;
        }

        return null;
    }

    private static void SetTabButtonText(Button button, string text)
    {
        if (button == null || !button)
            return;

        TMP_Text label = GetPrimaryLabel(button);
        if (label != null)
        {
            label.text = text;
            label.color = new Color(1f, 0.85f, 0.35f, 1f);
        }
        else
        {
            Text legacyLabel = button.GetComponentInChildren<Text>();
            if (legacyLabel != null)
            {
                legacyLabel.text = text;
                legacyLabel.color = new Color(1f, 0.85f, 0.35f, 1f);
            }
        }
    }

    private static void SetTabButtonColor(Button button, Color color)
    {
        if (button == null || !button)
            return;

        TMP_Text label = GetPrimaryLabel(button);
        if (label != null)
        {
            label.color = color;
        }
        else
        {
            Text legacyLabel = button.GetComponentInChildren<Text>();
            if (legacyLabel != null)
            {
                legacyLabel.color = color;
            }
        }
    }
}
