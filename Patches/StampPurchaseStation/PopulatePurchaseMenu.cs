using System.Collections.Generic;
using System.Linq;
using Archipelago.Archipelago;
using HarmonyLib;
using PerfectRandom.Sulfur.Core.World;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(StampPurchaseStation), nameof(StampPurchaseStation.PopulatePurchaseMenu))]
public class PopulatePurchaseMenu
{
    private static Dictionary<string, StampLocation> _stampLocations = [];
    
    public static void Prefix()
    {
        if (_stampLocations.Count > 0) return;
        var stampLocationIds = Plugin.Client.Session.DataStorage.GetLocationNameGroups()["Stamp trade with Arthur"].Select(name =>
            Plugin.Client.Session.Locations.GetLocationIdFromName("SULFUR", name));
        var scoutedLocations = Plugin.Client.Session.Locations.ScoutLocationsAsync(true, stampLocationIds.ToArray()).Result;
        var scoutedIds = scoutedLocations.Keys.ToList();
        scoutedIds.Sort();
        _stampLocations = scoutedIds.Select(id => scoutedLocations[id])
            .ToDictionary(
                scoutedLocation => scoutedLocation.LocationName,
                scoutedLocation => new StampLocation(scoutedLocation.ItemName,
                    scoutedLocation.IsReceiverRelatedToActivePlayer
                        ? $"Your {scoutedLocation.ItemName}"
                        : $"{scoutedLocation.Player.Name}'s {scoutedLocation.ItemName} belonging in {scoutedLocation.Player.Game}"));
    }

    public static void Postfix(StampPurchaseStation __instance)
    {
        if (_stampLocations.Count > __instance.purchaseListItems.Count)
            Plugin.Logger.LogError($"Too many stamp locations {_stampLocations.Count} compared to actual purchase list {__instance.purchaseListItems.Count}");
        foreach (var listItem in __instance.purchaseListItems)
        {
            if (listItem.Item.isEndlessUpgrade) continue;
            var itemIdString = ArchipelagoItems.Get(listItem.Item.id).Name;
            var locationName = $"Trade stamps for the {itemIdString}";
            var stampLocation = _stampLocations[locationName];
            listItem.label.text = stampLocation.Name;
            listItem.flavorLabel.text = stampLocation.Description;
        }
    }
}