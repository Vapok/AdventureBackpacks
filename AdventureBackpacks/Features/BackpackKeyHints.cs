using System;
using System.Collections.Generic;
using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using AdventureBackpacks.Patches;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdventureBackpacks.Features;

public static class BackpackKeyHints
{
    private static GameObject _invHintKB;
    private static TMP_Text _invHintLabelKB;
    private static TMP_Text _invHintKeyKB;

    private static GameObject _invDropHintKB;
    private static TMP_Text _invDropHintLabelKB;
    private static TMP_Text _invDropHintKeyKB;

    private static GameObject _containerTabHintKB;
    private static TMP_Text _containerTabHintLabelKB;
    private static TMP_Text _containerTabHintKeyKB;

    private static GameObject _containerTabHintGP;
    private static TMP_Text _containerTabHintLabelGP;
    private static TMP_Text _containerTabHintKeyGP;

    private static GameObject _containerBpHintKB;
    private static TMP_Text _containerBpHintLabelKB;
    private static TMP_Text _containerBpHintKeyKB;

    private static GameObject _containerDropHintKB;
    private static TMP_Text _containerDropHintLabelKB;
    private static TMP_Text _containerDropHintKeyKB;

    private static Transform ResolveTransform(Transform root, string childName, UIInputHint inputHint, bool isGamepad)
    {
        if (root == null)
            return null;

        Transform target = root.Find(childName);
        if (target != null)
            return target;

        if (inputHint != null)
        {
            if (!isGamepad && inputHint.m_mouseKeyboardHint != null)
                return inputHint.m_mouseKeyboardHint.transform;
            if (isGamepad && inputHint.m_gamepadHint != null)
                return inputHint.m_gamepadHint.transform;
        }

        return null;
    }

    private static GameObject SetupHintRow(Transform parent, string hintName, ref TMP_Text labelOut, ref TMP_Text keyOut)
    {
        if (parent == null)
            return null;

        Transform existing = parent.Find(hintName);
        GameObject hintObj = existing != null ? existing.gameObject : null;

        if (hintObj == null)
        {
            Transform template = parent.Find("PrimaryAttack");
            if (template == null && parent.childCount > 0)
            {
                template = parent.GetChild(0);
            }

            if (template == null)
                return null;

            hintObj = UnityEngine.Object.Instantiate(template.gameObject, parent, false);
            hintObj.name = hintName;
            hintObj.transform.SetAsLastSibling();
        }

        if (hintObj != null)
        {
            Transform labelTransform = hintObj.transform.Find("Text");
            if (labelTransform != null)
            {
                labelOut = labelTransform.GetComponent<TMP_Text>();
                Localize loc = labelTransform.GetComponent<Localize>();
                if (loc != null)
                {
                    UnityEngine.Object.Destroy(loc);
                }
            }

            Transform keyTransform = hintObj.transform.Find("key_bkg/Key") ?? hintObj.transform.Find("Key");
            if (keyTransform != null)
            {
                keyOut = keyTransform.GetComponent<TMP_Text>();
                Localize keyLoc = keyTransform.GetComponent<Localize>();
                if (keyLoc != null)
                {
                    UnityEngine.Object.Destroy(keyLoc);
                }
            }
            else
            {
                TMP_Text[] texts = hintObj.GetComponentsInChildren<TMP_Text>(true);
                for (int i = 0; i < texts.Length; i++)
                {
                    TMP_Text t = texts[i];
                    if (t != labelOut)
                    {
                        keyOut = t;
                        Localize keyLoc = t.GetComponent<Localize>();
                        if (keyLoc != null)
                        {
                            UnityEngine.Object.Destroy(keyLoc);
                        }
                        break;
                    }
                }
            }
        }

        return hintObj;
    }

    public static void HideHudHints()
    {
        if (_invHintKB != null && _invHintKB) _invHintKB.SetActive(false);
        if (_invDropHintKB != null && _invDropHintKB) _invDropHintKB.SetActive(false);
        if (_containerTabHintKB != null && _containerTabHintKB) _containerTabHintKB.SetActive(false);
        if (_containerTabHintGP != null && _containerTabHintGP) _containerTabHintGP.SetActive(false);
        if (_containerBpHintKB != null && _containerBpHintKB) _containerBpHintKB.SetActive(false);
        if (_containerDropHintKB != null && _containerDropHintKB) _containerDropHintKB.SetActive(false);
    }

    public static void HideAll()
    {
        HideHudHints();
        CleanupInventoryGuiHint();
    }

