using Archipelago.Archipelago;
using HarmonyLib;
using PerfectRandom.Sulfur.Core.Items;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(InventoryItem), nameof(InventoryItem.OnRankUp))]
public class OnRankUp
{
    public static void Postfix(InventoryItem __instance)
    {
        Plugin.Logger.LogInfo($"Ranked up {__instance.itemDefinition.LocalizedDisplayName}");
        var item = ArchipelagoItems.Get(__instance.itemDefinition.id);
        var locationName = $"Reach Rank {__instance.GetRankLevel()} with {item.Name}";
        Plugin.Client.CompleteLocations(locationName);
    }
}