using System.Linq;
using Archipelago.Archipelago;
using HarmonyLib;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.Units;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(ShopKeeper), nameof(ShopKeeper.SetupItems))]
public class SetupItems
{
    public static void Postfix(ShopKeeper __instance)
    {
        if (__instance.Npc.UnitIdentifier is not "CongregationScholar") return;
        var existingItems = __instance.serviceGrid.AllItems().Select(item => item.itemDefinition.id).ToHashSet();
        foreach (var scroll in Plugin.Client.State.ScrollUnlocks)
        {
            var itemId = ArchipelagoItems.Get(scroll).ItemId();
            if (existingItems.Contains(itemId)) continue;
            __instance.serviceGrid.AddItem(itemId.GetAsset(), false, false);
            existingItems.Add(itemId);
        }
    }
}