    private static void CleanupInventoryGuiHint()
    {
        if (InventoryGui.instance != null && InventoryGui.instance.m_player != null)
        {
            Transform existing = InventoryGui.instance.m_player.Find("AB_InventoryKeyHint");
            if (existing != null && existing.gameObject)
            {
                UnityEngine.Object.Destroy(existing.gameObject);
            }
        }
    }

    public static void UpdateHints(KeyHints keyHints)
    {
        CleanupInventoryGuiHint();

        if (keyHints == null || !InventoryGui.IsVisible())
        {
            HideHudHints();
            return;
        }

        Player localPlayer = Player.m_localPlayer;
        if (localPlayer == null || !localPlayer.IsBackpackEquipped())
        {
            HideHudHints();
            return;
        }

        bool containerOpen = InventoryGui.instance != null && InventoryGui.instance.IsContainerOpen();
        bool isBackpackOpen = InventoryGuiPatches.BackpackIsOpen || (InventoryGui.instance != null && InventoryGui.instance.m_currentContainer != null && InventoryGui.instance.m_currentContainer.IsBackpackProxy());
        string openKeyStr = ConfigRegistry.HotKeyOpen.Value.MainKey.ToString();
        string dropKeyStr = ConfigRegistry.HotKeyDrop.Value.MainKey.ToString();

        if (!containerOpen)
        {
            if (keyHints.m_inventoryHints != null && keyHints.m_inventoryHints.activeSelf)
            {
                Transform hintsTransform = keyHints.m_inventoryHints.transform;
                UIInputHint inputHint = keyHints.m_inventoryHints.GetComponent<UIInputHint>();
                Transform kbTransform = ResolveTransform(hintsTransform, "Keyboard", inputHint, false);

                if (kbTransform != null)
                {
                    _invHintKB = SetupHintRow(kbTransform, "AB_InventoryBackpackHintKB", ref _invHintLabelKB, ref _invHintKeyKB);
                    if (_invHintKB != null)
                    {
                        string bpHintKey = isBackpackOpen ? "$vapok_mod_close_backpack" : "$vapok_mod_backpack";
                        string bpHintText = Localization.instance != null
                            ? Localization.instance.Localize(bpHintKey)
                            : (isBackpackOpen ? "Close Backpack" : "Backpack");
                        if (_invHintKeyKB != null) _invHintKeyKB.text = openKeyStr;
                        if (_invHintLabelKB != null) _invHintLabelKB.text = bpHintText;
                        _invHintKB.SetActive(true);
                    }

                    if (ConfigRegistry.OutwardMode.Value)
                    {
                        _invDropHintKB = SetupHintRow(kbTransform, "AB_InventoryQuickDropHintKB", ref _invDropHintLabelKB, ref _invDropHintKeyKB);
                        if (_invDropHintKB != null)
                        {
                            string dropHintText = Localization.instance != null
                                ? Localization.instance.Localize("$vapok_mod_drop_backpack")
                                : "Drop Backpack";
                            if (_invDropHintKeyKB != null) _invDropHintKeyKB.text = dropKeyStr;
                            if (_invDropHintLabelKB != null) _invDropHintLabelKB.text = dropHintText;
                            _invDropHintKB.SetActive(true);
                        }
                    }
                    else if (_invDropHintKB != null && _invDropHintKB)
                    {
                        _invDropHintKB.SetActive(false);
                    }

                    if (kbTransform is RectTransform kbRt)
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(kbRt);
                    }
                }
            }

            if (_containerTabHintKB != null && _containerTabHintKB) _containerTabHintKB.SetActive(false);
            if (_containerTabHintGP != null && _containerTabHintGP) _containerTabHintGP.SetActive(false);
            if (_containerBpHintKB != null && _containerBpHintKB) _containerBpHintKB.SetActive(false);
            if (_containerDropHintKB != null && _containerDropHintKB) _containerDropHintKB.SetActive(false);
        }
        else
        {
            if (keyHints.m_inventoryWithContainerHints != null && keyHints.m_inventoryWithContainerHints.activeSelf)
            {
                Transform hintsTransform = keyHints.m_inventoryWithContainerHints.transform;
                UIInputHint inputHint = keyHints.m_inventoryWithContainerHints.GetComponent<UIInputHint>();
                Transform kbTransform = ResolveTransform(hintsTransform, "Keyboard", inputHint, false);
                Transform gpTransform = ResolveTransform(hintsTransform, "Gamepad", inputHint, true);

                bool hasTabs = ContainerTabs.HasActiveExternalContainer && ConfigRegistry.EnableContainerTabs.Value;

                if (kbTransform != null)
                {
                    if (hasTabs)
                    {
                        _containerTabHintKB = SetupHintRow(kbTransform, "AB_ContainerTabHintKB", ref _containerTabHintLabelKB, ref _containerTabHintKeyKB);
                        if (_containerTabHintKB != null)
                        {
                            string switchText = Localization.instance != null
                                ? Localization.instance.Localize("$vapok_mod_switch_container")
                                : "Switch Container";
                            if (_containerTabHintKeyKB != null) _containerTabHintKeyKB.text = "< / >";
                            if (_containerTabHintLabelKB != null) _containerTabHintLabelKB.text = switchText;
                            _containerTabHintKB.SetActive(true);
                        }
                    }
                    else if (_containerTabHintKB != null && _containerTabHintKB)
                    {
                        _containerTabHintKB.SetActive(false);
                    }

                    if (!hasTabs)
                    {
                        _containerBpHintKB = SetupHintRow(kbTransform, "AB_ContainerBpHintKB", ref _containerBpHintLabelKB, ref _containerBpHintKeyKB);
                        if (_containerBpHintKB != null)
                        {
                            string bpContainerHintKey = isBackpackOpen ? "$vapok_mod_close_backpack" : "$vapok_mod_backpack";
                            string bpContainerHintText = Localization.instance != null
                                ? Localization.instance.Localize(bpContainerHintKey)
                                : (isBackpackOpen ? "Close Backpack" : "Backpack");
                            if (_containerBpHintKeyKB != null) _containerBpHintKeyKB.text = openKeyStr;
                            if (_containerBpHintLabelKB != null) _containerBpHintLabelKB.text = bpContainerHintText;
                            _containerBpHintKB.SetActive(true);
                        }
                    }
                    else if (_containerBpHintKB != null && _containerBpHintKB)
                    {
                        _containerBpHintKB.SetActive(false);
                    }

                    if (ConfigRegistry.OutwardMode.Value)
                    {
                        _containerDropHintKB = SetupHintRow(kbTransform, "AB_ContainerQuickDropHintKB", ref _containerDropHintLabelKB, ref _containerDropHintKeyKB);
                        if (_containerDropHintKB != null)
                        {
                            string dropContainerHintText = Localization.instance != null
                                ? Localization.instance.Localize("$vapok_mod_drop_backpack")
                                : "Drop Backpack";
                            if (_containerDropHintKeyKB != null) _containerDropHintKeyKB.text = dropKeyStr;
                            if (_containerDropHintLabelKB != null) _containerDropHintLabelKB.text = dropContainerHintText;
                            _containerDropHintKB.SetActive(true);
                        }
                    }
                    else if (_containerDropHintKB != null && _containerDropHintKB)
                    {
                        _containerDropHintKB.SetActive(false);
                    }

                    if (kbTransform is RectTransform kbRt)
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(kbRt);
                    }
                }

                if (gpTransform != null)
                {
                    if (hasTabs)
                    {
                        _containerTabHintGP = SetupHintRow(gpTransform, "AB_ContainerTabHintGP", ref _containerTabHintLabelGP, ref _containerTabHintKeyGP);
                        if (_containerTabHintGP != null)
                        {
                            string switchTextGp = Localization.instance != null
                                ? Localization.instance.Localize("$vapok_mod_switch_container")
                                : "Switch Container";
                            if (_containerTabHintKeyGP != null) _containerTabHintKeyGP.text = "R3";
                            if (_containerTabHintLabelGP != null) _containerTabHintLabelGP.text = switchTextGp;
                            _containerTabHintGP.SetActive(true);
                        }
                    }
                    else if (_containerTabHintGP != null && _containerTabHintGP)
                    {
                        _containerTabHintGP.SetActive(false);
                    }

                    if (gpTransform is RectTransform gpRt)
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(gpRt);
                    }
                }
            }

            if (_invHintKB != null && _invHintKB) _invHintKB.SetActive(false);
            if (_invDropHintKB != null && _invDropHintKB) _invDropHintKB.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(KeyHints), "UpdateHints")]
    static class KeyHintsUpdateHintsPatch
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        [HarmonyPostfix]
        private static void Postfix(KeyHints __instance)
        {
            UpdateHints(__instance);
        }
    }
}
