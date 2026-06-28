using Archipelago.Archipelago;
using HarmonyLib;
using PerfectRandom.Sulfur.Core.Items;
using PerfectRandom.Sulfur.Core.World;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(StampPurchaseStation), nameof(StampPurchaseStation.IsPurchased))]
public class IsPurchased
{
    public static bool Prefix(ItemDefinition item, ref bool __result)
    {
        if (item.isEndlessUpgrade)
            return true;
        var itemIdString = ArchipelagoItems.Get(item.id).Name;
        var locationName = $"Trade stamps for the {itemIdString}";
        __result = Plugin.Client.State.AllCompletedLocations.Contains(Plugin.Client.Session.Locations.GetLocationIdFromName("SULFUR", locationName));
        __result = false;
        return false;
    }
}