using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.Items;
using PerfectRandom.Sulfur.Core.Stats;
using PerfectRandom.Sulfur.Core.Units;
using UnityEngine;

namespace Archipelago.Patches;

//[HarmonyPatch(typeof(LootManager), nameof(LootManager.SelectItemsFromTable))]
public class SelectOneItem
{
    public static void Postfix(LootTable lootTable, List<ItemDefinition> __result, LootManager __instance)
    {
        if (lootTable is null)
        {
            Plugin.Logger.LogInfo("No table");
            return;
        }

        if (__result == null || __result.Count == 0)
            Plugin.Logger.LogInfo("Result was empty");
        Plugin.Logger.LogInfo("Selecting from table:" + lootTable.name);
        Plugin.Logger.LogInfo("With items: " + string.Join(", ",
            lootTable.entries.Select(entry => $"{entry.lootItem}: {entry.lootWeight}")));
        Plugin.Logger.LogInfo("Selected: " + string.Join(", ", __result.Select(item => item.name)));
    }
}

//[HarmonyPatch(typeof(Unit), nameof(Unit.SpawnLoot))]
public class UnitStuff
{
    public static void Prefix(Unit __instance)
    {
        GlobalSettings.VerboseLoggingLoot = true;
        Plugin.Logger.LogInfo("Checking to spawn?" +
                              $"{__instance.IsCivilian}, {__instance.unitSO.shouldDropLoot}, {__instance.hasDroppedLoot}");
        if (!__instance.IsCivilian && __instance.unitSO.shouldDropLoot && !__instance.hasDroppedLoot)
        {
            Plugin.Logger.LogInfo(__instance + " trying to spawn loot");
        }
    }
}

[HarmonyPatch(typeof(LootManager), nameof(LootManager.GetLootTypeDecision))]
public class TypeDecision
{
    public static void Prefix(LootManager __instance)
    {
        LootSettings settings = GameManager.Instance.Settings.LootSettings; // __instance.lootSettings;
        Plugin.Logger.LogInfo("Found settings " + settings);
        var weights = new List<int>
        {
            settings.weightAmmo,
            settings.weightAttachments,
            settings.weightEnchantments,
            settings.weightEnchantmentsOil,
            settings.weightEquipment,
            settings.weightGadgets,
            settings.weightQuestItems,
            settings.weightScavenge,
            settings.weightValuables,
            settings.weightWeapons
        };
        settings.weightWeapons = 10000;
        Plugin.Logger.LogInfo("Weights: " + string.Join(", ", weights));
    }
}