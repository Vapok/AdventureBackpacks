using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using AdventureBackpacks.Components;
using AdventureBackpacks.Extensions;
using AdventureBackpacks.Features;
using UnityEngine;

namespace AdventureBackpacks.Compats;

public class BackpackContainerRealProxy : RealProxy
{
    public BackpackContainerRealProxy(Type targetInterface) : base(targetInterface)
    {
    }

    public override IMessage Invoke(IMessage msg)
    {
        if (msg is not IMethodCallMessage call)
            return null;

        try
        {
            object result = null;
            var outArgs = new object[call.ArgCount];

            var player = Player.m_localPlayer;
            Inventory backpackInventory = null;
            var hasBackpack = player != null && CraftFromBackpack.CanCraftFromBackpack(player, out backpackInventory) && backpackInventory != null;

            switch (call.MethodName)
            {
                case "ItemCount":
                    if (hasBackpack && call.Args.Length > 0 && call.Args[0] is string itemName)
                    {
                        result = backpackInventory.CountItems(itemName, -1, true);
                    }
                    else
                    {
                        result = 0;
                    }
                    break;

                case "ContainsItem":
                    if (hasBackpack && call.Args.Length >= 3 && call.Args[0] is string reqName)
                    {
                        var quality = call.Args[1] is int q ? q : -1;
                        var count = backpackInventory.CountItems(reqName, quality, true);
                        outArgs[2] = count;
                        result = count > 0;
                    }
                    else
                    {
                        if (call.ArgCount >= 3)
                            outArgs[2] = 0;
                        result = false;
                    }
                    break;

                case "ProcessContainerInventory":
                    if (hasBackpack && call.Args.Length >= 3 && call.Args[0] is string matName)
                    {
                        var totalAmount = call.Args[1] is int cur ? cur : 0;
                        var totalRequirement = call.Args[2] is int needed ? needed : 0;
                        var shortfall = totalRequirement - totalAmount;

                        if (shortfall > 0)
                        {
                            var count = Mathf.Min(backpackInventory.CountItems(matName, -1, true), shortfall);
                            if (count > 0)
                            {
                                var allBpItems = backpackInventory.GetAllItems();
                                if (allBpItems != null)
                                {
                                    var matchingBpItems = allBpItems.Where(x => 
                                        x != null && 
                                        x.m_shared != null && 
                                        string.Equals(x.m_shared.m_name, matName)).ToList();

                                    var remainingToRemove = count;
                                    foreach (var item in matchingBpItems)
                                    {
                                        if (remainingToRemove <= 0)
                                            break;

                                        var toRemove = Mathf.Min(item.m_stack, remainingToRemove);
                                        backpackInventory.RemoveItem(item, toRemove);
                                        remainingToRemove -= toRemove;
                                    }
                                }

                                var bp = player.GetEquippedBackpack();
                                bp?.Save();
                                backpackInventory.Changed();
                                totalAmount += count;
                            }
                        }
                        result = totalAmount;
                    }
                    else
                    {
                        result = call.Args.Length > 1 ? call.Args[1] : 0;
                    }
                    break;

                case "GetInventory":
                    result = hasBackpack ? backpackInventory : null;
                    break;

                case "GetPosition":
                    result = player != null ? player.transform.position : Vector3.zero;
                    break;

                case "GetPrefabName":
                    result = "Backpack";
                    break;

                case "Save":
                    if (player != null)
                    {
                        var bp = player.GetEquippedBackpack();
                        bp?.Save();
                    }
                    result = null;
                    break;

                case "RemoveItem":
                    if (hasBackpack && call.Args.Length >= 2 && call.Args[0] is string remName && call.Args[1] is int remAmt)
                    {
                        var allBpItems = backpackInventory.GetAllItems();
                        if (allBpItems != null)
                        {
                            var matchingBpItems = allBpItems.Where(x => 
                                x != null && 
                                x.m_shared != null && 
                                string.Equals(x.m_shared.m_name, remName)).ToList();

                            var remainingToRemove = remAmt;
                            foreach (var item in matchingBpItems)
                            {
                                if (remainingToRemove <= 0)
                                    break;

                                var toRemove = Mathf.Min(item.m_stack, remainingToRemove);
                                backpackInventory.RemoveItem(item, toRemove);
                                remainingToRemove -= toRemove;
                            }
                        }
                        var bp = player.GetEquippedBackpack();
                        bp?.Save();
                        backpackInventory.Changed();
                    }
                    result = null;
                    break;

                case "Equals":
                    if (call.Args.Length > 0 && call.Args[0] != null)
                    {
                        var target = call.Args[0];
                        if (ReferenceEquals(GetTransparentProxy(), target))
                        {
                            result = true;
                        }
                        else if (RemotingServices.IsTransparentProxy(target))
                        {
                            result = ReferenceEquals(this, RemotingServices.GetRealProxy(target));
                        }
                        else
                        {
                            result = false;
                        }
                    }
                    else
                    {
                        result = false;
                    }
                    break;

                case "GetHashCode":
                    result = GetHashCode();
                    break;

                case "ToString":
                    result = "BackpackContainerProxy";
                    break;

                default:
                    if (call.MethodBase is MethodInfo mi && mi.ReturnType != typeof(void))
                    {
                        result = mi.ReturnType.IsValueType ? Activator.CreateInstance(mi.ReturnType) : null;
                    }
                    break;
            }

            return new ReturnMessage(result, outArgs, outArgs.Length, call.LogicalCallContext, call);
        }
        catch (Exception ex)
        {
            return new ReturnMessage(ex, call);
        }
    }
}
