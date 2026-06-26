using System;
using System.Collections.Generic;
using Archipelago.Archipelago;
using HarmonyLib;
using PerfectRandom.Sulfur.Core;

namespace Archipelago.Patches;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.SwitchLevelRoutine))]
public class SwitchLevelRoutine
{
    private static readonly List<string> RomanNumeral = ["I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X"];

    private static bool _connectedOnFirstLoad = false;
    
    public static void Postfix(
        WorldEnvironmentIds chapterSO,
        int levelIndex
    )
    {
        if (!_connectedOnFirstLoad)
        {
            if (chapterSO == WorldEnvironmentIds.Onboarding)
                Plugin.Client.NewFileRequired = true;
            Plugin.Client.Connect();
            _connectedOnFirstLoad = true;
        }
        
        Plugin.Logger.LogInfo($"Reached {Enum.GetName(typeof(WorldEnvironmentIds), chapterSO)} stage {levelIndex}");
        string environment;
        switch (chapterSO)
        {
            case WorldEnvironmentIds.ChurchHub:
                environment = "The Church";
                break;
            case WorldEnvironmentIds.Act_01_Caves:
                environment = "Sulfur Caves";
                break;
            case WorldEnvironmentIds.Act_01_Shanty:
                environment = "Town";
                break;
            case WorldEnvironmentIds.Act_01_Sewers:
                environment = "Sewers";
                break;
            case WorldEnvironmentIds.Act_01_HedgemazeFromChurch:
            case WorldEnvironmentIds.Act_01_Hedgemaze:
                environment = "Hedge Maze";
                break;
            case WorldEnvironmentIds.Act_01_Dungeon:
                environment = "Dungeon";
                break;
            case WorldEnvironmentIds.Act_01_Castle:
                environment = "Castle";
                break;
            case WorldEnvironmentIds.Act_02_Forest:
                environment = "Forest";
                break;
            case WorldEnvironmentIds.Act_02_Bridge:
                environment = "Shav'Wani Bridge";
                break;
            case WorldEnvironmentIds.Act_02_Fortress:
                environment = "Shav'Wani Fortress";
                break;
            case WorldEnvironmentIds.Act_03_Canyon:
            case WorldEnvironmentIds.Act_03_Desert:
                environment = "Desert";
                break;
            case WorldEnvironmentIds.Act_03_EndChurch:
                environment = "Beyond the Veil";
                break;
            default:
                return;
        }
        Plugin.Logger.LogInfo($"Environment is {environment}");
        var locationName = environment is "The Church" or "Hedge Maze" or "Shav'Wani Bridge" or "Beyond the Veil" 
            ? $"Reach {environment}"
            : $"Reach {environment} {RomanNumeral[levelIndex]}";
        Plugin.Logger.LogInfo($"Completing location {locationName}");
        Plugin.Client.CompleteLocations(locationName);
    }
}