using Archipelago.Archipelago;
using HarmonyLib;
using PerfectRandom.Sulfur.Core.UI.Inventory;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(TeliaPrintStationListItem), nameof(TeliaPrintStationListItem.PurchasePrice))]
[HarmonyPatch(MethodType.Getter)]
public class PurchasePrice
{
    public static bool Prefix(ref int __result, TeliaPrintStationListItem __instance)
    {
        if (!Plugin.Client.State.PrintableWeaponIds.Contains(ArchipelagoItems.Get(__instance.Item.id).Id)) return true;
        __result = 0;
        return false;

    }
}