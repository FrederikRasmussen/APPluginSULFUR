using System;
using System.Linq;
using System.Text;
using Archipelago.Helpers;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using HarmonyLib;
using I2.Loc;
using NodeCanvas.DialogueTrees;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.Items;
using PerfectRandom.Sulfur.Core.Stats;
using Unity.Serialization.Json;
using UnityEngine;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(DialogController), nameof(DialogController.OnMultipleChoiceRequest))]
public class OnMultipleChoiceRequest
{
    
    
    public static void Postfix(MultipleChoiceRequestInfo info, DialogController __instance)
    {
        foreach (ItemId itemId in typeof(ItemIds).GetFields().Select(f => f.GetValue(null)))
        {
            var definition = itemId.GetAsset();
            var builder = new StringBuilder();
            builder.Append(definition.name);
            builder.Append(", ");
            builder.Append(definition.ItemType);
            builder.Append(", ");
            if (definition.ItemType is ItemType.Consumable)
            {
                foreach (var buff in definition.buffsOnConsume)
                {
                    if (buff.attributeId == EntityAttributes.Stat_HealthRegen)
                    {
                        builder.Append(buff.totalValueOverride);
                        builder.Append(", ");
                    }
                }
            }

            builder.Append(definition.basePrice);
            builder.Append(", ");
            builder.Append(definition.itemQuality);
            
            Plugin.Logger.LogInfo(builder);
        }
        
        /*var builder = new StringBuilder();
        builder.Append("\n");
        builder.Append("[\n");
        var lootTables = Resources.FindObjectsOfTypeAll<LootTable>().OrderBy(lootTable => lootTable.name).ToArray();
        foreach (var lootTable in lootTables)
        {
            builder.Append("  {\n");
            builder.Append("    \"Name\": \"" + lootTable.name + "\",\n");
            builder.Append("    \"Entries\":" + "\n");
            builder.Append("    {\n");
            foreach (var entry in lootTable.entries)
            {
                builder.Append("       \"" + $"{entry.lootItem}".Replace("\"", "\\\"") + "\": "+ entry.lootWeight);
                builder.Append(entry.Equals(lootTable.entries[^1]) ? "\n" : ",\n");
            }
            builder.Append("    }\n");
            builder.Append("  }");
            builder.Append(lootTable.Equals(lootTables[^1]) ? "\n" : ",\n");
        }
        builder.Append("]");
        Plugin.Logger.LogInfo(builder);
        
        builder = new StringBuilder();
        builder.Append("\n");
        builder.Append("[\n");
        foreach (var lootTable in lootTables.OrderBy(lootTable => lootTable.name))
        {
            builder.Append("  {\n");
            builder.Append("    \"Name\": \"" + lootTable.name + "\",\n");
            builder.Append("    \"Entries\":" + "\n");
            builder.Append("    {\n");
            var entries = lootTable.DebugGetLootChancePerObject().OrderByDescending(entry => entry.Value).ToList();
            foreach (var entry in entries)
            {
                builder.Append("       \"" + $"{entry.Key}".Replace("\"", "\\\"") + "\": "+ entry.Value);
                builder.Append(entry.Equals(lootTable.entries[^1]) ? "\n" : ",\n");
            }
            builder.Append("    }\n");
            builder.Append("  }");
            builder.Append(lootTable.Equals(lootTables[^1]) ? "\n" : ",\n");
        }
        builder.Append("]");
        Plugin.Logger.LogInfo(builder);
        */
        
        if (Plugin.Client.Connected()) return;
        LocalizationManager.TryGetTranslation("UnitNames/Amulet", out var translatedAmuletName);
        if (__instance.CurrentSpeakable.ActorName != translatedAmuletName) return;
        
        foreach (var button in __instance.playerDialogButtons)
        {
            var option = button.GetComponent<DialogOption>();
            const string translatePath = "Dialog/Amulet_PLA_WhereamIgoingnext";
            if (option.optionText.text != LocalizationManager.GetTranslation(translatePath)) continue;
            option.SetDialogOption($"Connect to Multi World {Plugin.Client.CurrentSessionConfigAsString()}.");
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate
            {
                Plugin.Client.Connect();
                __instance.OnSubtitlesRequest(Internal_OnSubtitlesRequestInfo.CurrentInfo);
            });
        }
    }
}